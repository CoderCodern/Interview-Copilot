"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";

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
  /** "overlay" sits transparently over the landing video; "solid" is white with a hairline border. */
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
        className={`fixed inset-x-0 top-0 z-10 flex items-center justify-between px-5 py-4 text-foreground sm:px-8 sm:py-5 ${
          variant === "solid" ? "border-b border-border bg-background" : "bg-transparent"
        }`}
      >
        <Link href="/" className="flex items-center gap-3" onClick={closeAll}>
          <span className="text-[21px] tracking-tight sm:text-[26px]" style={{ fontFamily: "var(--font-heading)" }}>
            Interview&nbsp;Copilot®
          </span>
          <span className="select-none text-[25px] sm:text-[30px]" style={{ letterSpacing: "-0.02em" }} aria-hidden="true">
            ✳︎
          </span>
        </Link>

        <div className="hidden items-center gap-7 text-[18px] md:flex lg:text-[20px]">
          {navItems.map((item) =>
            isLeaf(item) ? (
              <Link
                key={item.label}
                href={item.href}
                className={`transition-opacity hover:opacity-60 ${
                  isActive(item.href) ? "underline underline-offset-[6px]" : ""
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
                  className="inline-flex items-center gap-1.5 transition-opacity hover:opacity-60"
                  aria-expanded={prepareOpen}
                  aria-haspopup="true"
                >
                  {item.label}
                  <svg
                    width="12"
                    height="12"
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
                  className={`absolute left-0 top-full min-w-52 pt-3 transition-opacity duration-150 ${
                    prepareOpen ? "opacity-100" : "pointer-events-none opacity-0"
                  }`}
                >
                  <div className="overflow-hidden rounded-lg border border-border bg-background">
                    {item.children.map((child) => (
                      <Link
                        key={child.href}
                        href={child.href}
                        onClick={closeAll}
                        className={`block px-4 py-2.5 text-[15px] transition-colors hover:bg-surface-muted ${
                          isActive(child.href) ? "bg-surface-muted" : ""
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

        <Link
          href={cta.href}
          className="hidden text-[18px] underline underline-offset-2 transition-opacity hover:opacity-60 md:inline lg:text-[20px]"
        >
          {cta.label}
        </Link>

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
            >
              {item.label}
            </Link>
          ) : (
            <div key={item.label} className="flex flex-col gap-3">
              <span className="text-[15px] uppercase tracking-[0.14em] text-muted-foreground">{item.label}</span>
              {item.children.map((child) => (
                <Link
                  key={child.href}
                  href={child.href}
                  onClick={closeAll}
                  className="text-[26px] font-medium text-foreground"
                >
                  {child.label}
                </Link>
              ))}
            </div>
          ),
        )}
        <Link href={cta.href} onClick={closeAll} className="text-[32px] font-medium text-foreground underline">
          {cta.label}
        </Link>
      </div>
    </>
  );
}
