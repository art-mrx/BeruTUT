import { z } from 'zod'

export const checkoutSchema = z.object({
  shippingAddress: z.string().min(1, 'Введите адрес доставки').max(500, 'Слишком длинный адрес'),
})

export type CheckoutFormValues = z.infer<typeof checkoutSchema>
