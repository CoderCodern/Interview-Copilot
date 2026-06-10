import Link from "next/link";
import { ArrowRight, Building2, FileText, Sparkles, Target } from "lucide-react";

const steps = [
  { href: "/resumes", title: "Analyze your resume", desc: "Upload a PDF, DOCX, or image — we extract skills, experience, and projects.", icon: FileText },
  { href: "/companies", title: "Research the company", desc: "Paste a URL or document for culture, hiring process, and likely questions.", icon: Building2 },
  { href: "/job-descriptions", title: "Decode the JD", desc: "Required skills, keywords, technologies, and your gaps.", icon: Target },
  { href: "/preparations", title: "Build your prep plan", desc: "Personalized questions, STAR answers, tips, and a roadmap.", icon: Sparkles },
];

export default function DashboardPage() {
  return (
    <section>
      <p className="eyebrow">Getting started</p>
      <h1 className="mt-2 text-[clamp(28px,4.5vw,38px)]">
        Welcome back<span className="italic text-accent-deep">.</span>
      </h1>
      <p className="mt-2 max-w-lg text-[15px] text-muted-foreground">Four steps to walk in prepared.</p>

      <div className="mt-9 grid gap-4 sm:grid-cols-2">
        {steps.map(({ href, title, desc, icon: Icon }, i) => (
          <Link
            key={href}
            href={href}
            className="panel panel-interactive group flex flex-col p-6"
          >
            <div className="flex items-start justify-between">
              <span
                className="grid size-9 place-items-center rounded-[10px] border bg-accent-soft transition-shadow group-hover:shadow-[0_0_0_4px_var(--accent-softer)]"
                style={{ borderColor: "var(--accent-ring)" }}
              >
                <Icon className="size-[17px] text-accent" strokeWidth={1.8} />
              </span>
              <span
                className="text-[19px] tabular-nums text-faint-foreground"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                {String(i + 1).padStart(2, "0")}
              </span>
            </div>
            <h2 className="mt-5 text-[19px]">{title}</h2>
            <p className="mt-1.5 flex-1 text-[13.5px] leading-relaxed text-muted-foreground">{desc}</p>
            <span className="mt-5 inline-flex -translate-x-1 items-center gap-1 text-[13px] font-medium text-accent opacity-0 transition-all duration-200 group-hover:translate-x-0 group-hover:opacity-100">
              Open
              <ArrowRight className="size-3.5" strokeWidth={2} />
            </span>
          </Link>
        ))}
      </div>

      <div className="mt-8 flex flex-wrap items-center gap-3">
        <Link href="/mock" className="btn btn-primary">
          Start a mock interview
          <ArrowRight className="size-4" strokeWidth={2} />
        </Link>
        <Link href="/preparations" className="btn btn-ghost">
          View prep plans
        </Link>
      </div>
    </section>
  );
}
