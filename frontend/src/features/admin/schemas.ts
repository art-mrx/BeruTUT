import { z } from 'zod'

const slugPattern = /^[a-z0-9]+(-[a-z0-9]+)*$/

export const categorySchema = z.object({
  name: z.string().min(1, 'Введите название').max(200),
  slug: z.string().min(1, 'Введите slug').max(200).regex(slugPattern, 'Только строчные латинские буквы, цифры и дефисы'),
  parentCategoryId: z.string().optional(),
})

export type CategoryFormValues = z.infer<typeof categorySchema>

export const productSchema = z.object({
  name: z.string().min(1, 'Введите название').max(300),
  slug: z.string().min(1, 'Введите slug').max(300).regex(slugPattern, 'Только строчные латинские буквы, цифры и дефисы'),
  description: z.string().max(4000).optional(),
  price: z.number({ message: 'Введите цену' }).positive('Цена должна быть больше нуля'),
  stockQuantity: z.number({ message: 'Введите остаток' }).int().min(0, 'Остаток не может быть отрицательным'),
  categoryId: z.string().min(1, 'Выберите категорию'),
})

export type ProductFormValues = z.infer<typeof productSchema>
