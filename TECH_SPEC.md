# Техническое задание: Интернет-магазин (пет-проект)

## 1. Общее описание

Разработка полнофункционального интернет-магазина с публичной частью (каталог, корзина, оформление заказа) и админ-панелью для управления товарами, категориями и заказами.

Цель проекта — учебный/портфолио-проект, демонстрирующий владение full-stack разработкой на .NET + React.

## 2. Технологический стек

### Backend
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core + PostgreSQL
- ASP.NET Core Identity или кастомная JWT-аутентификация (access + refresh token)
- FluentValidation — валидация входных DTO
- AutoMapper — маппинг Entity <-> DTO
- MediatR — паттерн CQRS для use cases
- Serilog — структурированное логирование
- Swagger / OpenAPI
- xUnit + Moq (или NSubstitute) + FluentAssertions — юнит-тесты

### Frontend
- React 18 + TypeScript + Vite
- React Router v6
- TanStack Query (React Query) — работа с сервером, кеширование
- Zustand — глобальный стейт (корзина, auth)
- React Hook Form + Zod — формы и валидация
- Tailwind CSS — стилизация
- Axios — HTTP-клиент

### Инфраструктура
- Docker + docker-compose (api, postgres, frontend)
- GitHub Actions — CI (build + тесты на пуш в main)
- Хостинг: backend — Railway/Render, frontend — Vercel, БД — Render/Neon (можно поднять там же, где backend)

## 3. Архитектура backend

Слоистая архитектура (упрощённая Clean Architecture):

```
src/
  Store.Api/              — контроллеры, middleware, DI-конфигурация, Program.cs
  Store.Application/      — DTO, интерфейсы сервисов, MediatR-хендлеры (команды/запросы), валидаторы
  Store.Domain/           — сущности, enum'ы, доменные интерфейсы (IRepository и т.д.)
  Store.Infrastructure/   — EF Core (DbContext, конфигурации, миграции), репозитории, внешние сервисы (хранение файлов, email)
tests/
  Store.Application.Tests/
  Store.Api.Tests/ (опционально, интеграционные тесты через WebApplicationFactory)
```

Принципы:
- Контроллеры тонкие — только приём запроса, вызов MediatR-хендлера, возврат результата.
- Бизнес-логика — в Application-слое.
- Domain не зависит ни от чего.
- Infrastructure реализует интерфейсы, определённые в Domain/Application.

## 4. Доменная модель

### User
- Id (Guid)
- Email (unique)
- PasswordHash
- FullName
- Role (enum: Customer, Admin)
- CreatedAt

### Category
- Id (Guid)
- Name
- Slug (unique)
- ParentCategoryId (nullable, для вложенных категорий — опционально в MVP)

### Product
- Id (Guid)
- Name
- Slug
- Description
- Price (decimal)
- StockQuantity (int)
- CategoryId (FK)
- ImageUrls (список, отдельная таблица ProductImage или JSON-массив)
- IsActive (bool) — для скрытия товара без удаления
- CreatedAt, UpdatedAt

### Cart
- Id (Guid)
- UserId (FK, nullable для гостевой корзины — опционально)
- Items: список CartItem

### CartItem
- Id (Guid)
- CartId (FK)
- ProductId (FK)
- Quantity (int)

### Order
- Id (Guid)
- UserId (FK)
- Status (enum: New, Processing, Shipped, Delivered, Cancelled)
- TotalAmount (decimal)
- ShippingAddress
- CreatedAt
- Items: список OrderItem

### OrderItem
- Id (Guid)
- OrderId (FK)
- ProductId (FK)
- ProductName (снапшот на момент заказа)
- Price (снапшот цены на момент заказа)
- Quantity (int)

## 5. API — функциональные требования

### Auth
- `POST /api/auth/register` — регистрация (email, password, fullName)
- `POST /api/auth/login` — логин, возвращает access + refresh token
- `POST /api/auth/refresh` — обновление токена
- `GET /api/auth/me` — текущий пользователь (требует авторизации)

### Categories
- `GET /api/categories` — список категорий (публично)
- `GET /api/categories/{id}` — одна категория
- `POST /api/categories` — создать (Admin)
- `PUT /api/categories/{id}` — обновить (Admin)
- `DELETE /api/categories/{id}` — удалить (Admin)

