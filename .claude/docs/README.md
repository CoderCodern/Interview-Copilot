# Claude Code Workspace Guide

This `.claude/` workspace turns the repository into an AI-assisted engineering environment. It is consumed by Claude Code automatically.

## Layout

```
.claude/
├── CLAUDE.md            # canonical engineering memory (imported by root CLAUDE.md)
├── settings.json        # hooks wiring + permission allow/deny
├── skills/<name>/SKILL.md   # reusable workflows (code-review, review, update-test, documentation, deploy-checklist, learn)
├── agents/<name>.md     # subagents (architect, backend, frontend, reviewer, tester, devops, ai-engineer)
├── hooks/*.sh           # PreToolUse, PostToolUse, SessionStart, Stop scripts
├── commands/*.md        # slash commands mapping to skills (/code-review, /review, /update-test, /document, /deploy-checklist, /learn)
├── docs/                # this guide, repository assessment, and ADRs
└── mcp/                 # MCP config templates + setup/security notes
```

## How it fits together

- **Memory:** root `CLAUDE.md` imports `.claude/CLAUDE.md` so standards load every session.
- **Hooks** (configured in `settings.json`):
  - *SessionStart* injects orientation + active ADRs.
  - *PreToolUse* blocks dangerous shell ops and Domain/Application dependency-rule violations.
  - *PostToolUse* reminds about tests/docs after code edits.
  - *Stop* prints a wrap-up checklist.
- **Commands** invoke **skills**; complex work is delegated to **subagents**.
- **MCP** templates are opt-in (copy to root `.mcp.json`).

## Typical loop

1. `architect` (if design needed) -> 2. `backend`/`frontend` implement a vertical slice ->
3. `/update-test` (or `tester`) -> 4. `/code-review` (or `reviewer`) ->
5. `/document` if architecture/contracts changed -> 6. `/deploy-checklist` before release.
Periodically run `/learn` to propose codifying new patterns (approval required).

## Customizing
Edit `.claude/CLAUDE.md` for standards, add skills under `.claude/skills/`, agents under `.claude/agents/`, and tune `settings.json` permissions/hooks. Keep ADRs current in `.claude/docs/adr/`.
