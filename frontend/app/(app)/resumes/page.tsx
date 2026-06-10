import { Upload } from "lucide-react";
import { ResumeList } from "@/components/resumes/resume-list";

export default function ResumesPage() {
  return (
    <section>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="eyebrow">Library</p>
          <h1 className="mt-2 text-[clamp(28px,4.5vw,38px)]">Resumes</h1>
          <p className="mt-2 text-[15px] text-muted-foreground">Upload and manage the resumes behind your prep.</p>
        </div>
        <button type="button" className="btn btn-primary">
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
