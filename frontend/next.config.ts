import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Containerized standalone output for Docker/ECS (Doc 08 §1).
  output: "standalone",
  reactStrictMode: true,
  // React Compiler moved to top-level in Next.js 16 (Doc 06 §6).
  reactCompiler: true,
  async headers() {
    // Baseline security headers (Doc 10 §6); CSP tightened per environment.
    return [
      {
        source: "/(.*)",
        headers: [
          { key: "X-Content-Type-Options", value: "nosniff" },
          { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
          { key: "X-Frame-Options", value: "DENY" },
        ],
      },
    ];
  },
};

export default nextConfig;
