export default function AuthCard({ children }: { children: React.ReactNode }) {
  return (
    <div className="auth-page">
      <div className="auth-card">{children}</div>
    </div>
  )
}
