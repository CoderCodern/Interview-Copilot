import type { Metadata } from "next";
import { Geist_Mono, Inter, Newsreader } from "next/font/google";
import { Providers } from "@/components/providers";
import "./globals.css";

// Folio type pairing (DESIGN.md §4): Newsreader (serif, editorial) for headings
// and display numbers; Inter for body and UI; Geist Mono for code/tokens.
const newsreader = Newsreader({
  variable: "--font-newsreader",
  subsets: ["latin"],
  style: ["normal", "italic"],
  weight: ["400", "500", "600"],
});
const inter = Inter({ variable: "--font-inter", subsets: ["latin"] });
const geistMono = Geist_Mono({ variable: "--font-geist-mono", subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Interview Copilot AI",
  description: "Walk in prepared. AI-powered, grounded interview preparation.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        {/* Prevent flash of light theme when user has dark mode saved (runs before paint). */}
        <script
          dangerouslySetInnerHTML={{
            __html: `try{const s=localStorage.getItem('ic-theme');if(s&&JSON.parse(s).state?.theme==='dark')document.documentElement.classList.add('dark')}catch(e){}`,
          }}
        />
      </head>
      <body className={`${newsreader.variable} ${inter.variable} ${geistMono.variable}`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
