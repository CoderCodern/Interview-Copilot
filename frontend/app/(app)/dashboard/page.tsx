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
      <p className="text-[13px] uppercase tracking-[0.14em] text-muted-foreground">Getting started</p>
      <h1 className="mt-2 text-[clamp(30px,5vw,44px)] leading-[1.05] tracking-tight">Welcome back</h1>
      <p className="mt-3 max-w-lg text-[17px] text-muted-foreground">Four steps to walk in prepared.</p>

      <div className="mt-10 grid gap-px overflow-hidden rounded-lg border border-border bg-border sm:grid-cols-2">
        {steps.map(({ href, title, desc, icon: Icon }, i) => (
          <Link
            key={href}
            href={href}
            className="group flex flex-col bg-background p-6 transition-colors hover:bg-surface-muted sm:p-7"
          >
            <div className="flex items-start justify-between">
              <Icon className="size-6" strokeWidth={1.5} />
              <span className="text-[13px] tabular-nums text-muted-foreground">{String(i + 1).padStart(2, "0")}</span>
            </div>
            <h2 className="mt-6 text-[21px]">{title}</h2>
            <p className="mt-2 text-[15px] leading-relaxed text-muted-foreground">{desc}</p>
            <span className="mt-5 inline-flex items-center gap-1 text-[14px] opacity-0 transition-opacity group-hover:opacity-100">
              Open
              <ArrowUpRight className="size-4" strokeWidth={1.5} />
            </span>
          </Link>
        ))}
      </div>

      <div className="mt-8 flex flex-wrap gap-y-1">
        <Link
          href="/mock"
          className="mx-[0.2em] mb-[0.4em] inline-flex items-center justify-center whitespace-nowrap rounded-full border border-foreground bg-foreground px-5 py-[0.4em] text-[15px] text-background transition-colors duration-200 hover:bg-background hover:text-foreground"
        >
          Start a mock interview
        </Link>
        <Link
          href="/preparations"
          className="mx-[0.2em] mb-[0.4em] inline-flex items-center justify-center whitespace-nowrap rounded-full border border-black/10 bg-background px-5 py-[0.4em] text-[15px] transition-colors duration-200 hover:bg-foreground hover:text-background"
        >
          View prep plans
        </Link>
      </div>
    </section>
  );
}