### Products
- `GET /api/products` — список с query-параметрами: `page`, `pageSize`, `categoryId`, `search`, `sortBy` (price_asc, price_desc, newest), `minPrice`, `maxPrice`
- `GET /api/products/{id}` — карточка товара
- `POST /api/products` — создать (Admin)
- `PUT /api/products/{id}` — обновить (Admin)
- `DELETE /api/products/{id}` — удалить/деактивировать (Admin)
- `POST /api/products/{id}/images` — загрузить изображение (Admin)

### Cart
- `GET /api/cart` — текущая корзина пользователя
- `POST /api/cart/items` — добавить товар (productId, quantity)
- `PUT /api/cart/items/{itemId}` — изменить количество
- `DELETE /api/cart/items/{itemId}` — удалить из корзины
- `DELETE /api/cart` — очистить корзину

### Orders
- `POST /api/orders` — оформить заказ из корзины (shippingAddress)
- `GET /api/orders` — список заказов текущего пользователя
- `GET /api/orders/{id}` — детали заказа
- `GET /api/admin/orders` — все заказы (Admin), с фильтром по статусу
- `PUT /api/admin/orders/{id}/status` — смена статуса заказа (Admin)

## 6. Нефункциональные требования

- Все списковые эндпоинты — с пагинацией (page/pageSize, ответ включает totalCount)
- Обработка ошибок — единый формат ответа об ошибке (middleware для глобального exception handling), корректные HTTP-статусы (400/401/403/404/409/500)
- Валидация всех входных DTO через FluentValidation
- CORS настроен для домена фронтенда
- Пароли только в виде хешей (BCrypt/Identity hasher)
- JWT с ограниченным временем жизни access token (~15-30 мин) + refresh token
- Логирование ключевых операций (создание заказа, ошибки) через Serilog
- Секреты (строки подключения, JWT-ключи) — через переменные окружения / User Secrets, не в репозитории

## 7. Frontend — структура

```
src/
  api/            — axios-инстанс, функции запросов к API, типы ответов
  components/     — переиспользуемые UI-компоненты
  features/
    auth/         — формы логина/регистрации, стор auth
    catalog/      — список товаров, карточка, фильтры
    cart/         — корзина, стор корзины
    orders/       — история заказов, оформление
    admin/        — админка (товары, категории, заказы)
  pages/          — страницы, собранные из features
  routes/         — конфигурация роутинга, ProtectedRoute/AdminRoute
  hooks/          — кастомные хуки
  types/          — общие TypeScript-типы
```

### Страницы (MVP)
- `/` — главная / каталог
- `/products/:slug` — карточка товара
- `/cart` — корзина
- `/checkout` — оформление заказа
- `/orders` — история заказов пользователя
- `/orders/:id` — детали заказа
- `/login`, `/register`
- `/admin/products` — управление товарами
- `/admin/categories` — управление категориями
- `/admin/orders` — управление заказами

## 8. Этапы разработки (порядок реализации)

1. **Backend skeleton**: solution, проекты, Docker + Postgres, EF Core DbContext, первая миграция, Swagger
2. **Auth**: регистрация, логин, JWT, роли, `/me`
3. **Categories + Products (backend)**: CRUD, публичные эндпоинты со списком/фильтрами, загрузка изображений
4. **Cart + Orders (backend)**: полный цикл заказа, админские эндпоинты заказов
5. **Frontend skeleton**: Vite-проект, роутинг, Tailwind, API-клиент, React Query
6. **Каталог (frontend)**: список, фильтры, карточка товара
7. **Auth (frontend)**: формы, хранение токена, protected routes
8. **Корзина и чекаут (frontend)**
9. **Админка (frontend)**: CRUD-таблицы для товаров/категорий/заказов
10. **Полировка**: обработка ошибок, адаптивность, README, деплой

## 9. Критерии приёмки MVP

- Пользователь может зарегистрироваться, войти, просмотреть каталог с фильтрами, добавить товары в корзину и оформить заказ.
- Администратор может логиниться, управлять товарами/категориями (CRUD) и менять статусы заказов.
- Все API-эндпоинты задокументированы в Swagger.
- Проект поднимается локально одной командой (`docker-compose up`).
- Есть базовые юнит-тесты на ключевую бизнес-логику (расчёт суммы заказа, проверка остатков).

## 10. Что сознательно вынесено за рамки MVP

- Реальные платежи (только заглушка/Stripe test mode при наличии времени)
- Отзывы и рейтинги товаров
- Многоуровневые вложенные категории
- Email-уведомления
- Мультиязычность/мультивалютность
- Полноценная система прав (только 2 роли: Customer, Admin)
