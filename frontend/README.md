# BeruTUT — frontend

React 18 + TypeScript + Vite. См. [корневой README](../README.md) для запуска всего проекта и общего описания.

## Команды

```bash
npm run dev      # dev-сервер на :5173, проксирует /api и /uploads на backend (:5080)
npm run build    # tsc -b && vite build
npm run lint      # oxlint
npm run preview   # предпросмотр production-сборки
```

## Структура

```
src/
  api/            — axios-клиент, типизированные функции запросов к backend
  components/     — переиспользуемые UI-компоненты
  features/       — логика по доменам (auth, catalog, cart, orders, admin):
                    React Query хуки, мутации, Zod-схемы форм
  pages/          — страницы, собранные из features
  routes/         — конфигурация роутинга, ProtectedRoute/AdminRoute
  hooks/          — общие кастомные хуки
  lib/            — мелкие утилиты (форматирование, разбор ошибок API)
  types/          — TypeScript-типы, зеркалящие backend DTO
```
