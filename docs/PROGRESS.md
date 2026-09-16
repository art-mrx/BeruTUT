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
4. **Cart + Orders (backend)** — `todo`
5. **Frontend skeleton** — `todo` (Node.js LTS уже установлен)
6. **Каталог (frontend)** — `todo`
7. **Auth (frontend)** — `todo`
8. **Корзина и чекаут (frontend)** — `todo`
9. **Админка (frontend)** — `todo`
10. **Полировка** — `todo`

## Следующий шаг
Этап 4: **Cart + Orders (backend)** — полный цикл заказа, админские эндпоинты заказов, юнит-тесты на расчёт суммы заказа и проверку остатков (ТЗ п.9).
