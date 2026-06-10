import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import ResumesPage from "@/app/(app)/resumes/page";

vi.mock("@/components/resumes/resume-list", () => ({
  ResumeList: () => <div data-testid="resume-list" />,
}));

describe("ResumesPage", () => {
  it("renders the eyebrow label", () => {
    render(<ResumesPage />);
    expect(screen.getByText("Library")).toBeInTheDocument();
  });

  it("renders the Resumes heading", () => {
    render(<ResumesPage />);
    expect(screen.getByRole("heading", { level: 1, name: /resumes/i })).toBeInTheDocument();
  });

  it("renders the upload button", () => {
    render(<ResumesPage />);
    const btn = screen.getByRole("button", { name: /upload resume/i });
    expect(btn).toBeInTheDocument();
    expect(btn).toHaveClass("btn-primary");
  });

  it("renders the ResumeList component", () => {
    render(<ResumesPage />);
    expect(screen.getByTestId("resume-list")).toBeInTheDocument();
  });
});
