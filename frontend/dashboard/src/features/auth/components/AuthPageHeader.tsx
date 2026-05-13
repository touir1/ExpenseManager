export default function AuthPageHeader({ title, subtitle }: Readonly<{ title: string; subtitle: string }>) {
  return (
    <div className="mb-7">
      <h1 className="font-serif text-[38px] leading-[1.05] tracking-[-0.01em] text-ink mb-2">{title}</h1>
      <p className="text-[15px] text-ink-body leading-relaxed">{subtitle}</p>
    </div>
  )
}

