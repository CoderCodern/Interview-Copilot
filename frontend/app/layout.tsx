import type { Metadata } from "next";
import { Geist_Mono } from "next/font/google";
import { Providers } from "@/components/providers";
import "./globals.css";

// Mono retained for code/tokens; body + headings use Helvetica Now (loaded via <link> below).
const geistMono = Geist_Mono({ variable: "--font-geist-mono", subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Interview Copilot AI",
  description: "Walk in prepared. AI-powered, grounded interview preparation.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        {/* Helvetica Now Display — heading (Medium) + body (Regular). DESIGN.md §4. */}
        <link
          rel="stylesheet"
          href="https://db.onlinewebfonts.com/c/5ac3fe7c6abd2f62067f266d89671492?family=HelveticaNowDisplay-Medium"
        />
        <link
          rel="stylesheet"
          href="https://db.onlinewebfonts.com/c/1aa3377e489837a26d019bba501e779d?family=HelveticaNowDisplayW01-Rg"
        />
      </head>
      <body className={geistMono.variable}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
