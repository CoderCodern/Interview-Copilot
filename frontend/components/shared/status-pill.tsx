import { cn } from "@/lib/utils";

type Status = "pending" | "processing" | "completed" | "failed";

const dot: Record<Status, string> = {
  pending: "bg-muted-foreground",
  processing: "bg-warning",
  completed: "bg-success",
  failed: "bg-danger",
};

/** Canonical async status indicator — warm bordered pill + status dot (DESIGN.md §7). */
export function StatusPill({ status }: { status: Status }) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-2 rounded-full border px-3 py-1 text-[10.5px] font-semibold uppercase tracking-[0.06em]",
        status === "completed"
          ? "border-accent-ring bg-accent-softer text-accent"
          : "border-border text-muted-foreground",
      )}
    >
      <span className={cn("size-1.5 rounded-full", dot[status])} />
      {status}
    </span>
  );
}
