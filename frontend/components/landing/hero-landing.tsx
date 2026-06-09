"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { SiteNav } from "@/components/layout/site-nav";

const VIDEO_URL = "/videos/hero.mp4";
const EMAIL = "hello@interviewcopilot.ai";
const SCRUB_SENSITIVITY = 0.8;

const whitePills = [
  { label: "Analyze a company", href: "/dashboard" },
  { label: "Decode a job description", href: "/dashboard" },
  { label: "Start a mock interview", href: "/dashboard" },
  { label: "See how it works", href: "/dashboard" },
];

const TYPED_TEXT =
  "Glad you stopped by. Point me at a company, a job description, and your resume — and we'll get you ready to walk in prepared.";

/** Reveals `text` one character at a time after `startDelay`, then sets `done`. */
function useTypewriter(text: string, speed = 38, startDelay = 600) {
  const [displayed, setDisplayed] = useState("");
  const [done, setDone] = useState(false);

  useEffect(() => {
    setDisplayed("");
    setDone(false);
    let i = 0;
    let interval: ReturnType<typeof setInterval> | undefined;
    const timeout = setTimeout(() => {
      interval = setInterval(() => {
        i += 1;
        setDisplayed(text.slice(0, i));
        if (i >= text.length) {
          if (interval) clearInterval(interval);
          setDone(true);
        }
      }, speed);
    }, startDelay);

    return () => {
      clearTimeout(timeout);
      if (interval) clearInterval(interval);
    };
  }, [text, speed, startDelay]);

  return { displayed, done };
}

export function HeroLanding() {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [pillsVisible, setPillsVisible] = useState(false);
  const [copied, setCopied] = useState(false);
  const { displayed, done } = useTypewriter(TYPED_TEXT);

  // Pills fade/slide in 400ms after load, independent of the typewriter.
  useEffect(() => {
    const t = setTimeout(() => setPillsVisible(true), 400);
    return () => clearTimeout(t);
  }, []);

  // Mouse-scrub the background video; an onSeeked queue prevents seek-flooding.
  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    let prevX: number | null = null;
    let targetTime = 0;
    let seeking = false;

    const seek = () => {
      if (!video.duration || Number.isNaN(video.duration)) return;
      if (Math.abs(video.currentTime - targetTime) < 0.001) {
        seeking = false;
        return;
      }
      seeking = true;
      video.currentTime = targetTime;
    };

    const onMouseMove = (e: MouseEvent) => {
      if (prevX === null) {
        prevX = e.clientX;
        return;
      }
      const delta = e.clientX - prevX;
      prevX = e.clientX;
      if (!video.duration || Number.isNaN(video.duration)) return;

      const next = targetTime + (delta / window.innerWidth) * SCRUB_SENSITIVITY * video.duration;
      targetTime = Math.min(Math.max(next, 0), video.duration);
      if (!seeking) seek();
    };

    const onSeeked = () => {
      if (Math.abs(video.currentTime - targetTime) > 0.001) {
        seek();
      } else {
        seeking = false;
      }
    };

    window.addEventListener("mousemove", onMouseMove);
    video.addEventListener("seeked", onSeeked);
    return () => {
      window.removeEventListener("mousemove", onMouseMove);
      video.removeEventListener("seeked", onSeeked);
    };
  }, []);

  const copyEmail = async () => {
    try {
      await navigator.clipboard.writeText(EMAIL);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // Clipboard may be unavailable (insecure context); silently no-op.
    }
  };

  return (
    <div className="text-black" style={{ fontFamily: "var(--font-body)" }}>
      {/* Background video — mouse-scrub controlled, does not autoplay. */}
      <video
        ref={videoRef}
        className="fixed inset-0 z-0 h-full w-full object-cover"
        style={{ objectPosition: "70% center" }}
        src={VIDEO_URL}
        muted
        playsInline
        preload="auto"
      />

      <SiteNav variant="overlay" cta={{ label: "Get started", href: "/dashboard" }} />

      {/* Hero */}
      <section className="relative z-[1] flex h-screen flex-col justify-end overflow-hidden px-5 pb-12 sm:px-8 md:justify-center md:px-10 md:pb-0">
        <div className="relative z-10 max-w-xl">
          <div
            className="pointer-events-none mb-5 select-none sm:mb-6"
            style={{
              fontSize: "clamp(18px, 4vw, 26px)",
              lineHeight: 1.3,
              fontWeight: 400,
              color: "#000",
              filter: "blur(4px)",
            }}
          >
            Hey there, I&apos;m your Interview Copilot,
            <br />
            grounded prep for the company, the role, and you.
          </div>

          <p
            className="mb-5 text-black sm:mb-6"
            style={{ fontSize: "clamp(18px, 4vw, 26px)", lineHeight: 1.35, fontWeight: 400, minHeight: "54px" }}
          >
            {displayed}
            {!done && (
              <span
                className="ml-[2px] inline-block h-[1.1em] w-[2px] bg-black align-middle"
                style={{ animation: "blink 1s step-end infinite" }}
              />
            )}
          </p>

          <div
            className="flex flex-wrap gap-y-1"
            style={{
              opacity: pillsVisible ? 1 : 0,
              transform: pillsVisible ? "translateY(0)" : "translateY(8px)",
              transition: "opacity 0.4s ease, transform 0.4s ease",
            }}
          >
            {whitePills.map((pill) => (
              <Link
                key={pill.label}
                href={pill.href}
                className="mx-[0.2em] mb-[0.4em] inline-flex items-center justify-center whitespace-nowrap rounded-full border border-black/10 bg-white px-4 py-[0.3em] text-[13px] text-black transition-colors duration-200 hover:bg-black hover:text-white sm:px-5 sm:text-[15px]"
              >
                {pill.label}
              </Link>
            ))}

            <button
              type="button"
              onClick={copyEmail}
              aria-label={`Copy email ${EMAIL}`}
              className="mx-[0.2em] mb-[0.4em] inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-full border border-white bg-transparent px-4 py-[0.3em] text-[13px] text-white transition-colors duration-200 hover:bg-white hover:text-black sm:gap-3 sm:px-5 sm:text-[15px]"
            >
              <span>
                Reach us: <span className="underline underline-offset-1">{EMAIL}</span>
              </span>
              {copied ? (
                <svg width="12" height="12" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.4" aria-hidden="true">
                  <path d="M2.5 6.5 5 9l4.5-5" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              ) : (
                <svg width="12" height="12" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1" aria-hidden="true">
                  <rect x="3.5" y="3.5" width="6.5" height="6.5" rx="1" />
                  <rect x="1.5" y="1.5" width="6.5" height="6.5" rx="1" />
                </svg>
              )}
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
