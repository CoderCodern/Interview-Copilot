import Link from "next/link";
import { Building2, FileText, LayoutDashboard, MessagesSquare, Sparkles, Target } from "lucide-react";

const nav = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/companies", label: "Companies", icon: Building2 },
  { href: "/resumes", label: "Resumes", icon: FileText },
  { href: "/job-descriptions", label: "Job Descriptions", icon: Target },
  { href: "/preparations", label: "Preparations", icon: Sparkles },
  { href: "/mock", label: "Mock Interview", icon: MessagesSquare },
];

/** Authenticated app shell (Doc 06 §6, DESIGN.md §6). */
export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen bg-background">
      <aside className="hidden w-60 flex-col border-r border-border bg-surface px-3 py-5 md:flex">
        <Link href="/dashboard" className="mb-7 flex items-center gap-2.5 px-2">
          <span className="flex size-7 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Sparkles className="size-4" />
          </span>
          <span className="font-serif text-lg font-medium tracking-tight">Interview Copilot</span>
        </Link>
        <nav className="flex flex-col gap-0.5">
          {nav.map(({ href, label, icon: Icon }) => (
            <Link
              key={href}
              href={href}
              className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-surface-muted hover:text-foreground"
            >
              <Icon className="size-4" />
              {label}
            </Link>
          ))}
        </nav>
        <div className="mt-auto rounded-xl border border-border bg-surface-muted/60 p-3 text-xs leading-relaxed text-muted-foreground">
          <p className="font-medium text-foreground">Walk in prepared.</p>
          <p className="mt-1">Grounded, cited, personalized to your resume and target role.</p>
        </div>
      </aside>
      <main className="flex-1 px-6 py-8">
        <div className="mx-auto max-w-5xl">{children}</div>
      </main>
    </div>
  );
}
