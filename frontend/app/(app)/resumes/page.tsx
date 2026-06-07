import { ResumeList } from "@/components/resumes/resume-list";

export default function ResumesPage() {
  return (
    <section>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">Resumes</h1>
          <p className="mt-2 text-muted-foreground">Upload and manage the resumes behind your prep.</p>
        </div>
        <button className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-opacity hover:opacity-90">
          Upload resume
        </button>
      </div>
      <div className="mt-8">
        <ResumeList />
      </div>
    </section>
  );
}
