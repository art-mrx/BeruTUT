import { useParams } from 'react-router-dom'

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900">Заказ {id}</h1>
      <p className="mt-2 text-gray-500">Детали заказа появятся на этапе 8.</p>
    </div>
  )
}
