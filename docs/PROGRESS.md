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
6. **Каталог (frontend)** — `done`
   - [x] `HomePage` — сетка товаров, фильтры (поиск/категория/цена/сортировка) синхронизированы с URL query-параметрами через `useSearchParams` (шарибельные/букмаркабельные ссылки, работает back/forward браузера), пагинация
   - [x] `ProductDetailPage` — карточка товара по **slug** (не id — см. backend-фикс ниже), галерея изображений, наличие
   - [x] `features/catalog/{useProducts,useProduct,useCategories}.ts` — React Query хуки
   - [x] `features/catalog/ProductFilters.tsx` — debounce на поиске и вводе цены (300мс), чтобы не долбить API на каждое нажатие клавиши
   - [x] `components/{ProductCard,Pagination}.tsx` — переиспользуемые
   - [x] **Backend-фикс**: `GET /api/products/{id}` теперь принимает id ИЛИ slug (`GetProductByIdQuery.IdOrSlug`) — ТЗ п.7 явно требует маршрут `/products/:slug`, а backend на этапе 3 поддерживал только Guid-поиск. Один эндпоинт обслуживает оба случая (Admin CRUD продолжает ходить по Guid, публичная карточка — по slug)
   - [x] Ручная проверка в браузере: поиск с debounce, сортировка по цене, синхронизация URL, переход по карточке на `/products/gaming-laptop` — всё подтверждено на реальных данных (добавил 8 sample-товаров для проверки сетки/сортировки)
7. **Auth (frontend)** — `done`
   - [x] `LoginPage`/`RegisterPage` — React Hook Form + Zod (клиентская валидация: email-формат, длина пароля, совпадение пароля/подтверждения)
   - [x] `features/auth/{useLogin,useRegister}.ts` — React Query мутации, на успехе кладут результат в `authStore`
   - [x] Серверные ошибки: field-level (400 validation) мапятся на конкретные поля формы через `setError`, остальные (401 неверный пароль, 409 дубликат email) — общий баннер над кнопкой submit (`lib/apiError.ts`)
   - [x] Редирект после логина на страницу, с которой пришёл неавторизованный пользователь (`location.state.from`, выставляется в `ProtectedRoute` с этапа 5)
   - [x] Залогиненного пользователя со страниц `/login`/`/register` редиректит на главную
   - [x] `Layout` уже был подключен к `authStore`/`useLogout` с этапа 5 — теперь наконец есть чем его наполнить
   - [x] Ручной прогон в браузере на реальном backend: регистрация → авто-логин → persist после full reload → protected route без редиректа → logout → редирект на `/login` → неверный пароль (401, баннер) → верный пароль (редирект на исходную "from"-страницу) → повторная регистрация тем же email (409, баннер) — всё подтверждено
8. **Корзина и чекаут (frontend)** — `done`
   - [x] `CartPage` — строки с изменением количества (+/−, мгновенно через мутацию), удаление позиции, очистка корзины, итог
   - [x] `CheckoutPage` — форма адреса (RHF+Zod) + сводка заказа из корзины, после оформления редирект на `/orders/:id`
   - [x] `OrdersPage`/`OrderDetailPage` — список своих заказов с пагинацией и статусами (переведены на русский, `features/orders/statusLabels.ts`), детали заказа
   - [x] Кнопка «В корзину» + селектор количества на `ProductDetailPage` (сознательно не делали на этапе 6, см. DECISIONS.md) — для неавторизованных показывает "Войдите, чтобы добавить"
   - [x] Счётчик товаров в корзине в шапке (`Layout.tsx`)
   - [x] **Роутинг-фикс**: `/cart` перенесён под `ProtectedRoute` — на backend `CartController` требует `[Authorize]`, а маршрут в router.tsx с этапа 5 был публичным
   - [x] Ошибки нехватки остатка (409) отображаются прямо в строке корзины/на карточке товара, не ломая остальной UI
   - [x] Ручной e2e-прогон в браузере на реальном backend: логин → товар → +/− количество → «В корзину» → счётчик в шапке → корзина (изменение количества, пересчёт суммы) → чекаут (валидация пустого адреса, сводка) → оформление → редирект на детали заказа → список заказов → корзина пуста после заказа → превышение остатка (409) показывается в строке товара — всё подтверждено
9. **Админка (frontend)** — `done`
   - [x] `AdminProductsPage` — таблица (превью фото/цена/остаток/статус), форма создания/редактирования (RHF+Zod), загрузка изображения, скрыть/показать (деактивация/реактивация), пагинация
   - [x] `AdminCategoriesPage` — таблица с родителем, форма создания/редактирования, удаление с обработкой 409 (категория с товарами/подкатегориями)
   - [x] `AdminOrdersPage` — таблица всех заказов с данными покупателя, фильтр по статусу, смена статуса прямо в строке
   - [x] **Backend-добавление**: `GET /api/admin/products` (Admin-only, по аналогии с уже существующим `/api/admin/orders`) — публичный список всегда прячет `IsActive=false` товары, админке нужно видеть и их, чтобы иметь возможность вернуть в продажу. `GetProductsQuery.IncludeInactive` дефолтит в false, наружу через публичный `/api/products` не пробрасывается
   - [x] `ProductUpdatePayload.isActive` — сделан обязательным на фронте (не `isActive?`), т.к. `UpdateProductCommand.IsActive` на backend — обязательный bool, который тихо превратился бы в `false` при пропуске поля
   - [x] Ручной прогон в браузере под admin: увидел деактивированный ещё на этапе 3 "USB Cable" → вернул в продажу; создал/отредактировал товар с валидацией; создал вложенную категорию, попытка удалить родителя с товарами → 409 показан, удаление пустой категории → успех; сменил статус заказа, фильтр по статусу сработал; загрузка изображения (эмулирована через `DataTransfer`, реальный OS file-picker автоматизации недоступен) → миниатюра появилась в таблице
