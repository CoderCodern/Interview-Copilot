import { render } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ThemeProvider } from "@/components/providers/theme-provider";
import { useThemeStore } from "@/stores/theme.store";
import type { ThemeState } from "@/stores/theme.store";

vi.mock("@/stores/theme.store");

const mockUseThemeStore = vi.mocked(useThemeStore);

/** ThemeProvider calls useThemeStore with a selector; mockImplementation forwards it. */
function mockTheme(theme: "light" | "dark") {
  const state: ThemeState = { theme, toggleTheme: vi.fn(), setTheme: vi.fn() };
  mockUseThemeStore.mockImplementation((sel?: (s: ThemeState) => unknown) =>
    sel ? sel(state) : (state as never),
  );
}

describe("ThemeProvider", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
    document.documentElement.className = "";
  });

  afterEach(() => {
    vi.useRealTimers();
    document.documentElement.className = "";
  });

  it("adds .dark class when theme is dark", () => {
    mockTheme("dark");
    render(<ThemeProvider>child</ThemeProvider>);
    expect(document.documentElement.classList.contains("dark")).toBe(true);
  });

  it("removes .dark class when theme is light", () => {
    document.documentElement.classList.add("dark");
    mockTheme("light");
    render(<ThemeProvider>child</ThemeProvider>);
    expect(document.documentElement.classList.contains("dark")).toBe(false);
  });

  it("adds .theme-switching on mount then removes it after 300ms", () => {
    mockTheme("light");
    render(<ThemeProvider>child</ThemeProvider>);
    expect(document.documentElement.classList.contains("theme-switching")).toBe(true);
    vi.advanceTimersByTime(300);
    expect(document.documentElement.classList.contains("theme-switching")).toBe(false);
  });

  it("renders children", () => {
    mockTheme("light");
    const { getByText } = render(<ThemeProvider><span>content</span></ThemeProvider>);
    expect(getByText("content")).toBeInTheDocument();
  });
});
