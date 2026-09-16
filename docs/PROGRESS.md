# Прогресс разработки

Статусы: `done`, `in-progress`, `blocked`, `todo`. Обновлять после каждого этапа.
Порядок этапов — раздел 8 [TECH_SPEC.md](../TECH_SPEC.md).

## Backend

1. **Backend skeleton** — `in-progress`
   - [x] Solution + 4 проекта (Store.Domain/Application/Infrastructure/Api) + Store.Application.Tests
   - [x] Доменные сущности (User, Category, Product, ProductImage, Cart, CartItem, Order, OrderItem)
   - [x] StoreDbContext + Fluent-конфигурации в Store.Infrastructure/Persistence
   - [x] Serilog, Swagger, DI (AddApplication/AddInfrastructure) в Program.cs
   - [x] Первая миграция `InitialCreate` сгенерирована
   - [ ] Миграция применена к локальной БД (ждём, пока пользователь создаст роль/БД — см. [DECISIONS.md](DECISIONS.md#локальная-бд))
   - [ ] Dockerfile + docker-compose.yml (написать, но не запускать — на этом устройстве Docker пока не работает)
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
Ждём подтверждения, что пользователь выполнил `backend/scripts/setup-local-db.sql` через psql, затем:
```bash
cd backend
dotnet ef database update --project src/Store.Infrastructure --startup-project src/Store.Api
```
