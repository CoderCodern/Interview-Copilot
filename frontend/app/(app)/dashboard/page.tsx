import Link from "next/link";
import { ArrowUpRight, Building2, FileText, Sparkles, Target } from "lucide-react";

const steps = [
  { href: "/resumes", title: "Analyze your resume", desc: "Upload a PDF, DOCX, or image — we extract skills, experience, and projects.", icon: FileText },
  { href: "/companies", title: "Research the company", desc: "Paste a URL or document for culture, hiring process, and likely questions.", icon: Building2 },
  { href: "/job-descriptions", title: "Decode the JD", desc: "Required skills, keywords, technologies, and your gaps.", icon: Target },
  { href: "/preparations", title: "Build your prep plan", desc: "Personalized questions, STAR answers, tips, and a roadmap.", icon: Sparkles },
];

export default function DashboardPage() {
  return (
    <section>
      <p className="text-sm font-medium uppercase tracking-[0.12em] text-primary">Getting started</p>
      <h1 className="mt-2 text-3xl tracking-tight">Welcome back</h1>
      <p className="mt-2 text-muted-foreground">Four steps to walk in prepared.</p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2">
        {steps.map(({ href, title, desc, icon: Icon }, i) => (
          <Link
            key={href}
            href={href}
            className="group relative rounded-2xl border border-border bg-surface p-6 transition-all hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-[0_4px_24px_rgba(0,0,0,0.05)]"
          >
            <div className="flex items-start justify-between">
              <span className="flex size-10 items-center justify-center rounded-xl bg-surface-muted text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
                <Icon className="size-5" />
              </span>
              <span className="text-xs font-medium tabular-nums text-muted-foreground">
                {String(i + 1).padStart(2, "0")}
              </span>
            </div>
            <h2 className="mt-4 text-xl">{title}</h2>
            <p className="mt-1.5 text-sm leading-relaxed text-muted-foreground">{desc}</p>
            <span className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary opacity-0 transition-opacity group-hover:opacity-100">
              Open
              <ArrowUpRight className="size-4" />
            </span>
          </Link>
        ))}
      </div>
    </section>
  );
}
