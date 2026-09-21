# 05. Окружение, грабли и рецепты проверки

Всё ниже — реально пойманное по ходу работы. Читать **до первой команды**. Окружение: Windows 10, у пользователя PowerShell; у Claude — инструмент Bash (Git Bash) и PowerShell, песочница десктопного приложения Claude (MSIX).

## Грабли инструментов Claude
1. **Длинные heredoc в Bash-инструменте иногда не парсятся** (`unexpected EOF while looking for matching '`'`), и тогда **ничего из команды не выполняется**. Файлы с кодом писать инструментом **Write/Edit**, а не `cat <<EOF`; в bash — короткие команды. После такой ошибки проверять, что записалось (`ls`, `grep`).
2. **Состояние оболочки не сохраняется между вызовами Bash** (переменные, `cd` внутри `&&`-цепочки не «переезжает»). Всё, что зависит друг от друга, — в одном вызове.
3. **Node не в PATH** у Claude: в каждом вызове `export PATH="/c/Program Files/nodejs:$PATH"` (или полный путь `C:\Program Files\nodejs\node.exe`). В `.claude/launch.json` тоже полный путь.
4. **`UID` в bash — readonly** (id пользователя ОС). Не называть так идентификатор в скриптах: присваивание молча не сработает, и запросы уйдут на чужой id (получишь необъяснимые 404).
5. **curl в Git Bash:** для `-F file=@` — Windows-путь (`D:\...`), не `/d/...`; `grep -P` не работает (локаль) — JSON разбирать `sed -n 's/.*"field":"\([^"]*\)".*/\1/p'` (жадный, берёт **последнее** вхождение — для первого использовать якорь `^{"id":"..."}`).
6. **PowerShell 5.1:** запуск exe в кавычках — через `&`; статический `RandomNumberGenerator.GetBytes(int)` недоступен — `Create()` + `GetBytes($byteArray)`.
7. **Права:** процессы, запущенные пользователем из своих окон, Claude **убить не может** («отказано в доступе»); свои фоновые процессы (`nohup dotnet run &`) — может (`Get-NetTCPConnection -LocalPort N -State Listen` → `Stop-Process`).

## Секреты (самая коварная грабля)
`dotnet user-secrets set`, выполненный из окружения Claude, пишет в **виртуализированный `%APPDATA%`** (`...\AppData\Local\Packages\Claude_*\LocalCache\Roaming\Microsoft\UserSecrets\<id>`), и обычный терминал пользователя этих секретов **не видит**. Симптом у пользователя: backend отвечает `500 An unexpected error occurred` на логин (в консоли `ArgumentNullException ... Encoding.GetBytes`, `Program.cs` про `SigningKey`), а у Claude всё работает. Лечение — пользователь сам в **своём** терминале: `dotnet user-secrets set "Jwt:SigningKey" "<64 случайных байта в base64>"` (готовая команда — в истории; в `backend/src/Store.Api`). Пароли и ключи в git **не** попадают.

## Сборка, миграции и параллельный запуск
- Запущенный backend пользователя (`Store.Api.exe`) **блокирует DLL** → `dotnet build`/`dotnet ef` падают с `MSB3027`. Варианты: (a) попросить пользователя `Ctrl+C` (обязательно для `dotnet ef migrations/database update`); (b) для проверки без остановки — собрать в отдельный каталог и запустить на другом порту:
  `dotnet build src/Store.Api/Store.Api.csproj -o <scratchpad>/apibuild` → `ASPNETCORE_ENVIRONMENT=Development dotnet Store.Api.dll --urls http://localhost:5081` (из этого каталога).
  `dotnet test tests/Store.Application.Tests` собирается независимо и работает при запущенном backend.
- Второй frontend на другом порту с указанием backend: `API_PROXY_TARGET=http://localhost:5081 node node_modules/vite/bin/vite.js --port 5174 --strictPort` (из `frontend/`). Порт **5173** обычно занят dev-сервером пользователя — `preview_start` тогда откажет; открывать `http://localhost:5173` через `navigate`.
- `dotnet-ef` — глобальный, версии 10.x при пакетах 8.x; работает. Миграции: `dotnet ef migrations add <Имя> --project src/Store.Infrastructure --startup-project src/Store.Api --output-dir Persistence/Migrations`, затем `database update` (из `backend/`).
- `dotnet new` на SDK 8.0.418 падал (битая установка) — новые проекты создавать SDK 10 с `-f net8.0`. EF-пакеты — всегда с явной `--version 8.0.11`.
- npm/Vite: TypeScript 6 — без `baseUrl`; в `vite.config.ts` — `import.meta.dirname`, не `__dirname`. React Router ставить `@latest` (v7, патчи безопасности). После `npm install` проверять `npm audit`.
- Docker у пользователя не работает; Postgres — локальная служба `postgresql-x64-18`.

## Браузерные инструменты (Claude_Browser)
- Иногда панель скрыта → viewport 0×0, `read_page` пустой. Лечится `resize_window` с явными `width/height` (например 1100×800); после теста — `preset: desktop`.
- **Скриншоты после смены размера окна бывают обрезаны справа** — это артефакт панели, не баг. Для проверки вёрстки надёжнее `javascript_tool`: `documentElement.scrollWidth` vs `innerWidth`, поиск элементов с `getBoundingClientRect().right > innerWidth`.
- **`window.confirm` в автоматизации отклоняется по умолчанию** → перед кликом по кнопкам с подтверждением подменять: `window.confirm = () => true` (после каждой навигации — заново).
- Диалог выбора файла не автоматизируется — эмулировать `DataTransfer` + событие `change` на скрытом `<input type=file>`.
- Быстрый вход без формы: `fetch('/api/auth/login')` и запись в `localStorage['berutut-auth']` (формат в 02), затем `location.href`.
- `form_input` корректно меняет `<select>` и `<input type=date>` (React реагирует); ввод текста — `computer.type` после клика по `ref`.

## Рецепт проверки любой доработки
1. `cd backend && dotnet test tests/Store.Application.Tests` (юнит-тесты на бизнес-логику обязательны).
2. Реальный HTTP по настоящей БД (curl по `http://localhost:5080/api`, токены — логином): сценарий «до → действие → после» + отрицательные случаи (401/403/404/409/400). InMemory-тесты **не** проверяют трансляцию LINQ в SQL Npgsql — поисковые/сортировочные запросы проверять на живой БД.
3. Браузер: основной путь + ошибки + мобильная ширина (375).
4. `cd frontend && npx tsc -b && npm run lint && npm run build`.
5. Обновить `docs/PROGRESS.md`, `docs/DECISIONS.md`, `docs/context/04-state-and-backlog.md`; коммит (без соавтора) → `git push`.
6. За собой: остановить свои процессы, удалить тестовые данные/временные каталоги/логи; порты пользователя (5080/5173) не занимать без нужды.

## Прочее
- Предупреждения git про LF→CRLF безвредны. `CRLF` в рабочей копии — нормально для Windows.
- Предупреждение `NU1903` (AutoMapper 13.0.1) при каждой сборке — известное, см. 01.
- `psql`: `PGPASSWORD=store "/c/Program Files/PostgreSQL/18/bin/psql.exe" -U store -h localhost -d store`. Роль `postgres` (суперпользователь) Claude не использует — пароль знает только пользователь.
