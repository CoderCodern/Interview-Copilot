using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Provides the RSA key used to sign (and publish via JWKS) access tokens (Doc 17 §3.2).
/// In production the private key PEM comes from Secrets Manager; in its absence an ephemeral
/// in-memory key is generated so local dev works (with a loud warning — tokens won't survive
/// a restart and must never be used in production).
/// </summary>
public sealed partial class RsaSigningKeyProvider : IDisposable
{
    private readonly RSA _rsa;

    public RsaSigningKeyProvider(IOptions<AuthOptions> options, ILogger<RsaSigningKeyProvider> logger)
    {
        var opts = options.Value;
        _rsa = RSA.Create(2048);

        if (!string.IsNullOrWhiteSpace(opts.SigningKeyPem))
        {
            _rsa.ImportFromPem(opts.SigningKeyPem);
        }
        else
        {
            LogEphemeralKey(logger);
        }

        var securityKey = new RsaSecurityKey(_rsa) { KeyId = opts.SigningKeyId };
        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        PublicKey = securityKey;
    }

    public SigningCredentials SigningCredentials { get; }

    /// <summary>Public key for token validation / JWKS export.</summary>
    public RsaSecurityKey PublicKey { get; }

    /// <summary>The public JWKS document served at <c>/.well-known/jwks.json</c> (Doc 17 §3.2).</summary>
    public string PublicJwksJson()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(PublicKey);
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;
        return System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { jwk } });
    }

    public void Dispose() => _rsa.Dispose();

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Auth:SigningKeyPem is not configured — using an EPHEMERAL RSA key. " +
                  "Tokens will be invalidated on restart. Configure a key from Secrets Manager for production.")]
    private static partial void LogEphemeralKey(ILogger logger);
}
