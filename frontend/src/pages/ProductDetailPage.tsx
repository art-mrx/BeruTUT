import { useParams } from 'react-router-dom'

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>()

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900">Товар: {slug}</h1>
      <p className="mt-2 text-gray-500">Карточка товара появится на этапе 6.</p>
    </div>
  )
}
