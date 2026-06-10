import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { SiteNav } from "@/components/layout/site-nav";

vi.mock("next/link", () => ({
  default: ({
    href,
    children,
    className,
    onClick,
  }: {
    href: string;
    children: React.ReactNode;
    className?: string;
    onClick?: () => void;
  }) => (
    <a href={href} className={className} onClick={onClick}>
      {children}
    </a>
  ),
}));

vi.mock("next/navigation", () => ({
  usePathname: vi.fn(() => "/dashboard"),
}));

const { usePathname } = await import("next/navigation");
const mockUsePathname = vi.mocked(usePathname);

/** The desktop links live inside the <nav>; mobile overlay is a sibling <div>. */
function getDesktopNav() {
  return screen.getByRole("navigation");
}

describe("SiteNav — active state (desktop nav)", () => {
  it("marks Dashboard active when pathname is /dashboard", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    const link = within(getDesktopNav()).getByRole("link", { name: /^dashboard$/i });
    expect(link.className).toContain("bg-accent-soft");
  });

  it("does not mark Mock Interview active when pathname is /dashboard", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    const link = within(getDesktopNav()).getByRole("link", { name: /mock interview/i });
    expect(link.className).not.toContain("bg-accent-soft");
  });

  it("marks Mock Interview active when pathname is /mock", () => {
    mockUsePathname.mockReturnValue("/mock");
    render(<SiteNav />);
    const link = within(getDesktopNav()).getByRole("link", { name: /mock interview/i });
    expect(link.className).toContain("bg-accent-soft");
  });

  it("marks a child route active when pathname starts with its prefix", () => {
    mockUsePathname.mockReturnValue("/resumes/some-id");
    render(<SiteNav />);
    // Open dropdown so child links are inspectable
    fireEvent.click(screen.getByRole("button", { name: /prepare/i }));
    const nav = getDesktopNav();
    const links = within(nav).getAllByRole("link", { name: /^resumes$/i });
    expect(links.some((l) => l.className.includes("bg-accent-soft"))).toBe(true);
  });
});

describe("SiteNav — Prepare dropdown", () => {
  it("Prepare button starts with aria-expanded false", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    expect(screen.getByRole("button", { name: /prepare/i })).toHaveAttribute(
      "aria-expanded",
      "false",
    );
  });

  it("Prepare button sets aria-expanded true after click", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    const btn = screen.getByRole("button", { name: /prepare/i });
    fireEvent.click(btn);
    expect(btn).toHaveAttribute("aria-expanded", "true");
  });

  it("dropdown child links are present in the desktop nav", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    fireEvent.click(screen.getByRole("button", { name: /prepare/i }));
    const nav = getDesktopNav();
    expect(within(nav).getByRole("link", { name: /companies/i })).toBeInTheDocument();
    expect(within(nav).getAllByRole("link", { name: /^resumes$/i })[0]).toBeInTheDocument();
    expect(within(nav).getByRole("link", { name: /job descriptions/i })).toBeInTheDocument();
    expect(within(nav).getAllByRole("link", { name: /preparations/i })[0]).toBeInTheDocument();
  });
});

describe("SiteNav — variant", () => {
  it("solid variant has border-b class", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    const { container } = render(<SiteNav variant="solid" />);
    expect(container.querySelector("nav")?.className).toContain("border-b");
  });

  it("overlay variant has bg-transparent class", () => {
    mockUsePathname.mockReturnValue("/");
    const { container } = render(<SiteNav variant="overlay" />);
    expect(container.querySelector("nav")?.className).toContain("bg-transparent");
  });
});

describe("SiteNav — mobile menu", () => {
  it("hamburger button starts with aria-expanded false", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    expect(screen.getByRole("button", { name: /open menu/i })).toHaveAttribute(
      "aria-expanded",
      "false",
    );
  });

  it("hamburger button sets aria-expanded true after click", () => {
    mockUsePathname.mockReturnValue("/dashboard");
    render(<SiteNav />);
    const btn = screen.getByRole("button", { name: /open menu/i });
    fireEvent.click(btn);
    expect(btn).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByRole("button", { name: /close menu/i })).toBeInTheDocument();
  });
});
