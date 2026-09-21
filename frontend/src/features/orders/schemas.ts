import { z } from 'zod'

export const checkoutSchema = z.object({
  shippingAddress: z.string().min(1, 'Введите адрес доставки').max(500, 'Слишком длинный адрес'),
  contactPhone: z
    .string()
    .min(1, 'Введите телефон')
    .regex(/^\+?[0-9\s\-()]{6,32}$/, 'Введите корректный номер телефона'),
})

export type CheckoutFormValues = z.infer<typeof checkoutSchema>
