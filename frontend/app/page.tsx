import Link from "next/link";
import { ArrowRight, Building2, FileText, Sparkles, Target } from "lucide-react";

const pillars = [
  { icon: Building2, label: "Company analysis" },
  { icon: FileText, label: "Resume parsing" },
  { icon: Target, label: "JD gap-finder" },
  { icon: Sparkles, label: "Scored mock interview" },
];

/** Marketing landing (Server Component, cacheable — Doc 06 §1). */
export default function Home() {
  return (
    <main className="relative mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-8 px-6 py-20 text-center">
      <span className="inline-flex items-center gap-2 rounded-full border border-border bg-surface px-3.5 py-1.5 text-sm text-muted-foreground shadow-[0_0_0_1px_rgba(0,0,0,0.02)]">
        <Sparkles className="size-3.5 text-primary" />
        AI-powered interview preparation
      </span>

      <h1 className="font-serif text-5xl font-medium leading-[1.08] tracking-tight text-foreground sm:text-6xl">
        Walk in prepared.
      </h1>

      <p className="max-w-xl text-lg leading-relaxed text-muted-foreground">
        Interview Copilot studies the company, your resume, and the job description, then
        builds a grounded, personalized prep plan — and practices with you in a scored mock
        interview.
      </p>

      <div className="flex flex-col items-center gap-3 sm:flex-row">
        <Link
          href="/dashboard"
          className="group inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 font-medium text-primary-foreground transition-colors hover:bg-primary-hover"
        >
          Get started
          <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
        </Link>
        <Link
          href="/dashboard"
          className="inline-flex items-center gap-2 rounded-xl border border-border bg-surface-muted px-6 py-3 font-medium text-foreground transition-colors hover:bg-surface"
        >
          See how it works
        </Link>
      </div>

      <ul className="mt-6 flex flex-wrap items-center justify-center gap-2.5">
        {pillars.map(({ icon: Icon, label }) => (
          <li
            key={label}
            className="inline-flex items-center gap-2 rounded-full border border-border bg-surface px-3.5 py-1.5 text-sm text-muted-foreground"
          >
            <Icon className="size-4 text-primary" />
            {label}
          </li>
        ))}
      </ul>
    </main>
  );
}
