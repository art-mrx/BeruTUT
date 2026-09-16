# Прогресс разработки

Статусы: `done`, `in-progress`, `blocked`, `todo`. Обновлять после каждого этапа.
Порядок этапов — раздел 8 [TECH_SPEC.md](../TECH_SPEC.md).

## Backend

1. **Backend skeleton** — `done`
   - [x] Solution + 4 проекта (Store.Domain/Application/Infrastructure/Api) + Store.Application.Tests
   - [x] Доменные сущности (User, Category, Product, ProductImage, Cart, CartItem, Order, OrderItem)
   - [x] StoreDbContext + Fluent-конфигурации в Store.Infrastructure/Persistence
   - [x] Serilog, Swagger, DI (AddApplication/AddInfrastructure) в Program.cs
   - [x] Первая миграция `InitialCreate` сгенерирована и применена к локальной БД `store` (PostgreSQL 18, локальная служба)
   - [x] Dockerfile + docker-compose.yml написаны, не запускались (Docker пока не работает на устройстве пользователя)
   - [x] Проверено: `dotnet run` в Store.Api поднимается, Swagger UI открывается на `/swagger` (без операций — контроллеров ещё нет, это ожидаемо)
2. **Auth** — `done` (кастомный JWT, не Identity)
   - [x] `POST /api/auth/register`, `/login`, `/refresh`, `GET /api/auth/me`
   - [x] `POST /api/auth/logout` — сверх ТЗ, нужен раз завели БД-backed refresh-токены (см. DECISIONS.md)
   - [x] Refresh-токены хранятся в БД захешированными (SHA-256), с ротацией и reuse-detection (при повторном использовании уже отозванного токена отзываются все активные сессии пользователя)
   - [x] Пароли — `Microsoft.AspNetCore.Identity.PasswordHasher<User>` (PBKDF2)
   - [x] JWT-секреты — dotnet user-secrets (не в репозитории)
   - [x] Единый формат ошибок через `ExceptionHandlingMiddleware` (ProblemDetails: 400/401/404/409/500)
   - [x] MediatR ValidationBehavior — FluentValidation гоняется в пайплайне автоматически
   - [x] Swagger настроен на Bearer-авторизацию (кнопка Authorize)
   - [x] Ручной smoke-тест всех сценариев через curl: register/login/me/refresh-rotation/reuse-detection/logout — все статусы (200/204/400/401/409) подтверждены
3. **Categories + Products (backend)** — `done`
   - [x] Categories CRUD: `GET /api/categories`, `GET /{id}`, `POST/PUT/DELETE` (Admin)
   - [x] Products CRUD + публичный список: `GET /api/products` (page/pageSize/categoryId/search/sortBy/minPrice/maxPrice), `GET /{id}`, `POST/PUT/DELETE` (Admin)
   - [x] `DELETE /api/products/{id}` — деактивация (`IsActive=false`), не жёсткое удаление (см. ТЗ "удалить/деактивировать")
   - [x] `POST /api/products/{id}/images` — загрузка файла (Admin), локальное хранилище через `IFileStorageService` (см. DECISIONS.md)
   - [x] Пагинация — общий `PaginatedList<T>` (Items/Page/PageSize/TotalCount/TotalPages)
   - [x] `IApplicationDbContext` расширен на Categories/Products без изменений схемы (таблицы уже были с этапа 1)
   - [x] Сидер admin-пользователя при старте в Development (см. DECISIONS.md — учётные данные не в репозитории)
   - [x] Ручной smoke-тест через curl: CRUD категорий/товаров, фильтры/сортировка/пагинация, загрузка и раздача изображения, деактивация, конфликт удаления категории с товарами, 401/403/404/409/400 — всё подтверждено
