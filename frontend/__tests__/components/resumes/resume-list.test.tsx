import { render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ResumeList } from "@/components/resumes/resume-list";
import { useResumes, type ResumeSummary } from "@/hooks/use-resumes";

vi.mock("@/hooks/use-sse", () => ({ useServerEvents: vi.fn() }));
vi.mock("@/hooks/use-resumes");
vi.mock("framer-motion", () => ({
  motion: {
    li: ({ children, className }: { children: ReactNode; className?: string }) => (
      <li className={className}>{children}</li>
    ),
  },
}));

const mockUseResumes = vi.mocked(useResumes);

function makeResume(id: string, status: ResumeSummary["status"] = "completed"): ResumeSummary {
  return { id, status, isCurrent: false, createdAt: "2026-06-01T12:00:00Z" };
}

describe("ResumeList", () => {
  beforeEach(() => vi.clearAllMocks());

  it("renders three skeleton rows while loading", () => {
    mockUseResumes.mockReturnValue({ data: undefined, isLoading: true, isError: false } as never);
    const { container } = render(<ResumeList />);
    expect(container.querySelectorAll(".animate-pulse")).toHaveLength(3);
    expect(screen.queryByText(/no resumes yet/i)).not.toBeInTheDocument();
  });

  it("renders an error message when the query fails", () => {
    mockUseResumes.mockReturnValue({ data: undefined, isLoading: false, isError: true } as never);
    render(<ResumeList />);
    expect(screen.getByText(/couldn't load your resumes/i)).toBeInTheDocument();
  });

  it("renders the empty state when the candidate has no resumes", () => {
    mockUseResumes.mockReturnValue({ data: [], isLoading: false, isError: false } as never);
    render(<ResumeList />);
    expect(screen.getByText(/no resumes yet/i)).toBeInTheDocument();
    expect(screen.getByText(/upload one to get started/i)).toBeInTheDocument();
  });

  it("renders one list item per resume", () => {
    const resumes = [
      makeResume("aaaaaaaa-0000-0000-0000-000000000001"),
      makeResume("bbbbbbbb-0000-0000-0000-000000000002"),
      makeResume("cccccccc-0000-0000-0000-000000000003"),
    ];
    mockUseResumes.mockReturnValue({ data: resumes, isLoading: false, isError: false } as never);
    render(<ResumeList />);
    expect(screen.getAllByRole("listitem")).toHaveLength(3);
    expect(screen.getByText("Resume aaaaaaaa")).toBeInTheDocument();
  });

  it("renders the status pill for each resume", () => {
    const resumes = [
      makeResume("aaaaaaaa-0000-0000-0000-000000000001", "pending"),
      makeResume("bbbbbbbb-0000-0000-0000-000000000002", "processing"),
      makeResume("cccccccc-0000-0000-0000-000000000003", "failed"),
    ];
    mockUseResumes.mockReturnValue({ data: resumes, isLoading: false, isError: false } as never);
    render(<ResumeList />);
    expect(screen.getByText("pending")).toBeInTheDocument();
    expect(screen.getByText("processing")).toBeInTheDocument();
    expect(screen.getByText("failed")).toBeInTheDocument();
  });

  it("applies interactive panel classes to each list item", () => {
    const resumes = [makeResume("aaaaaaaa-0000-0000-0000-000000000001")];
    mockUseResumes.mockReturnValue({ data: resumes, isLoading: false, isError: false } as never);
    const { container } = render(<ResumeList />);
    const item = container.querySelector("li");
    expect(item?.className).toContain("panel");
    expect(item?.className).toContain("panel-interactive");
  });
});
