import { Upload } from "lucide-react";
import { ResumeList } from "@/components/resumes/resume-list";

export default function ResumesPage() {
  return (
    <section>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-[clamp(30px,5vw,44px)] leading-[1.05] tracking-tight">Resumes</h1>
          <p className="mt-3 text-[17px] text-muted-foreground">Upload and manage the resumes behind your prep.</p>
        </div>
        <button
          type="button"
          className="inline-flex items-center gap-2 whitespace-nowrap rounded-full border border-foreground bg-foreground px-5 py-[0.45em] text-[15px] text-background transition-colors duration-200 hover:bg-background hover:text-foreground"
        >
          <Upload className="size-4" strokeWidth={1.5} />
          Upload resume
        </button>
      </div>
      <div className="mt-10">
        <ResumeList />
      </div>
    </section>
  );
}
