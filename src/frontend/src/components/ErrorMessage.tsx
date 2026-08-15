interface ErrorMessageProps {
  message: string
}

export function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div className="alert alert-error" role="alert">
      {message}
    </div>
  )
}
