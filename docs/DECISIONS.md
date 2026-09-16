# Ключевые решения и договорённости

Не переисследовать заново — сверяться с этим файлом перед изменениями в соответствующих областях.

## Структура репозитория
- `backend/` — .NET решение (`Store.slnx`), `src/` — 4 проекта слоистой архитектуры, `tests/` — юнит-тесты.
- `frontend/` — React/Vite приложение (появится на этапе 5).
- `docs/` — этот прогресс-трекер и решения (не часть ТЗ, только для ускорения работы).
- Git-репозиторий уже существовал на GitHub (`art-mrx/BeruTUT`) с одним коммитом (README). Локальный `git init` + `git merge --allow-unrelated-histories` объединил историю.

## SDK / версии
- Установлены .NET SDK 8/9/10 одновременно. `dotnet new` на SDK 8.0.418 падал с `FileNotFoundException` в template engine (похоже, битая установка) — решили не чинить, а таргетировать `net8.0` через `-f net8.0` на SDK 10 (мультитаргетинг работает нормально). `global.json` НЕ используется.
- Все EF Core / ASP.NET пакеты явно пиннятся на `8.0.11` (последний стабильный 8.x на момент старта), иначе `dotnet add package` без версии подтягивает 10.x, несовместимый с `net8.0`.
- MediatR 12.4.1, AutoMapper 13.0.1 — последние версии до перехода на платную лицензию (более новые мажорные версии — коммерческие, не брать не глядя).
- Solution-файл в новом XML-формате `.slnx` (дефолт для `dotnet new sln` на SDK 10) — это нормально, открывается в VS/Rider/CLI.

## Auth
- Выбран **кастомный JWT** (не ASP.NET Core Identity) — решение пользователя.
- **Доступ к данным из Application-слоя**: вместо generic `IRepository<T>` (как буквально предлагал ТЗ п.3) сделан `IApplicationDbContext` (Store.Application/Common/Interfaces) с `DbSet<T>` — реализует `StoreDbContext`. Это стандартный паттерн для связки CQRS+MediatR+EF Core (Jason Taylor Clean Architecture template) — EF Core `DbSet` уже даёт repository+unit-of-work, обёртка над ним поверх лишняя. Application-проект из-за этого зависит от пакета `Microsoft.EntityFrameworkCore` (только ради типа `DbSet<T>`, без провайдера БД) — это нормально и намеренно.
- **Refresh-токены — senior-подход**: таблица `RefreshTokens` в БД, хранится НЕ сырой токен, а его SHA-256 хеш (`TokenHash`, уникальный индекс). Сырой токен — 64 случайных байта (`RandomNumberGenerator`), отдаётся клиенту один раз.
  - **Ротация**: при каждом `/api/auth/refresh` старый токен помечается `RevokedAt`, выдаётся новый (тот же как OAuth refresh token rotation).
  - **Reuse-detection**: если пришёл токен, у которого `RevokedAt` уже проставлен (т.е. его уже использовали или отозвали) — считаем это признаком кражи и отзываем ВСЕ активные refresh-токены пользователя, требуем перелогин. Из-за этого logout тоже отдаёт "reuse detected" при повторном рефреше уже разлогиненной сессии — это нормально и безопасно (не различаем "украли" от "разлогинились", в обоих случаях доступ закрыт).
  - Пароли — `Microsoft.AspNetCore.Identity.PasswordHasher<User>` (пакет `Microsoft.Extensions.Identity.Core`, НЕ полный Identity — только алгоритм хеширования, PBKDF2). Это то, что подразумевало ТЗ п.6 ("BCrypt/Identity hasher").
  - JWT signing key сгенерирован (64 случайных байта, base64) и лежит в `dotnet user-secrets` проекта Store.Api (НЕ в git). Локально уже настроено — если БД/секреты потеряются на новой машине, надо заново: `dotnet user-secrets set "Jwt:SigningKey" "<base64>"` внутри `backend/src/Store.Api`.
