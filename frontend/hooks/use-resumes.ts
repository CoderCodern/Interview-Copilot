"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api/client";
import { queryKeys } from "@/lib/query/keys";

export interface ResumeSummary {
  id: string;
  status: "pending" | "processing" | "completed" | "failed";
  isCurrent: boolean;
  createdAt: string;
}

/** List the current candidate's resumes. */
export function useResumes() {
  return useQuery({
    queryKey: queryKeys.resumes.list(),
    queryFn: () => apiFetch<ResumeSummary[]>("/resumes"),
  });
}

/** Upload a resume source; on success the resume list is invalidated and refetched. */
export function useUploadResume() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: { sourceType: string; text?: string; blobKey?: string; contentType?: string }) =>
      apiFetch<ResumeSummary>("/resumes", { method: "POST", body: JSON.stringify(body) }),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.resumes.list() }),
  });
}
