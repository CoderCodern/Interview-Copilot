import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import DashboardPage from "@/app/(app)/dashboard/page";

vi.mock("next/link", () => ({
  default: ({ href, children, className }: { href: string; children: React.ReactNode; className?: string }) => (
    <a href={href} className={className}>
      {children}
    </a>
  ),
}));

describe("DashboardPage", () => {
  it("renders the eyebrow label", () => {
    render(<DashboardPage />);
    expect(screen.getByText("Getting started")).toBeInTheDocument();
  });

  it("renders the heading", () => {
    render(<DashboardPage />);
    expect(screen.getByRole("heading", { level: 1 })).toBeInTheDocument();
  });

  it("renders all four step cards", () => {
    render(<DashboardPage />);
    expect(screen.getByText("Analyze your resume")).toBeInTheDocument();
    expect(screen.getByText("Research the company")).toBeInTheDocument();
    expect(screen.getByText("Decode the JD")).toBeInTheDocument();
    expect(screen.getByText("Build your prep plan")).toBeInTheDocument();
  });

  it("links each card to the correct route", () => {
    render(<DashboardPage />);
    const resumeLink = screen.getByRole("link", { name: /analyze your resume/i });
    expect(resumeLink).toHaveAttribute("href", "/resumes");
    const companyLink = screen.getByRole("link", { name: /research the company/i });
    expect(companyLink).toHaveAttribute("href", "/companies");
  });

  it("renders the mock interview CTA linking to /mock", () => {
    render(<DashboardPage />);
    const cta = screen.getByRole("link", { name: /start a mock interview/i });
    expect(cta).toHaveAttribute("href", "/mock");
    expect(cta).toHaveClass("btn-primary");
  });

  it("renders the prep plans link linking to /preparations", () => {
    render(<DashboardPage />);
    const link = screen.getByRole("link", { name: /view prep plans/i });
    expect(link).toHaveAttribute("href", "/preparations");
    expect(link).toHaveClass("btn-ghost");
  });
});
