export default function FieldError({ id, message }: Readonly<{ id: string; message?: string }>) {
  if (!message) return null
  return (
    <p id={id} className="field-error" role="alert">
      {message}
    </p>
  )
}
