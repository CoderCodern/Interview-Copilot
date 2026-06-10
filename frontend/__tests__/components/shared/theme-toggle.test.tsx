import { render, screen, fireEvent } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ThemeToggle } from "@/components/shared/theme-toggle";
import { useThemeStore } from "@/stores/theme.store";

vi.mock("@/stores/theme.store");

const mockUseThemeStore = vi.mocked(useThemeStore);

describe("ThemeToggle", () => {
  beforeEach(() => vi.clearAllMocks());

  it("shows Moon icon and correct aria-label in light mode", () => {
    mockUseThemeStore.mockReturnValue({ theme: "light", toggleTheme: vi.fn(), setTheme: vi.fn() });
    render(<ThemeToggle />);
    expect(screen.getByRole("button", { name: /switch to dark mode/i })).toBeInTheDocument();
  });

  it("shows Sun icon and correct aria-label in dark mode", () => {
    mockUseThemeStore.mockReturnValue({ theme: "dark", toggleTheme: vi.fn(), setTheme: vi.fn() });
    render(<ThemeToggle />);
    expect(screen.getByRole("button", { name: /switch to light mode/i })).toBeInTheDocument();
  });

  it("calls toggleTheme when clicked", () => {
    const toggleTheme = vi.fn();
    mockUseThemeStore.mockReturnValue({ theme: "light", toggleTheme, setTheme: vi.fn() });
    render(<ThemeToggle />);
    fireEvent.click(screen.getByRole("button"));
    expect(toggleTheme).toHaveBeenCalledOnce();
  });
});
