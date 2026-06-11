# Auth — Build & Handoff Notes

> Companion to [17-auth-architecture](17-auth-architecture.md) / [18-auth-implementation-plan](18-auth-implementation-plan.md). Covers the **backend P0–P2 implementation** delivered in this change: how to build, migrate, run, and what remains. The assistant environment has no .NET 10 SDK, so the code below was written to the repo's patterns but **must be built locally** (`dotnet build` / `dotnet test`).

---

## 1. What was implemented (email/password + sessions + profile + RBAC scaffolding)

**Domain** (`InterviewCopilot.Domain/Users/`): `UserProfile` aggregate (+ `CareerProfile`, `ResumeStatus`, `Preferences`/`NotificationSettings`, `OnboardingProgress`, `Theme`), `UserSession` aggregate with rotation + reuse-detection, `RefreshToken`, `ExternalLogin`, `AuthAuditLog`, `Roles`/`Permissions`/`AuthPolicies`, `UserEvents`. New IDs in `Common/Identifiers.cs`.

**Application** (`Features/Auth/*`, `Features/Profile/*`, `Abstractions/Identity.cs`): ports (`ITokenService`, `IIdentityService`, `IRefreshTokenService`, `IUserProfileRepository`, `IAuthAuditWriter`, `IEmailSender`, `IAuthLinkBuilder`) and slices: Register, VerifyEmail, ResendVerification, Login, RefreshSession, Logout, LogoutAll, ForgotPassword, ResetPassword, ChangePassword, ListSessions, RevokeSession, GetMe, UpdateProfile, CompleteOnboardingStep. Handlers/validators auto-register via the existing assembly scan.

**Infrastructure** (`Identity/*`, `Persistence/*`): `ApplicationUser`/`ApplicationRole`, `IdentityService` (UserManager/SignInManager), `TokenService` (RS256), `RsaSigningKeyProvider` (+ JWKS), `RefreshTokenService`, `LoggingEmailSender`, `AuthLinkBuilder`, `AuthAuditWriter`, `IdentitySeeder`, `AuthOptions`/`GoogleOAuthOptions`, EF configs, `UserProfileRepository`, `AppDbContext` now an `IdentityDbContext`, DI wiring.

**API** (`Authentication/*`, `Endpoints/*`): `JwtBearerSetup`, `RefreshCookie`, `AuthEndpoints`, `MeEndpoints`, `ResultExtensions` (auth codes), JWKS endpoint, role/policy registration, `Program.cs` rewired to first-party JWT in all environments.

**Tests** (`Domain.UnitTests/Users/`): `UserSessionTests` (rotation, reuse-detection, expiry, revoke), `UserProfileTests` (onboarding, preferences, resume status).

## 2. Build, migrate, run

```bash
cd backend
dotnet restore          # pulls Identity + IdentityModel packages added to CPM
dotnet build            # warnings-as-errors + analyzers must be clean

# Generate the additive migration (creates Identity + auth tables; resumes.owner_id is unchanged).
dotnet ef migrations add AddIdentityAndAuth \
  --project src/InterviewCopilot.Infrastructure \
  --startup-project src/InterviewCopilot.Api

dotnet ef database update \
  --project src/InterviewCopilot.Infrastructure \
  --startup-project src/InterviewCopilot.Api

dotnet run --project src/InterviewCopilot.Api      # Scalar UI at /scalar/v1, JWKS at /.well-known/jwks.json
dotnet test                                        # domain unit tests
```

There is **no `candidates` table** today (it was never an EF entity), so the migration is purely additive — it creates `users`, `roles`, `user_roles`, `user_profiles`, `user_sessions`, `refresh_tokens`, `external_logins`, `auth_audit_logs`, and the Identity support tables. `resumes.owner_id` remains a bare `uuid` that now logically references `users.id`.

## 3. Configuration & secrets

`appsettings.json` → `Auth` section (issuer, audience, token lifetimes, cookie name, web origin) and `Auth:Google`. In production, set via Secrets Manager / env (never commit):

- `Auth:SigningKeyPem` — RSA private key (PEM) for RS256. If absent, an **ephemeral** dev key is generated (logged warning; tokens die on restart — dev only).
- `Auth:IpHashSalt` — salt for hashing client IPs in audit rows.
- `Auth:Google:ClientId` / `ClientSecret` — for the P3 Google slices.
- `ConnectionStrings:Postgres` — via `dotnet user-secrets` locally.

## 4. Manual smoke test

```bash
# Register → check API logs for the [DEV EMAIL] verification link, copy userId+token
curl -sX POST localhost:8080/api/v1/auth/register -H 'content-type: application/json' \
  -d '{"email":"ada@example.com","password":"Correct-Horse-9","fullName":"Ada Lovelace"}'

curl -sX POST localhost:8080/api/v1/auth/verify-email -H 'content-type: application/json' \
  -d '{"userId":"<id>","token":"<token>"}'

# Login → returns accessToken + sets __Host-ic_refresh cookie
curl -sX POST localhost:8080/api/v1/auth/login -c jar.txt -H 'content-type: application/json' \
  -d '{"email":"ada@example.com","password":"Correct-Horse-9"}'

curl -s localhost:8080/api/v1/me -H "authorization: Bearer <accessToken>"
curl -sX POST localhost:8080/api/v1/auth/refresh -b jar.txt -c jar.txt   # rotates the cookie
```

## 5. Known caveats / deviations (intentional, documented)

- **Package versions** — `Microsoft.IdentityModel.JsonWebTokens`/`Tokens` pinned at `8.14.0` to match `JwtBearer 10.0.0`'s transitive IdentityModel line. If restore reports a conflict, align to the JwtBearer-resolved version.
- **`__Host-` cookie uses `Path=/`** (a hard requirement of the `__Host-` prefix), refining doc 17 §3.4 which suggested `/api/v1/auth`. Functionally equivalent; strictly more compatible.
- **Career/resume-status stored as `jsonb`** (not split columns as in doc 17 §5.2 DDL), following the repo's existing `ResumeProfile` precedent ("simpler/safer than column splitting for value-object records"). Promote to columns later if they need to be queried.
- **`DevAuthHandler` / `DevCurrentUser`** are now unused (real JWT in all envs). Safe to delete; left in place to avoid touching unrelated files.
- **`citext` extension** is declared; Identity's `NormalizedEmail` already enforces case-insensitive uniqueness.

## 6. Not yet implemented (next chunks per doc 18)

- **P3 Google OAuth** — `IGoogleOAuthClient` + `/auth/google/start|callback` + provisioning/linking slices.
- **Frontend** — the `(auth)` route group, forms, `AuthProvider`/token store, onboarding wizard (doc 18 §2–3).
- **Integration + E2E tests** — API journey on Testcontainers Postgres; Playwright flows (doc 18 §5 A11/B10).
- **Real email** — replace `LoggingEmailSender` with an Amazon SES sender (verified domain, SPF/DKIM/DMARC).
- **Resource-ownership `AuthorizationBehavior`** (doc 18 §5 B6) — tenant isolation is currently enforced by the EF global query filter; the explicit MediatR ownership behavior is the RBAC-chunk follow-up.
- **Outbox dispatch of auth domain events** (`UserRegistered`, etc.) — events are raised; wiring them through the outbox is the Doc 01 §5 follow-up.
- **Breached-password (HIBP) check** — policy hooks are present; the k-anonymity check itself is pending.
