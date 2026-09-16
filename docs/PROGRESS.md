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
2. **Auth** — `todo` (кастомный JWT, не Identity — см. решение)
3. **Categories + Products (backend)** — `todo`
4. **Cart + Orders (backend)** — `todo`
5. **Frontend skeleton** — `todo` (Node.js LTS уже установлен)
6. **Каталог (frontend)** — `todo`
7. **Auth (frontend)** — `todo`
8. **Корзина и чекаут (frontend)** — `todo`
9. **Админка (frontend)** — `todo`
10. **Полировка** — `todo`

## Следующий шаг
Этап 2: **Auth** — регистрация, логин, кастомный JWT (access + refresh), роли, `GET /api/auth/me`.
