interface SubmitButtonProps {
  readonly isSubmitting: boolean
  readonly label: string
  readonly loadingLabel?: string
  readonly disabled?: boolean
}

export default function SubmitButton({ isSubmitting, label, loadingLabel = 'Loading…', disabled }: SubmitButtonProps) {
  return (
    <button type="submit" disabled={disabled ?? isSubmitting} className="btn-primary mt-1">
      {isSubmitting ? (
        <span className="flex items-center justify-center gap-2">
          <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24" aria-hidden="true">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          {loadingLabel}
        </span>
      ) : (
        label
      )}
    </button>
  )
}
