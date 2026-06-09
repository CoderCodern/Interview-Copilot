"use client";

import { motion } from "framer-motion";
import { FileText } from "lucide-react";
import { useResumes } from "@/hooks/use-resumes";
import { useServerEvents } from "@/hooks/use-sse";
import { StatusPill } from "@/components/shared/status-pill";

/**
 * Client component: TanStack Query for data, SSE for live status, Framer Motion for
 * entrance — the three frontend pillars in one small surface (Doc 06).
 */
export function ResumeList() {
  useServerEvents();
  const { data, isLoading, isError } = useResumes();

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[0, 1, 2].map((i) => (
          <div key={i} className="h-16 animate-pulse rounded-lg border border-border bg-surface-muted" />
        ))}
      </div>
    );
  }

  if (isError) {
    return <p className="text-sm text-danger">Couldn&apos;t load your resumes. Please retry.</p>;
  }

  if (!data?.length) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-background p-12 text-center">
        <span className="mx-auto flex size-12 items-center justify-center rounded-lg border border-border">
          <FileText className="size-6" strokeWidth={1.5} />
        </span>
        <p className="mt-4 text-[20px]">No resumes yet</p>
        <p className="mt-1 text-sm text-muted-foreground">Upload one to get started.</p>
      </div>
    );
  }

  return (
    <ul className="space-y-3">
      {data.map((r, i) => (
        <motion.li
          key={r.id}
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.18, delay: Math.min(i * 0.03, 0.24) }}
          className="flex items-center justify-between rounded-lg border border-border bg-background p-4 transition-colors hover:border-foreground/30"
        >
          <div className="flex items-center gap-3">
            <span className="flex size-9 items-center justify-center rounded-lg border border-border">
              <FileText className="size-5" strokeWidth={1.5} />
            </span>
            <div>
              <p className="text-sm font-medium">Resume {r.id.slice(0, 8)}</p>
              <p className="text-xs text-muted-foreground">{new Date(r.createdAt).toLocaleDateString()}</p>
            </div>
          </div>
          <StatusPill status={r.status} />
        </motion.li>
      ))}
    </ul>
  );
}
