import { NavLink } from 'react-router-dom'
import styles from './Header.module.css'

export function Header() {
  return (
    <header className={styles.header}>
      <div className={`container ${styles.inner}`}>
        <NavLink to="/" className={styles.brand}>
          🚌 OniBus Express
        </NavLink>
        <nav className={styles.nav}>
          <NavLink to="/" end className={({ isActive }) => (isActive ? styles.activeLink : styles.link)}>
            Buscar passagens
          </NavLink>
          <NavLink to="/consulta" className={({ isActive }) => (isActive ? styles.activeLink : styles.link)}>
            Consultar reserva
          </NavLink>
        </nav>
      </div>
    </header>
  )
}