- **`POST /api/auth/logout`** добавлен сверх списка эндпоинтов из ТЗ п.5 — раз завели БД-backed refresh-токены с возможностью отзыва, endpoint для явного отзыва напрашивался сам собой.
- **Единая обработка ошибок** (ТЗ п.6) реализована уже на этом этапе, не отложена на "Полировку" (этап 10) — `Store.Api/Middleware/ExceptionHandlingMiddleware.cs` мапит кастомные исключения (`Store.Application.Common.Exceptions.*`) в ProblemDetails: `ValidationException`→400, `AuthenticationException`→401, `NotFoundException`→404, `ConflictException`→409, всё остальное→500 (с логом полного стектрейса через Serilog, наружу — только общее сообщение).
- FluentValidation подключен через `ValidationBehavior` — MediatR pipeline behavior, все валидаторы гоняются автоматически перед хендлером, ничего вручную вызывать не надо.
- Namespace-ловушка C#: папка/namespace команды `RefreshToken` конфликтовала с именем сущности `Store.Domain.Entities.RefreshToken` (компилятор ищет вложенные namespace'ы раньше using-алиасов) — решено переименованием в `Commands/RefreshTokens` (множественное число). Если заводите новую команду/namespace с именем, совпадающим с именем сущности — сразу называйте во множественном числе, чтобы не наступить на то же самое.

## Локальная БД
- Docker на устройстве пользователя пока не работает — используем локально установленный **PostgreSQL 18** (служба `postgresql-x64-18` в Windows), НЕ контейнер.
- Роль/БД для разработки: `store` / `store` (пароль), БД `store`. Создаются скриптом `backend/scripts/setup-local-db.sql` — **пользователь запускает сам** (там его пароль от суперпользователя `postgres`, Claude пароли не вводит).
- Connection string лежит в `backend/src/Store.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`) — это dev-заглушка с несекретным локальным паролем, для прод/реальных секретов при деплое переходим на env vars / User Secrets (см. ТЗ п.6).
- Docker + docker-compose добавим позже (пользователь хочет подключить, когда почините Docker на устройстве) — Dockerfile/compose файлы можно написать заранее, не запуская.

## Categories + Products
- Поиск по товарам (`search`) сделан через `p.Name.ToLower().Contains(term)` (а не Npgsql-специфичный `EF.Functions.ILike`) — чтобы Application-слой не тянул зависимость на конкретного провайдера БД (Npgsql), которая должна жить только в Infrastructure. Небольшая цена — не использует GIN/trigram индекс, для MVP-масштаба не важно.
- Списки используют `AutoMapper.ProjectTo<T>()` (не `.Map()` после загрузки сущностей) — проекция сразу в SQL, без лишних колонок/N+1.
- `DELETE /api/products/{id}` — это **деактивация** (`IsActive=false`), не удаление строки (см. ТЗ "удалить/деактивировать"), чтобы не ломать FK из OrderItem/CartItem при будущих заказах.
- `DELETE /api/categories/{id}` — жёсткое удаление, но с явной проверкой в хендлере: если есть товары или подкатегории — 409 Conflict с понятным сообщением (а не сырая ошибка БД от FK `Restrict`).
- **Загрузка изображений**: `IFileStorageService` (Application) → `LocalFileStorageService` (Infrastructure) сохраняет файлы на диск в `Store.Api/wwwroot/uploads/products/`, раздаётся через `app.UseStaticFiles()`. Это временное dev-решение — когда дойдём до деплоя, заменить на реализацию поверх S3/Cloudinary/Azure Blob и подменить только DI-регистрацию (интерфейс в Application не поменяется). Сами файлы — в `.gitignore` (папка с `.gitkeep`, чтобы структура была в репо).
- **Admin-пользователь для тестирования**: `Store.Infrastructure/Persistence/Seed/DbSeeder.cs` создаёт одного Admin-пользователя при старте в Development, если Admin ещё нет. Email/FullName — в `appsettings.json` (`AdminSeed:Email`/`FullName`, не секрет), пароль — только в `dotnet user-secrets` (`AdminSeed:Password`); если секрет не задан, сидинг просто пропускается с warning в логах. Текущий пароль пользователю продиктован в чате при создании — если потерян, сгенерировать новый: `dotnet user-secrets set "AdminSeed:Password" "<новый>" --project src/Store.Api`, удалить admin-строку из таблицы `Users` и перезапустить API.
- В локальной dev-БД сейчас лежат тестовые категория "Electronics" и товары "USB Cable"/"Gaming Laptop" (созданы при smoke-тесте) — оставлены специально как образец данных для будущей разработки фронтенда, не нужно удалять.

