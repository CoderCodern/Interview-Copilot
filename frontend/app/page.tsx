import Link from "next/link";

/** Marketing landing (Server Component, cacheable — Doc 06 §1). */
export default function Home() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-8 px-6 text-center">
      <span className="rounded-full border border-border bg-surface-muted px-3 py-1 text-sm text-muted-foreground">
        AI-powered interview preparation
      </span>
      <h1 className="text-5xl font-semibold tracking-tight">Walk in prepared.</h1>
      <p className="max-w-xl text-lg text-muted-foreground">
        Interview Copilot analyzes the company, your resume, and the job description to build a
        personalized, grounded prep plan — then practices with you in a scored mock interview.
      </p>
      <Link
        href="/dashboard"
        className="rounded-lg bg-primary px-6 py-3 font-medium text-primary-foreground transition-colors hover:opacity-90"
      >
        Get started
      </Link>
    </main>
  );
}