4. **Cart + Orders (backend)** — `done`
   - [x] Cart: `GET /api/cart`, `POST /items`, `PUT /items/{id}`, `DELETE /items/{id}`, `DELETE /api/cart` — корзина создаётся автоматически при регистрации, доступна только владельцу
   - [x] Orders: `POST /api/orders` (оформление из корзины), `GET /api/orders`, `GET /api/orders/{id}` — только свои заказы
   - [x] Admin: `GET /api/admin/orders` (пагинация + фильтр по статусу), `PUT /api/admin/orders/{id}/status`
   - [x] Расчёт суммы заказа и проверка остатков — при добавлении в корзину (мягкая проверка) и повторно при оформлении заказа (авторитетная проверка, cart-to-order не доверяет предыдущим проверкам)
   - [x] `OrderItem` — снапшот цены/названия на момент заказа (Cart живёт по текущим ценам, Order — замороженная копия)
   - [x] Юнит-тесты (ТЗ п.9): расчёт суммы заказа, списание остатков, отказ при нехватке остатков, пустая корзина, накопление количества в корзине — на реальном `StoreDbContext` через EF Core InMemory provider (не моки LINQ), + тест валидности всех AutoMapper-профилей. 6/6 зелёных
   - [x] Исправлен баг: JSON-тело не принимало enum по строковому имени (`"status":"Processing"`) — добавлен `JsonStringEnumConverter` глобально
   - [x] Ручной smoke-тест через curl: полный цикл корзина→заказ→смена статуса, права доступа (403 не-админу), фильтр по статусу, конфликт при пустой корзине — всё подтверждено
5. **Frontend skeleton** — `done`
   - [x] Vite + React 18 + TypeScript (см. DECISIONS.md — почему React 18, а не React Router 6, версии зафиксированы/отклонены осознанно)
   - [x] Структура папок по ТЗ п.7: `api/`, `components/`, `features/{auth,catalog,cart,orders,admin}`, `pages/`, `routes/`, `hooks/`, `types/`
   - [x] Tailwind CSS v4 подключен через `@tailwindcss/vite`, alias `@/*` → `src/*` (vite.config.ts + tsconfig)
   - [x] `api/client.ts` — axios-инстанс: подстановка access token, single-flight refresh при 401 (учитывает ротацию refresh-токенов на бэкенде — см. DECISIONS.md)
   - [x] `api/{auth,categories,products,cart,orders}.ts` — типизированные функции для всех эндпоинтов backend
   - [x] `types/` — TS-типы, зеркалящие backend DTO (camelCase, как отдаёт System.Text.Json)
   - [x] `features/auth/authStore.ts` — Zustand-стор с персистентностью в localStorage
   - [x] React Router v6→v7 (см. DECISIONS.md, security-апгрейд) + `ProtectedRoute`/`AdminRoute` + роуты под все страницы MVP (наполнение — этапы 6-9, сейчас заглушки)
   - [x] React Query (`QueryClientProvider`) подключен в `main.tsx`
   - [x] Dev-прокси Vite `/api` и `/uploads` → `http://localhost:5080` (не нужен CORS в деве)
   - [x] Проверено: `npm run build`, `tsc -b`, `npm run lint` — чисто; dev-сервер отдаёт реальные данные с backend через прокси; `ProtectedRoute`/`AdminRoute` корректно редиректят неавторизованных на `/login`
6. **Каталог (frontend)** — `todo`
7. **Auth (frontend)** — `todo`
8. **Корзина и чекаут (frontend)** — `todo`
9. **Админка (frontend)** — `todo`
10. **Полировка** — `todo`

## Следующий шаг
Этап 6: **Каталог (frontend)** — список товаров, фильтры, карточка товара. Наполнить `HomePage`/`ProductDetailPage` реальным содержимым через `api/products.ts` + React Query.

Запуск для разработки: backend — `cd backend/src/Store.Api && dotnet run` (порт 5080), frontend — `cd frontend && npm run dev` (порт 5173, прокси на 5080 уже настроен).

В локальной БД есть sample-данные для разработки фронтенда: admin (`admin@store.local`), покупатель `buyer1@example.com`/`Password123`, категория Electronics, товар Gaming Laptop, один оформленный заказ.
