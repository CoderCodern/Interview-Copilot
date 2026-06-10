"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { ThemeToggle } from "@/components/shared/theme-toggle";

type NavLeaf = { label: string; href: string };
type NavItem = NavLeaf | { label: string; children: NavLeaf[] };

const navItems: NavItem[] = [
  { label: "Dashboard", href: "/dashboard" },
  {
    label: "Prepare",
    children: [
      { label: "Companies", href: "/companies" },
      { label: "Resumes", href: "/resumes" },
      { label: "Job Descriptions", href: "/job-descriptions" },
      { label: "Preparations", href: "/preparations" },
    ],
  },
  { label: "Mock Interview", href: "/mock" },
];

interface SiteNavProps {
  /** "overlay" sits transparently over the landing video; "solid" is the warm app navbar. */
  variant?: "overlay" | "solid";
  cta?: NavLeaf;
}

function isLeaf(item: NavItem): item is NavLeaf {
  return "href" in item;
}

export function SiteNav({ variant = "solid", cta = { label: "Get started", href: "/dashboard" } }: SiteNavProps) {
  const pathname = usePathname();
  const [menuOpen, setMenuOpen] = useState(false);
  const [prepareOpen, setPrepareOpen] = useState(false);

  const isActive = (href: string) => pathname === href || pathname.startsWith(`${href}/`);
  const closeAll = () => {
    setMenuOpen(false);
    setPrepareOpen(false);
  };

  return (
    <>
      <nav
        className={`fixed inset-x-0 top-0 z-10 flex items-center justify-between px-5 py-3.5 text-foreground sm:px-8 ${
          variant === "solid" ? "border-b border-border bg-background/90 backdrop-blur-sm" : "bg-transparent"
        }`}
      >
        <Link href="/" className="flex items-center gap-2.5" onClick={closeAll}>
          <span
            className="grid size-8 place-items-center rounded-[9px] text-[17px] italic text-primary-foreground shadow-sm"
            style={{
              fontFamily: "var(--font-heading)",
              background: "linear-gradient(160deg, var(--accent), var(--accent-deep))",
            }}
            aria-hidden="true"
          >
            ic
          </span>
          <span
            className="text-[19px] font-semibold tracking-[-0.01em]"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Interview&nbsp;Copilot
          </span>
        </Link>

        <div className="hidden items-center gap-1 text-[13.5px] md:flex">
          {navItems.map((item) =>
            isLeaf(item) ? (
              <Link
                key={item.label}
                href={item.href}
                className={`rounded-lg px-3 py-1.5 transition-colors ${
                  isActive(item.href)
                    ? "bg-accent-soft font-medium text-accent"
                    : "text-muted-foreground hover:bg-hover hover:text-foreground"
                }`}
              >
                {item.label}
              </Link>
            ) : (
              <div
                key={item.label}
                className="relative"
                onMouseEnter={() => setPrepareOpen(true)}
                onMouseLeave={() => setPrepareOpen(false)}
              >
                <button
                  type="button"
                  onClick={() => setPrepareOpen((o) => !o)}
                  className="inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-muted-foreground transition-colors hover:bg-hover hover:text-foreground"
                  aria-expanded={prepareOpen}
                  aria-haspopup="true"
                >
                  {item.label}
                  <svg
                    width="11"
                    height="11"
                    viewBox="0 0 12 12"
                    className={`transition-transform duration-200 ${prepareOpen ? "rotate-180" : ""}`}
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    aria-hidden="true"
                  >
                    <path d="m3 4.5 3 3 3-3" strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                </button>
                <div
                  className={`absolute left-0 top-full min-w-52 pt-2 transition-opacity duration-150 ${
                    prepareOpen ? "opacity-100" : "pointer-events-none opacity-0"
                  }`}
                >
                  <div
                    className="overflow-hidden rounded-xl border border-border bg-surface-raised p-1.5"
                    style={{ boxShadow: "var(--shadow-md)" }}
                  >
                    {item.children.map((child) => (
                      <Link
                        key={child.href}
                        href={child.href}
                        onClick={closeAll}
                        className={`relative block rounded-lg px-3 py-2 text-[13.5px] transition-colors ${
                          isActive(child.href)
                            ? "bg-accent-soft font-medium text-accent"
                            : "text-muted-foreground hover:bg-hover hover:text-foreground"
                        }`}
                      >
                        {child.label}
                      </Link>
                    ))}
                  </div>
                </div>
              </div>
            ),
          )}
        </div>

        <div className="hidden items-center gap-3 md:flex">
          <ThemeToggle />
          <Link
            href={cta.href}
            className="inline-flex items-center rounded-[9px] px-4 py-1.5 text-[13.5px] font-medium text-primary-foreground shadow-sm transition-all duration-150 hover:-translate-y-px hover:shadow-md active:translate-y-0"
            style={{ background: "linear-gradient(180deg, var(--accent), var(--accent-deep))" }}
          >
            {cta.label}
          </Link>
        </div>

        <button
          type="button"
          onClick={() => setMenuOpen((o) => !o)}
          className="flex flex-col gap-[5px] md:hidden"
          aria-label={menuOpen ? "Close menu" : "Open menu"}
          aria-expanded={menuOpen}
        >
          <span className={`h-[2px] w-6 bg-foreground transition-all duration-300 ${menuOpen ? "translate-y-[7px] rotate-45" : ""}`} />
          <span className={`h-[2px] w-6 bg-foreground transition-all duration-300 ${menuOpen ? "opacity-0" : ""}`} />
          <span className={`h-[2px] w-6 bg-foreground transition-all duration-300 ${menuOpen ? "-translate-y-[7px] -rotate-45" : ""}`} />
        </button>
      </nav>

      {/* Mobile overlay */}
      <div
        className={`fixed inset-0 z-[9] flex flex-col justify-center gap-7 bg-background/95 px-8 backdrop-blur-sm transition-opacity duration-300 md:hidden ${
          menuOpen ? "opacity-100" : "pointer-events-none opacity-0"
        }`}
      >
        {navItems.map((item) =>
          isLeaf(item) ? (
            <Link
              key={item.label}
              href={item.href}
              onClick={closeAll}
              className="text-[32px] font-medium text-foreground"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              {item.label}
            </Link>
          ) : (
            <div key={item.label} className="flex flex-col gap-3">
              <span className="text-[12px] font-semibold uppercase tracking-[0.1em] text-accent">{item.label}</span>
              {item.children.map((child) => (
                <Link
                  key={child.href}
                  href={child.href}
                  onClick={closeAll}
                  className="text-[26px] font-medium text-foreground"
                  style={{ fontFamily: "var(--font-heading)" }}
                >
                  {child.label}
                </Link>
              ))}
            </div>
          ),
        )}
        <Link
          href={cta.href}
          onClick={closeAll}
          className="text-[32px] font-medium italic text-accent"
          style={{ fontFamily: "var(--font-heading)" }}
        >
          {cta.label}
        </Link>
      </div>
    </>
  );
}