## Cart + Orders
- Namespace-ловушка (та же, что с `RefreshToken` в Auth) снова всплыла: папка фичи называется `Features/Carts` (множественное число), а не `Cart`, чтобы не конфликтовать с сущностью `Store.Domain.Entities.Cart`. Общее правило зафиксировано — см. запись про Auth выше.
- **Cart vs Order — где хранится цена**: `CartItem` цену не хранит — при отображении корзины цена берётся живьём из `Product.Price` (через AutoMapper `MapFrom`). `OrderItem` — наоборот, хранит `ProductName`/`Price` как снапшот на момент оформления заказа (так и было заложено в доменной модели ещё на этапе 1). Это осознанно: корзина показывает актуальную цену, заказ — историю по факту покупки, даже если товар потом подорожает/переименуется/удалится.
- **Проверка остатков — два уровня**: мягкая проверка при добавлении/изменении позиции в корзине (`AddCartItemCommandHandler`/`UpdateCartItemCommandHandler`, 409 при превышении) и повторная авторитетная проверка при оформлении заказа (`CreateOrderCommandHandler`) — между добавлением в корзину и чекаутом остаток мог измениться (кто-то другой купил последний). Чекаут никогда не доверяет более ранним проверкам.
- **Гонки/конкурентность**: сейчас нет optimistic concurrency token на `Product.StockQuantity` — при одновременных заказах на последнюю единицу теоретически возможен race condition (оба запроса проходят проверку, оба списывают остаток в минус). Для пет-проекта это сознательно не реализовано (не было в ТЗ), но если понадобится — добавить `[ConcurrencyCheck]`/`RowVersion` на `Product` и обрабатывать `DbUpdateConcurrencyException` в `CreateOrderCommandHandler`.
- **Тесты бизнес-логики** (ТЗ п.9, `tests/Store.Application.Tests`): решили тестировать реальные MediatR-хендлеры (`CreateOrderCommandHandler`, `AddCartItemCommandHandler`) через **EF Core InMemory provider**, а не через моки `IApplicationDbContext`/`DbSet` — мокать LINQ-запросы поверх `DbSet<T>` в Moq болезненно и хрупко; InMemory даёт реальное поведение EF Core (Include, LINQ-трансляция) почти бесплатно. `IMapper` в тестах — настоящий (собран из всех профилей сборки), не мок — заодно ловит ошибки конфигурации маппинга (см. `MapperProfilesTests`).
- **Баг с enum в JSON-теле**: `System.Text.Json` по умолчанию не умеет `"status":"Processing"` → `OrderStatus` (ждёт число). Пофикшено глобально через `AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))` в `Program.cs`. Если будут другие enum-поля в теле запроса (не query-параметры — те и так работали через стандартный model binding) — этот конвертер их тоже покроет.
- `PUT /api/admin/orders/{id}/status` не имеет state-machine валидации переходов (например, из `Delivered` обратно в `New`) — намеренно, ТЗ этого не требует, это MVP-simplification.

## Прочее
- Node.js LTS установлен через winget по ходу этапа 1 (понадобится для frontend, этап 5).
- `dotnet-ef` CLI установлен глобально, версия 10.x (новее пакетов EFCore 8.x в проекте) — миграции генерируются нормально, проблем не было.
