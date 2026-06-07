"use client";

import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

/**
 * Subscribes to the API's Server-Sent Events stream and invalidates the matching query
 * when an async artifact completes — no polling loops (Doc 05 §1, Doc 06 §3).
 */
export function useServerEvents(token?: string) {
  const qc = useQueryClient();

  useEffect(() => {
    const base = process.env.NEXT_PUBLIC_API_BASE_URL ?? "";
    const source = new EventSource(`${base}/events${token ? `?access_token=${token}` : ""}`);

    source.addEventListener("analysis.completed", (e) => {
      const { resource, id } = JSON.parse((e as MessageEvent).data);
      qc.invalidateQueries({ queryKey: [resource] });
      void id;
    });

    source.onerror = () => source.close();
    return () => source.close();
  }, [qc, token]);
}
