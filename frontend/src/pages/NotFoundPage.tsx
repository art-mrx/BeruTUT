import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900">404 — страница не найдена</h1>
      <Link to="/" className="mt-2 inline-block text-blue-600 hover:underline">
        На главную
      </Link>
    </div>
  )
}
