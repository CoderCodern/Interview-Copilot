import Link from "next/link";
import { Building2, FileText, Sparkles, Target } from "lucide-react";

const steps = [
  { href: "/resumes", title: "Analyze your resume", desc: "Upload a PDF, DOCX, or image — we extract skills, experience, and projects.", icon: FileText },
  { href: "/companies", title: "Research the company", desc: "Paste a URL or document for culture, hiring process, and likely questions.", icon: Building2 },
  { href: "/job-descriptions", title: "Decode the JD", desc: "Required skills, keywords, technologies, and your gaps.", icon: Target },
  { href: "/preparations", title: "Build your prep plan", desc: "Personalized questions, STAR answers, tips, and a roadmap.", icon: Sparkles },
];

export default function DashboardPage() {
  return (
    <section>
      <h1 className="text-3xl font-semibold tracking-tight">Welcome back</h1>
      <p className="mt-2 text-muted-foreground">Four steps to walk in prepared.</p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2">
        {steps.map(({ href, title, desc, icon: Icon }) => (
          <Link
            key={href}
            href={href}
            className="group rounded-xl border border-border bg-surface p-5 transition-colors hover:border-primary"
          >
            <Icon className="size-6 text-primary" />
            <h2 className="mt-3 font-medium">{title}</h2>
            <p className="mt-1 text-sm text-muted-foreground">{desc}</p>
          </Link>
        ))}
      </div>
    </section>
  );
}
