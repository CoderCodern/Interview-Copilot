import { create } from "zustand";

/**
 * Ephemeral UI state only. Never cache server data here — that belongs to TanStack
 * Query (Doc 06 §3).
 */
interface UiState {
  sidebarOpen: boolean;
  commandPaletteOpen: boolean;
  toggleSidebar: () => void;
  setCommandPalette: (open: boolean) => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarOpen: true,
  commandPaletteOpen: false,
  toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),
  setCommandPalette: (open) => set({ commandPaletteOpen: open }),
}));