10. **Полировка** — `in-progress`
    - [x] CORS настроен через конфигурируемый `Cors:AllowedOrigins` (dev: `http://localhost:5173`; прод-домен фронтенда добавить при деплое) — раньше не требовался благодаря Vite dev-прокси, но для реального деплоя на разные домены обязателен (ТЗ п.6)
    - [x] Frontend `ErrorBoundary` — необработанная ошибка рендера больше не даёт белый экран, показывает сообщение с кнопкой перезагрузки
    - [x] Адаптивность проверена вручную на 375px (mobile preset) по всем основным страницам — найден и исправлен реальный баг: кнопка «Удалить» в строке корзины обрезалась за пределами экрана (`CartItemRow.tsx`, было `flex items-center`, стало `flex flex-wrap` с группировкой qty/сумма/удалить в отдельный `ml-auto` блок, переносящийся на новую строку). Admin-таблицы обёрнуты в `overflow-x-auto` с `min-w-[...]`, чтобы широкая таблица скроллилась горизонтально, а не ломала страницу
    - [x] Корневой [README.md](../README.md) — описание проекта, стек, структура, инструкция запуска (БД/backend/frontend), Docker/деплой статус. `frontend/README.md` — заменён с дефолтного Vite-шаблона на описание команд и структуры frontend
    - [ ] Деплой (backend — Railway/Render, frontend — Vercel, БД — Render/Neon, ТЗ п.2) — отложен по решению пользователя (2026-09-16)

## Доработка после MVP: подтверждение заказов админом (по запросу пользователя, 2026-09-21) — `done`
Сценарий: клиент оформляет заказ → админ звонит клиенту → подтверждает («Одобрен») или отклоняет («Отклонён»).
- [x] Статус `Approved` (=5, дописан в конец enum — значения хранятся числами). `Cancelled` в UI называется «Отклонён». `New` в UI = «Ожидает подтверждения»
- [x] Допустимые переходы статусов — `Store.Domain/Enums/OrderStatusRules` (New→Approved/Cancelled; Approved→Processing/Shipped/Cancelled; Processing→Shipped/Cancelled; Shipped→Delivered; Delivered и Cancelled — конечные). Недопустимый переход → 409
- [x] Остаток: подтверждение его не меняет (резервируется при оформлении), отклонение возвращает товары в наличие — в `UpdateOrderStatusCommandHandler`, один раз (Cancelled конечный → повторно не вернуть)
- [x] `Order.ContactPhone` (обязателен при оформлении, валидация формата) — миграция `AddOrderContactPhone`, у старых заказов пустая строка → в админке «Телефон не указан»
- [x] `AdminOrderDto.AllowedNextStatuses` (вычисляемое, из тех же правил) — UI рисует только допустимые кнопки, правила не дублируются на фронте
- [x] Frontend: чекаут с телефоном; у клиента цветной бейдж статуса + пояснения; админка — `AdminLayout` с вкладками «Ожидают подтверждения (N)» / «Все заказы» / «Товары» / «Категории» (раньше на категории и заказы можно было попасть только по прямой ссылке), карточки заказов с клиентом/телефоном/адресом/составом и кнопками действий, счётчик ожидающих обновляется раз в 30 сек
- [x] Тесты: 25/25 (19 новых: правила переходов, подтверждение не трогает остаток, отклонение возвращает, повторное отклонение не возвращает дважды, возврат для скрытого товара, недопустимые переходы)
- [x] Проверено: curl (13→11→11 при подтверждении→9→11 при отклонении, повторное отклонение 409, 403 для не-админа, 400 без телефона) и в браузере (чекаут с валидацией → очередь у админа → подтвердить/отклонить → «Все заказы» → список клиента → мобильная ширина)
- [x] **Фильтры «Все заказы»** (2026-09-21): поиск по номеру заказа, фильтр по дате «с»/«по», сортировка «сначала новые/старые» (сортировка есть и во вкладке «Ожидают подтверждения»), статус, кнопка «Сбросить». Backend: `GET /api/admin/orders?search=&createdFrom=&createdTo=&sortBy=newest|oldest`. Тесты 43/43; на настоящем PostgreSQL проверены обе сортировки, поиск (регистр, «№»), диапазоны дат, комбинации, 400 на неверные параметры; UI проверен в браузере
- Как ставятся статусы (справка): в «Все заказы» у карточки показаны кнопки допустимых шагов — Одобрен → «В обработку»/«Отправлен»/«Отклонить»; В обработке → «Отправлен»/«Отклонить»; Отправлен → «Доставлен»

## Следующий шаг
Backend + Frontend MVP по разделу 8 ТЗ полностью готовы (этапы 1-9), этап 10 почти завершён — остался только сам деплой, требующий решений и аккаунтов пользователя на конкретных хостинг-платформах.

Запуск для разработки: backend — `cd backend/src/Store.Api && dotnet run` (порт 5080), frontend — `cd frontend && npm run dev` (порт 5173, прокси на 5080 уже настроен).

В локальной БД есть sample-данные для разработки фронтенда: admin (`admin@store.local`), покупатель `buyer1@example.com`/`Password123`, категория Electronics, 9 товаров (Gaming Laptop + 8 sample-товаров с ценами $29.99-$349), один оформленный заказ.
