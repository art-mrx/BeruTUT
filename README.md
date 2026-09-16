# BeruTUT

Учебный/портфолио-проект: полнофункциональный интернет-магазин на .NET 8 + React. Публичная часть (каталог, корзина, оформление заказа) и админ-панель (товары, категории, заказы) в одном репозитории.

Полное техническое задание — [TECH_SPEC.md](TECH_SPEC.md). Пошаговый статус разработки и обоснование ключевых решений — в [docs/PROGRESS.md](docs/PROGRESS.md) и [docs/DECISIONS.md](docs/DECISIONS.md) (там же — детали, которые не поместились сюда: почему выбраны те или иные версии пакетов, найденные и исправленные по ходу разработки нестыковки между ТЗ и реализацией и т.п.).

## Стек

**Backend** (`backend/`) — .NET 8, ASP.NET Core Web API, слоистая архитектура (Domain/Application/Infrastructure/Api), EF Core + PostgreSQL, MediatR (CQRS), FluentValidation, AutoMapper, Serilog, кастомный JWT (access + rotating refresh tokens), Swagger, xUnit.

**Frontend** (`frontend/`) — React 18 + TypeScript + Vite, React Router, TanStack Query, Zustand, React Hook Form + Zod, Tailwind CSS, Axios.

## Структура репозитория

```
backend/    — .NET решение (Store.slnx)
frontend/   — React/Vite приложение
docs/       — прогресс разработки и техническая документация решений
TECH_SPEC.md — исходное техническое задание
```

## Требования

- .NET 8 SDK
- Node.js 20+ (LTS)
- PostgreSQL 16+ (локально или через Docker)

## Запуск в разработке

### 1. База данных

Локальный PostgreSQL: создать роль/БД (пароль — только для локальной разработки, не секрет):

```bash
psql -U postgres -h localhost -f backend/scripts/setup-local-db.sql
```

### 2. Backend

```bash
cd backend
dotnet user-secrets set "Jwt:SigningKey" "<любая длинная случайная строка>" --project src/Store.Api
dotnet user-secrets set "AdminSeed:Password" "<пароль для первого админ-аккаунта>" --project src/Store.Api
dotnet ef database update --project src/Store.Infrastructure --startup-project src/Store.Api
dotnet run --project src/Store.Api --urls "http://localhost:5080"
```

При первом запуске в Development-окружении автоматически создаётся админ-пользователь (email — `AdminSeed:Email` из `appsettings.json`, пароль — тот, что задан выше через user-secrets).

Swagger UI: http://localhost:5080/swagger

Тесты: `dotnet test` (из `backend/`).

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Откроется на http://localhost:5173, dev-сервер проксирует `/api` и `/uploads` на backend (порт 5080) — отдельная настройка CORS для локальной разработки не нужна.

## Docker

`docker-compose.yml` и `backend/src/Store.Api/Dockerfile` подготовлены (Postgres + API), но не проверялись в реальном запуске — на момент написания Docker был недоступен на машине разработки. Frontend-сервис в compose-файле пока закомментирован.

## Деплой

По плану — backend на Railway/Render, frontend на Vercel, БД на Render/Neon. Не выполнялся.
