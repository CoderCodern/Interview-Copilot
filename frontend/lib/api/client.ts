/**
 * Single typed fetch wrapper for the API (Doc 06 §4). It is the only place that:
 *  - knows the API base URL,
 *  - attaches the Bearer token and a correlation id,
 *  - normalizes RFC 9457 problem+json into a typed ApiError.
 */
const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080/api/v1";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: string,
    message: string,
    public readonly correlationId?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export interface ApiOptions extends RequestInit {
  token?: string;
}

export async function apiFetch<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const { token, headers, ...rest } = options;
  const correlationId = crypto.randomUUID();

  const res = await fetch(`${BASE_URL}${path}`, {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      "X-Correlation-Id": correlationId,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
  });

  if (res.status === 204) return undefined as T;

  const body = await res.json().catch(() => null);

  if (!res.ok) {
    const code = body?.code ?? "internal.error";
    const message = body?.detail ?? body?.title ?? res.statusText;
    throw new ApiError(res.status, code, message, body?.correlationId ?? correlationId);
  }

  return body as T;
}
