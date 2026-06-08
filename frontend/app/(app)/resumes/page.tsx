import { Upload } from "lucide-react";
import { ResumeList } from "@/components/resumes/resume-list";

export default function ResumesPage() {
  return (
    <section>
      <div className="flex items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl tracking-tight">Resumes</h1>
          <p className="mt-2 text-muted-foreground">Upload and manage the resumes behind your prep.</p>
        </div>
        <button className="inline-flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary-hover">
          <Upload className="size-4" />
          Upload resume
        </button>
      </div>
      <div className="mt-8">
        <ResumeList />
      </div>
    </section>
  );
}
