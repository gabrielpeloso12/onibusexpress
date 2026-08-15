import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="container">
      <h1>Página não encontrada</h1>
      <p>O endereço acessado não existe.</p>
      <Link to="/" className="btn btn-primary">
        Voltar para a busca de passagens
      </Link>
    </div>
  )
}
