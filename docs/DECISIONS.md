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

## Прочее
- Node.js LTS установлен через winget по ходу этапа 1 (понадобится для frontend, этап 5).
- `dotnet-ef` CLI установлен глобально, версия 10.x (новее пакетов EFCore 8.x в проекте) — миграции генерируются нормально, проблем не было.
