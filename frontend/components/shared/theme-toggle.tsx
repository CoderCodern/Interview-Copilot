"use client";

import { Moon, Sun } from "lucide-react";
import { useThemeStore } from "@/stores/theme.store";

export function ThemeToggle() {
  const { theme, toggleTheme } = useThemeStore();
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label={`Switch to ${isDark ? "light" : "dark"} mode`}
      className="flex size-8 items-center justify-center rounded-full text-foreground transition-colors hover:bg-surface-muted"
    >
      {isDark ? (
        <Sun className="size-[18px]" strokeWidth={1.5} aria-hidden="true" />
      ) : (
        <Moon className="size-[18px]" strokeWidth={1.5} aria-hidden="true" />
      )}
    </button>
  );
}
