import { cn } from "@/lib/utils";

type Status = "pending" | "processing" | "completed" | "failed";

const styles: Record<Status, string> = {
  pending: "bg-surface-muted text-muted-foreground",
  processing: "bg-warning/15 text-warning",
  completed: "bg-success/15 text-success",
  failed: "bg-danger/15 text-danger",
};

/** Canonical async status indicator (DESIGN.md §7). */
export function StatusPill({ status }: { status: Status }) {
  return (
    <span className={cn("inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium capitalize", styles[status])}>
      {status}
    </span>
  );
}
