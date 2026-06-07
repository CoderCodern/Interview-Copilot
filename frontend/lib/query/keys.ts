/** Centralized, typed query keys so cache invalidation is precise (Doc 06 §3). */
export const queryKeys = {
  resumes: {
    all: ["resumes"] as const,
    list: () => [...queryKeys.resumes.all, "list"] as const,
    detail: (id: string) => [...queryKeys.resumes.all, "detail", id] as const,
  },
  companies: {
    all: ["companies"] as const,
    detail: (id: string) => [...queryKeys.companies.all, "detail", id] as const,
  },
  preparations: {
    all: ["preparations"] as const,
    detail: (id: string) => [...queryKeys.preparations.all, "detail", id] as const,
  },
  usage: { current: ["usage", "current"] as const },
} as const;
