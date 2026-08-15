import styles from './LoadingSpinner.module.css'

interface LoadingSpinnerProps {
  label?: string
}

export function LoadingSpinner({ label = 'Carregando...' }: LoadingSpinnerProps) {
  return (
    <div className={styles.wrapper} role="status" aria-live="polite">
      <span className={styles.spinner} aria-hidden="true" />
      <span>{label}</span>
    </div>
  )
}
