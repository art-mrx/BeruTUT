# 02. Архитектура

## Backend (`backend/src`)
Слои и зависимости (стрелка = «ссылается на»): `Store.Api → Application, Infrastructure` · `Infrastructure → Application, Domain` · `Application → Domain` · `Domain → ничего`.
- **Store.Domain** — сущности, enum'ы, `OrderStatusRules` (таблица допустимых переходов статуса заказа). Без зависимостей.
- **Store.Application** — вся бизнес-логика: CQRS на MediatR. Папка `Features/<Фича>/{Commands,Queries,Dtos}` + `<Фича>MappingProfile`. Фичи: `Auth, Carts, Categories, Orders, Products, Users`. Общее — в `Common/` (интерфейсы, исключения, `ValidationBehavior`, `PaginatedList<T>`, `JwtSettings`).
- **Store.Infrastructure** — EF Core (`StoreDbContext`, Fluent-конфигурации, миграции), `PasswordHasher` (Identity PBKDF2), `JwtTokenService`, `LocalFileStorageService`, `DbSeeder`.
- **Store.Api** — тонкие контроллеры (только `IMediator.Send`), middleware, `Program.cs`, `Security/UserStatusTokenValidator`, `Services/CurrentUserService`.

### Сквозные паттерны (не переизобретать)
- **Доступ к данным:** хендлеры зависят от `IApplicationDbContext` (интерфейс с `DbSet<T>`), а не от generic-репозитория; реализует `StoreDbContext`. Application ссылается на пакет `Microsoft.EntityFrameworkCore` только ради типа `DbSet`.
- **Валидация:** FluentValidation-валидаторы лежат рядом с командой/запросом, `ValidationBehavior` (MediatR pipeline) гоняет их автоматически → `ValidationException` → 400. Бизнес-конфликты (дубликат email/slug, нехватка остатка, недопустимый переход статуса) — `ConflictException` из хендлера, не из валидатора.
- **Ошибки:** `ExceptionHandlingMiddleware` мапит исключения в ProblemDetails: `ValidationException`→400 (`errors` по полям, ключи в PascalCase), `AuthenticationException`→401, `ForbiddenException`→403, `NotFoundException`→404, `ConflictException`→409, остальное→500 (полный стек только в лог, наружу «An unexpected error occurred» — **если видишь именно это, причина в логе backend**).
- **Порядок в `Program.cs`:** ExceptionHandling → Serilog request logging → HTTPS redirect → StaticFiles → CORS → Authentication → Authorization → контроллеры. Сидер админа — только в Development.
- **Списки:** `AutoMapper.ProjectTo<Dto>()` + `PaginatedList<T>.CreateAsync` → `{items,page,pageSize,totalCount,totalPages}`. Пагинация везде, `pageSize` ≤ 100.
- **JSON:** camelCase; enum в теле запроса принимается строкой (`JsonStringEnumConverter` глобально); в DTO статусы отдаются строками.
- **Поиск без привязки к провайдеру:** `ToLower().Contains(...)`, а не `EF.Functions.ILike` (Application не знает про Npgsql). Поиск по номеру — `Id.ToString().Contains(...)` (Npgsql транслирует в `::text`; InMemory-тесты этого не доказывают — проверять на реальной БД).
- **Время:** всё в UTC; `DateTime` из query-string приводится к `Kind=Utc` (Npgsql иначе бросает исключение). Диапазон дат — `[from, to)`.
- **Ловушка C# (сработала дважды):** имя папки/namespace фичи, совпадающее с именем сущности (`RefreshToken`, `Cart`), ломает компиляцию (namespace ищется раньше using-алиасов). Решение — **множественное число**: `Commands/RefreshTokens`, `Features/Carts`, `Features/Users`. Для новых фич — сразу множественное.
- **AutoMapper:** get-only вычисляемые коллекции в DTO (`AdminOrderDto.AllowedNextStatuses`) надо `Ignore()` в профиле, иначе `AssertConfigurationIsValid` (его вызывает `MapperProfilesTests`) падает.
- **Тесты** (`backend/tests/Store.Application.Tests`, 66 шт.): хендлеры тестируются на **реальном `StoreDbContext` с EF InMemory** (`TestDbContextFactory`) и **настоящем `IMapper`** (`MapperFactory`, заодно валидирует все профили); внешние сервисы (`ICurrentUserService`, `IPasswordHasher`, `IJwtTokenService`) — Moq. Запуск не требует остановки backend.

