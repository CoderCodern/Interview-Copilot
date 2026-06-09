import { SiteNav } from "@/components/layout/site-nav";

/** Authenticated app shell — shared top navbar (solid) over a white canvas (DESIGN.md §6). */
export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-background">
      <SiteNav variant="solid" cta={{ label: "Account", href: "/dashboard" }} />
      <main className="px-5 pb-16 pt-24 sm:px-8 md:px-10">
        <div className="mx-auto max-w-5xl">{children}</div>
      </main>
    </div>
  );
}
