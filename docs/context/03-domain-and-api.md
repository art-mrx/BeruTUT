# 03. Доменная модель, бизнес-правила, API

## Сущности (Store.Domain/Entities)
| Сущность | Ключевое |
|---|---|
| `User` | `Email` (уникальный), `PasswordHash`, `FullName`, `Role` (Customer/Admin), `IsBlocked`, `BlockedAt`, `CreatedAt`; 1:1 `Cart`, 1:N `Orders`, `RefreshTokens` |
| `Category` | `Name`, `Slug` (уникальный), `ParentCategoryId?` (вложенность поддержана в схеме и в админке; в каталоге не используется) |
| `Product` | `Name`, `Slug` (уникальный), `Description`, `Price` decimal(18,2), `StockQuantity`, `CategoryId`, `IsActive`, `CreatedAt/UpdatedAt`; N `ProductImage` (`Url`, `SortOrder`) |
| `Cart` / `CartItem` | у авторизованного пользователя (создаётся при регистрации); `CartItem` = `ProductId` + `Quantity` (цены **не хранит** — берёт живьём из товара) |
| `Order` / `OrderItem` | `Order`: `Status`, `TotalAmount`, `ShippingAddress`, `ContactPhone`, `CreatedAt`. `OrderItem` — **снапшот**: `ProductName`, `Price`, `Quantity` на момент заказа |
| `RefreshToken` | `TokenHash` (SHA-256, уникальный), `ExpiresAt`, `RevokedAt?`, `ReplacedByTokenHash?` |

Идентификаторы — Guid. Все id в UI показываются как «№» = первые 8 символов (`slice(0,8)`); отдельных сквозных номеров нет. Миграции: `InitialCreate`, `AddRefreshTokens`, `AddOrderContactPhone`, `AddUserBlocking`.

## Бизнес-правила
**Остатки.** Резервируются (списываются) **при оформлении заказа**, а не при подтверждении — чтобы товар не продали второму клиенту, пока админ звонит первому. Подтверждение остаток не меняет; отклонение возвращает товары в наличие (ровно один раз, даже если товар потом скрыли).
- В корзине — мягкая проверка при добавлении/изменении (409 при превышении), при оформлении — повторная авторитетная (между этими моментами остаток мог измениться). Конкурентность (гонка за последнюю единицу) **не защищена** — нет `RowVersion`.
- Добавление в корзину `POST /cart/items` **прибавляет** к существующему количеству; `PUT /cart/items/{id}` **устанавливает** точное (≥1).

**Статусы заказа** (`OrderStatus`, хранится числом — только дописывать в конец: New=0, Processing=1, Shipped=2, Delivered=3, Cancelled=4, Approved=5). В UI: New — «Ожидает подтверждения», Approved — «Одобрен», Processing — «В обработке», Shipped — «Отправлен», Delivered — «Доставлен», **Cancelled — «Отклонён»** (имя из ТЗ сохранено, в интерфейсе — «отклонён»).

| Из | Допустимые переходы |
|---|---|
| New | Approved, Cancelled |
| Approved | Processing, Shipped, Cancelled |
| Processing | Shipped, Cancelled |
| Shipped | Delivered |
| Delivered, Cancelled | — (конечные) |

Недопустимый переход → 409. `AdminOrderDto.AllowedNextStatuses` отдаёт допустимые шаги, UI рисует по ним кнопки (правила на фронте не дублируются). Сценарий: клиент оформил (New, остаток списан) → админ звонит → «Подтвердить» (Approved) либо «Отклонить» (Cancelled, остаток возвращён).

**Товары.** `DELETE /products/{id}` — только деактивация (`IsActive=false`); публичный каталог скрывает неактивные, админский список (`/admin/products`) показывает все. Вернуть в продажу — `PUT` с `isActive:true` (**`isActive` в теле PUT обязателен**: пропущенный bool молча станет `false`). Публичная карточка неактивного — 404. `GET /products/{idOrSlug}` принимает и Guid, и slug.
**Категории.** Удаление категории с товарами/подкатегориями → 409.
**Пользователи.** Блокировать можно только Customer; администратора — 409. Подробности — в 02 (мгновенное действие блокировки).
**Валидация:** пароль ≥ 8 символов; slug `^[a-z0-9]+(-[a-z0-9]+)*$`; цена > 0; остаток ≥ 0; телефон в заказе обязателен `^\+?[0-9\s\-()]{6,32}$`; адрес ≤ 500; загрузка изображений — jpeg/png/webp до 10 МБ.

## Эндпоинты (`/api`, Swagger — `/swagger` в Development)
Доступ: **P** — публичный, **U** — любой авторизованный, **A** — только Admin.

| Метод и путь | Дост. | Назначение |
|---|---|---|
| `POST /auth/register` · `/login` · `/refresh` | P | регистрация, вход, ротация токенов; ответ `{accessToken, refreshToken, ...ExpiresAt, user}` |
| `POST /auth/logout` (body `{refreshToken}`) · `GET /auth/me` | U | отзыв refresh-токена; текущий пользователь |
| `GET /categories` · `/categories/{id}` | P | список / одна |
| `POST /categories` · `PUT`/`DELETE /categories/{id}` | A | CRUD |
| `GET /products` | P | `page,pageSize,categoryId,search,sortBy(price_asc\|price_desc\|newest),minPrice,maxPrice` |
| `GET /products/{idOrSlug}` | P | карточка |
| `POST /products` · `PUT`/`DELETE /products/{id}` · `POST /products/{id}/images` (multipart `file`) | A | CRUD, деактивация, загрузка фото |
| `GET /admin/products` | A | как публичный список, но **включая неактивные** |
| `GET /cart` · `DELETE /cart` · `POST /cart/items` `{productId,quantity}` · `PUT`/`DELETE /cart/items/{itemId}` | U | корзина текущего пользователя |
| `POST /orders` `{shippingAddress, contactPhone}` · `GET /orders` · `GET /orders/{id}` | U | оформление из корзины; только свои заказы |
| `GET /admin/orders` | A | `page,pageSize,status,createdFrom(вкл.),createdTo(искл.),search(фрагмент номера),sortBy(newest\|oldest)`; в заказе — клиент, телефон, адрес, позиции, `allowedNextStatuses` |
| `PUT /admin/orders/{id}/status` `{status}` | A | смена статуса (с правилами и возвратом остатка при Cancelled) |
| `GET /admin/users` | A | `page,pageSize,search(имя/email/номер),isBlocked,sortBy`; в ответе id, email, имя, роль, дата, `ordersCount`, `isBlocked`, `blockedAt` |
| `POST /admin/users/{id}/block` · `/unblock` | A | блокировка/разблокировка (идемпотентно) |

Формат ошибок — ProblemDetails (`title,status,detail,instance,errors?`). Формат страницы — `{items,page,pageSize,totalCount,totalPages}`. Статусы HTTP: 400 валидация, 401 не аутентифицирован/неверный пароль, 403 нет роли или аккаунт заблокирован, 404, 409 конфликт.