### Аутентификация и безопасность
- Кастомный JWT (не Identity). Access — 20 мин (`Jwt:AccessTokenMinutes`), refresh — 14 дней. Refresh хранится **хешем SHA-256** в таблице `RefreshTokens`, **ротируется** при каждом `/auth/refresh`; повторное предъявление уже использованного токена = подозрение на кражу → отзываются **все** сессии пользователя.
- Роли `Customer`/`Admin` в claim `ClaimTypes.Role`; админские контроллеры — `[Authorize(Roles="Admin")]`.
- **Блокировка пользователя действует мгновенно:** `UserStatusTokenValidator` (`JwtBearerEvents.OnTokenValidated`) на каждом запросе смотрит пользователя в БД и отклоняет токен заблокированного/удалённого; логин и refresh заблокированного → 403; при блокировке refresh-токены отзываются. Логин проверяет пароль **до** статуса (не раскрываем заблокированные аккаунты).
- Секреты: `Jwt:SigningKey`, `AdminSeed:Password` — только `dotnet user-secrets`/env-переменные. Строка подключения в `appsettings.json` — несекретная локальная (`store/store`). CORS — `Cors:AllowedOrigins` (dev: `http://localhost:5173` в `appsettings.Development.json`; прод-домен добавить при деплое).
- Загрузка изображений: `IFileStorageService` → диск `Store.Api/wwwroot/uploads/products/` (в `.gitignore`), раздаётся `UseStaticFiles`. Для прода — заменить реализацию (S3/Cloudinary), интерфейс не меняется.

## Frontend (`frontend/src`)
```
api/         axios-клиент (client.ts) + типизированные функции по ресурсам (auth, products, categories, cart, orders, adminUsers)
components/  Layout, AdminLayout (вкладки админки + счётчик ожидающих), ProductCard, Pagination, OrderStatusBadge, ErrorBoundary
features/    auth (authStore, schemas, useLogin/useRegister) · catalog · cart · orders (statusLabels, schemas) · admin (хуки и Zod-схемы админки)
pages/       страницы; pages/admin/* — админские
routes/      router.tsx, ProtectedRoute, AdminRoute
hooks/       useLogout, useDebouncedValue
lib/         format (цена, USD), apiError (разбор ProblemDetails), dates (границы локальных суток), accountBlocked
types/       TS-типы, зеркалящие DTO backend (camelCase)
```
- **Алиас `@/*` → `src/*`** (vite.config.ts и tsconfig.app.json; без `baseUrl` — в TS 6 он deprecated).
- **API-клиент (`api/client.ts`):** подставляет access-токен; при 401 делает **один** общий `/auth/refresh` (single-flight, `refreshPromise`) — критично, потому что backend ротирует refresh-токен и параллельный второй refresh был бы принят за кражу и разлогинил бы пользователя. Если refresh ответил 403 → ставится флаг `berutut-account-blocked` (sessionStorage) для пояснения на странице входа.
- **Состояние:** серверное — TanStack Query; клиентское — только `authStore` (Zustand, persist в localStorage под ключом **`berutut-auth`**, формат `{state:{user,accessToken,refreshToken},version:0}`). Отдельного стора корзины нет намеренно (корзина только у авторизованного, источник истины — React Query `['cart']`).
- **Ключи кэша:** `['products', params|idOrSlug]`, `['categories']`, `['cart']`, `['orders', params|id]`, `['admin','orders'|'products'|'users', params]`. Мутации инвалидируют нужные префиксы (отклонение заказа инвалидирует ещё и товары — остаток изменился).
- **Роутинг:** `/` каталог, `/products/:slug`, `/login`, `/register`; под `ProtectedRoute`: `/cart`, `/checkout`, `/orders`, `/orders/:id`; под `AdminRoute` → `AdminLayout`: `/admin` (редирект на очередь), `/admin/orders/pending`, `/admin/orders`, `/admin/products`, `/admin/categories`, `/admin/users`. `AdminOrdersPage` получает `key`, чтобы состояние не «протекало» между двумя режимами.
- **Формы:** React Hook Form + Zod; числовые поля — `register(..., {valueAsNumber:true})` (а не `z.coerce`, ломает типы). Ошибки backend: field-level (400) → `setError`, остальное (401/409/403) → общий баннер (`lib/apiError.ts`).
- **Фильтры:** в каталоге — в URL (`useSearchParams`, ссылки шарятся); в админке — локальный state. Поиск/цены с debounce 300 мс; любое изменение фильтра сбрасывает страницу на 1.
- **UI:** Tailwind v4, mobile-first; широкие таблицы обёрнуты в `overflow-x-auto` + `min-w-[..]`; проверялось на 375px. Валюта — USD (`Intl.NumberFormat`), интерфейс русский.
- Dev-прокси Vite: `/api` и `/uploads` → `API_PROXY_TARGET` (по умолчанию `http://localhost:5080`).
