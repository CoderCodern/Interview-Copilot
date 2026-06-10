"use client";

import { useEffect } from "react";
import { useThemeStore } from "@/stores/theme.store";

/**
 * Syncs the Zustand theme state to the `.dark` class on <html>.
 * Wraps the toggle in a `.theme-switching` class for 300ms so the CSS
 * transition rule fires only during the switch — not on every hover.
 */
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const theme = useThemeStore((s) => s.theme);

  useEffect(() => {
    const root = document.documentElement;
    root.classList.add("theme-switching");
    root.classList.toggle("dark", theme === "dark");
    const t = setTimeout(() => root.classList.remove("theme-switching"), 300);
    return () => clearTimeout(t);
  }, [theme]);

  return <>{children}</>;
}
