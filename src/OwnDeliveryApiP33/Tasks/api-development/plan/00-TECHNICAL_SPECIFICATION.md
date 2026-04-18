# ?? OwnDelivery API - Уточнене Технічне Завдання

**Версія:** 1.0  
**Статус:** Активна розробка  
**Дата:** 2024  
**Проект:** Backend сервісу власної доставки

---

## ?? Огляд Проекту

Сервіс управління доставками з поділом ролей для:
- **Адміністратора** - управління системою
- **Кур'єра** - виконання замовлень
- **Клієнта** - створення та відстеження замовлень

**Технологічний стек:** .NET 8, PostgreSQL, JWT Authentication

---

## ??? АРХІТЕКТУРА

```
Presentation Layer (Controllers)
    ?
Application Layer (Services, DTOs, Validators)
    ?
Domain Layer (Entities, Enums, Interfaces)
    ?
Infrastructure Layer (DbContext, Repositories, Migrations)
    ?
Database (PostgreSQL)
```

---

## ?? РОЛІ ТА ДОЗВОЛИ

### 1. **Роль: Клієнт** (Customer)

#### Функціональність:
- ? Реєстрація акаунту
- ? Вхід в систему (JWT)
- ? Редагування профілю
- ? Створення замовлення
- ? Перегляд своїх замовлень
- ? Відстеження статусу замовлення в реальному часі
- ? Оцінка послуги після доставки
- ? Скасування замовлення (якщо не прийнято)
- ? Перегляд даних інших користувачів
- ? Управління кур'єрами

#### API Endpoints:
```
POST   /api/v1/auth/register              - Реєстрація
POST   /api/v1/auth/login                 - Вхід
POST   /api/v1/auth/refresh               - Оновити токен
POST   /api/v1/auth/logout                - Вихід

GET    /api/v1/customers/profile          - Мій профіль
PUT    /api/v1/customers/profile          - Редагування профілю
GET    /api/v1/customers/profile/avatar   - Аватар
PUT    /api/v1/customers/profile/avatar   - Завантажити аватар

POST   /api/v1/orders                     - Створити замовлення
GET    /api/v1/orders                     - Мої замовлення
GET    /api/v1/orders/{id}                - Деталі замовлення
PUT    /api/v1/orders/{id}/cancel         - Скасувати замовлення
PUT    /api/v1/orders/{id}/rate           - Оцінити доставку

GET    /api/v1/orders/{id}/tracking       - Відстеження (real-time)
GET    /api/v1/orders/{id}/history        - Історія статусів
```

---

### 2. **Роль: Кур'єр** (Courier)

#### Функціональність:
- ? Реєстрація акаунту
- ? Вхід в систему (JWT)
- ? Редагування профілю
- ? Завантажити документи (посвідчення, документи авто)
- ? Перегляд доступних замовлень
- ? Прийняття замовлення
- ? Відмова від замовлення
- ? Оновлення своєї геолокації
- ? Зміна статусу замовлення
- ? Додання фотодокументів при доставці
- ? Перегляд своїх статистик
- ? Перегляд даних інших кур'єрів
- ? Управління замовленнями інших кур'єрів

#### API Endpoints:
```
POST   /api/v1/auth/register-courier      - Реєстрація кур'єра
POST   /api/v1/auth/login                 - Вхід

GET    /api/v1/couriers/profile           - Мій профіль
PUT    /api/v1/couriers/profile           - Редагування профілю
POST   /api/v1/couriers/documents         - Завантажити документи
GET    /api/v1/couriers/documents         - Мої документи
PUT    /api/v1/couriers/documents/{id}    - Оновити документ
DELETE /api/v1/couriers/documents/{id}    - Видалити документ

GET    /api/v1/couriers/orders/available  - Доступні замовлення
POST   /api/v1/couriers/orders/{id}/accept - Прийняти замовлення
POST   /api/v1/couriers/orders/{id}/decline - Відмовити замовлення
GET    /api/v1/couriers/orders/active     - Активні замовлення
GET    /api/v1/couriers/orders/completed  - Виконані замовлення

POST   /api/v1/couriers/location          - Оновити геолокацію
GET    /api/v1/couriers/location          - Моя поточна локація

PUT    /api/v1/couriers/orders/{id}/status - Оновити статус замовлення
POST   /api/v1/couriers/orders/{id}/proof  - Додати доказ доставки (фото)

GET    /api/v1/couriers/stats             - Мої статистики
GET    /api/v1/couriers/earnings          - Мої заробітки
GET    /api/v1/couriers/ratings           - Мої оцінки
```

---

### 3. **Роль: Адміністратор** (Administrator)

#### Функціональність:
- ? Реєстрація адміністратора (тільки інший адмін)
- ? Перегляд всіх користувачів
- ? Блокування/розблокування користувачів
- ? Перегляд всіх замовлень
- ? Фільтрування замовлень за різними критеріями
- ? Перегляд статистики системи
- ? Управління тарифами доставки
- ? Перегляд звітів по кур'єрам
- ? Схвалення документів кур'єрів
- ? Управління правилами доставки

#### API Endpoints:
```
GET    /api/v1/admin/users                - Всі користувачі
GET    /api/v1/admin/users/{id}           - Деталі користувача
PUT    /api/v1/admin/users/{id}/status    - Блокувати/розблокувати
DELETE /api/v1/admin/users/{id}           - Видалити користувача

GET    /api/v1/admin/orders               - Всі замовлення (з фільтрацією)
GET    /api/v1/admin/orders/stats         - Статистика замовлень
GET    /api/v1/admin/orders/{id}          - Деталі замовлення
PUT    /api/v1/admin/orders/{id}/assign   - Призначити кур'єра

GET    /api/v1/admin/couriers             - Всі кур'єри
GET    /api/v1/admin/couriers/{id}        - Деталі кур'єра
GET    /api/v1/admin/couriers/{id}/stats  - Статистика кур'єра
PUT    /api/v1/admin/couriers/{id}/status - Активувати/дезактивувати

GET    /api/v1/admin/documents            - Невідповідні документи
PUT    /api/v1/admin/documents/{id}/verify - Схвалити документ
PUT    /api/v1/admin/documents/{id}/reject - Відхилити документ

GET    /api/v1/admin/tariffs              - Список тарифів
POST   /api/v1/admin/tariffs              - Створити тариф
PUT    /api/v1/admin/tariffs/{id}         - Оновити тариф
DELETE /api/v1/admin/tariffs/{id}         - Видалити тариф

GET    /api/v1/admin/reports              - Звіти (різні типи)
GET    /api/v1/admin/dashboard            - Dashboard з KPI
```

---

## ?? СУТНОСТІ (ENTITIES)

### 1. **User** (Base для всіх користувачів)
```
- Id: Guid
- Email: string (unique, indexed)
- PasswordHash: string
- FullName: string
- PhoneNumber: string
- AvatarUrl: string
- Role: UserRole enum
- Status: UserStatus enum (Active, Blocked, Deleted)
- CreatedAt: DateTime
- UpdatedAt: DateTime
- LastLoginAt: DateTime?
- IsEmailVerified: bool
- EmailVerificationToken: string?
```

### 2. **Customer** (extends User)
```
- UserId: Guid (FK)
- PreferredDeliveryAddress: string?
- SavedAddresses: List<Address>
- PhoneNumbers: List<string>
- AverageRating: decimal
- TotalOrders: int
- SuccessfulOrders: int
- CancelledOrders: int
```

### 3. **Courier** (extends User)
```
- UserId: Guid (FK)
- LicenseNumber: string
- IsVerified: bool
- VerificationDate: DateTime?
- CurrentStatus: CourierStatus enum (Available, OnDelivery, Offline, OnBreak)
- CurrentLocation: Location
- PhoneNumbers: List<string>
- Documents: List<CourierDocument>
- AverageRating: decimal
- TotalDeliveries: int
- CompletedDeliveries: int
- CancelledDeliveries: int
- AverageDeliveryTime: TimeSpan
- TotalEarnings: decimal
- BankAccount: string
```

### 4. **Order** (Замовлення)
```
- Id: Guid
- OrderNumber: string (unique, human-readable)
- CustomerId: Guid (FK)
- CourierId: Guid? (FK, nullable до прийняття)
- Status: OrderStatus enum
- PickupAddress: Address
- DeliveryAddress: Address
- Weight: decimal (kg)
- Dimensions: Dimensions (ширина x довжина x висота)
- Description: string
- SpecialInstructions: string?
- CreatedAt: DateTime
- ScheduledDeliveryTime: DateTime?
- ActualPickupTime: DateTime?
- ActualDeliveryTime: DateTime?
- EstimatedDeliveryTime: DateTime?
- Cost: decimal
- TariffId: Guid (FK)
- PaymentStatus: PaymentStatus enum
- PaymentMethod: PaymentMethod enum
- Notes: string
- CancelReason: string?
- StatusHistory: List<OrderStatusHistory>
- Ratings: List<Rating>
```

### 5. **Address**
```
- Id: Guid
- City: string
- Street: string
- BuildingNumber: string
- ApartmentNumber: string?
- PostalCode: string
- Latitude: decimal
- Longitude: decimal
- Description: string?
```

### 6. **CourierLocation** (Геолокація)
```
- Id: Guid
- CourierId: Guid (FK)
- Latitude: decimal
- Longitude: decimal
- Accuracy: decimal?
- Altitude: decimal?
- Timestamp: DateTime
- Speed: decimal?
```

### 7. **CourierDocument** (Документи кур'єра)
```
- Id: Guid
- CourierId: Guid (FK)
- DocumentType: DocumentType enum (License, Insurance, VehicleRegistration)
- DocumentNumber: string
- DocumentUrl: string
- ExpirationDate: DateTime
- Status: DocumentStatus enum (Pending, Verified, Rejected)
- VerifiedAt: DateTime?
- VerifiedBy: Guid? (FK to Admin)
- RejectionReason: string?
- UploadedAt: DateTime
```

### 8. **Tariff** (Тариф доставки)
```
- Id: Guid
- Name: string
- Description: string
- BaseCost: decimal
- PricePerKm: decimal
- PricePerKg: decimal
- EstimatedDeliveryTime: TimeSpan
- MaxWeight: decimal
- MaxDimensions: Dimensions
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### 9. **OrderStatusHistory** (Історія змін статусу)
```
- Id: Guid
- OrderId: Guid (FK)
- OldStatus: OrderStatus
- NewStatus: OrderStatus
- ChangedBy: Guid (FK to Courier or Admin)
- Reason: string?
- Timestamp: DateTime
- Location: Location?
- ProofUrl: string? (фото доказу)
```

### 10. **Rating** (Оцінки)
```
- Id: Guid
- OrderId: Guid (FK)
- CourierId: Guid (FK)
- CustomerId: Guid (FK)
- Score: int (1-5)
- Comment: string?
- CreatedAt: DateTime
- Type: RatingType enum (Courier, Delivery, Speed)
```

### 11. **Payment** (Платежі)
```
- Id: Guid
- OrderId: Guid (FK)
- Amount: decimal
- Currency: string
- Status: PaymentStatus enum
- Method: PaymentMethod enum
- TransactionId: string?
- CreatedAt: DateTime
- CompletedAt: DateTime?
- FailureReason: string?
```

### 12. **Notification** (Сповіщення)
```
- Id: Guid
- UserId: Guid (FK)
- Type: NotificationType enum
- Title: string
- Message: string
- Metadata: string (JSON)
- IsRead: bool
- CreatedAt: DateTime
- ReadAt: DateTime?
```

---

## ?? ENUMS

### UserRole
```csharp
public enum UserRole
{
    Customer = 0,
    Courier = 1,
    Administrator = 2
}
```

### UserStatus
```csharp
public enum UserStatus
{
    Active = 0,
    Blocked = 1,
    Deleted = 2,
    Pending = 3
}
```

### OrderStatus
```csharp
public enum OrderStatus
{
    New = 0,
    Accepted = 1,
    WaitingForCourier = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5,
    Failed = 6
}
```

### CourierStatus
```csharp
public enum CourierStatus
{
    Offline = 0,
    Available = 1,
    OnDelivery = 2,
    OnBreak = 3,
    Unavailable = 4
}
```

### DocumentStatus
```csharp
public enum DocumentStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
    Expired = 3
}
```

### PaymentStatus
```csharp
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3,
    Cancelled = 4
}
```

### PaymentMethod
```csharp
public enum PaymentMethod
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    BankTransfer = 3,
    Wallet = 4
}
```

### NotificationType
```csharp
public enum NotificationType
{
    OrderCreated = 0,
    OrderAccepted = 1,
    CourierAssigned = 2,
    DeliveryStarted = 3,
    DeliveryCompleted = 4,
    OrderCancelled = 5,
    PaymentConfirmed = 6,
    DocumentRejected = 7,
    SystemAlert = 8
}
```

---

## ?? БЕЗПЕКА

### Authentication & Authorization
- ? JWT (JSON Web Tokens) для всіх endpoint
- ? Refresh tokens для безпечного оновлення
- ? Role-based access control (RBAC)
- ? Хешування паролів (bcrypt/argon2)
- ? Email verification
- ? Password reset functionality

### Безпечні операції
- ? HTTPS/TLS для всіх комунікацій
- ? Input validation та sanitization
- ? SQL injection prevention (EF Core)
- ? CSRF protection
- ? Rate limiting
- ? API key для сторонніх сервісів

---

## ?? ЗОВНІШНІ ІНТЕГРАЦІЇ

### 1. **Геолокація**
- Google Maps API (маршрути, відстані)
- Геокодинг (адреса ? координати)

### 2. **SMS/Email**
- Twilio (SMS сповіщення)
- SendGrid (Email)

### 3. **Платежі**
- Stripe або LocalBitcoins (обробка платежів)
- Webhook для підтвердження платежу

### 4. **Файлові сховища**
- Azure Blob Storage або AWS S3 (аватари, документи, фото доказів)

---

## ?? ОСНОВНІ БІЗНЕС-ЛОГІКА

### 1. **Розрахунок вартості доставки**
```
Базова вартість: Tariff.BaseCost
Вартість за км: Distance * Tariff.PricePerKm
Вартість за кг: Weight * Tariff.PricePerKg
Урахування часу доставки (поспішна доставка - +20%)
Знижки для постійних клієнтів
```

### 2. **Призначення замовлення кур'єру**
```
Пошук кур'єрів у радіусі з поточною локацією
Фільтрація за статусом (Available, не заняті)
Сортування за рейтингом та часом відповіді
Надсилання пропозиції (Notification)
Таймаут на прийняття (5-10 хвилин)
```

### 3. **Оновлення статусу замовлення**
```
New ? Accepted (адмін/система)
Accepted ? WaitingForCourier (призначення)
WaitingForCourier ? InTransit (кур'єр взяв)
InTransit ? Delivered (доставлено)

Будь-коли ? Cancelled (якщо умови)
InTransit ? Failed (проблема доставки)
```

### 4. **Система рейтингів**
```
Оцінка кур'єра: 1-5 зірок
Оцінка якості доставки: 1-5 зірок
Коментарі до оцінок
Вплив на дозволи (рейтинг < 2.5 ? більше обмежень)
```

---

## ?? АНАЛІТИКА І СТАТИСТИКА

### Клієнт
- Всього замовлень
- Успішних доставок
- Скасованих замовлень
- Середня вартість замовлення
- Середній час доставки
- Список улюблених кур'єрів

### Кур'єр
- Всього замовлень
- Успішних доставок
- Скасованих замовлень
- Середня оцінка
- Середній час доставки
- Загальні заробітки
- Графік активності
- Регіони роботи

### Адміністратор
- Общая статистика
- Активне кур'єрів
- Активні замовлення
- Очікуючі на кур'єра замовлення
- Затримки у доставках
- Revenue за період
- Найпопулярніші маршрути

---

## ?? ТЕСТУВАННЯ

### Unit Tests
- ? Всі Services мають >80% покриття
- ? Всі Controllers мають >75% покриття
- ? Validators протестовані

### Integration Tests
- ? Database операції
- ? API endpoints
- ? Authentication/Authorization
- ? Business logic scenarios

### Безпечні сценарії
- ? Несанціоноване видалення даних
- ? Cross-role access
- ? Invalid input handling

---

## ??? ВИКОРИСТОВАНІ ПАТЕРНИ

- **Clean Architecture** - розділення по шарам
- **Repository Pattern** - абстракція доступу до даних
- **Dependency Injection** - управління залежностями
- **SOLID принципи**:
  - S - Single Responsibility
  - O - Open/Closed
  - L - Liskov Substitution
  - I - Interface Segregation
  - D - Dependency Inversion

---

## ?? ЗАЛЕЖНОСТІ

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.x" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.x" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.x" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.x" />
  <PackageReference Include="FluentValidation" Version="11.x" />
  <PackageReference Include="AutoMapper" Version="13.x" />
  <PackageReference Include="Serilog" Version="3.x" />
  <PackageReference Include="xUnit" Version="2.x" />
  <PackageReference Include="Moq" Version="4.x" />
</ItemGroup>
```

---

## ? КРИТЕРІЇ ЗАВЕРШЕННЯ

- ? Всі entities створені з міграціями
- ? Всі controllers реалізовані з правильними дозволами
- ? Всі services реалізовані з бізнес-логікою
- ? >80% unit test покриття
- ? API документація (Swagger)
- ? Error handling і logging
- ? Performance оптимізація (індекси, caching)
- ? Валідація input даних

---

## ?? ПРИОРИТИЗАЦІЯ

### Phase 1 (MVP) - Тиждень 1-2
- [ ] Authentication (Register, Login, JWT)
- [ ] Customer order creation & viewing
- [ ] Courier profile & order acceptance
- [ ] Basic order tracking

### Phase 2 - Тиждень 3-4
- [ ] Admin panel endpoints
- [ ] Tariffs management
- [ ] Location tracking
- [ ] Rating system

### Phase 3 - Тиждень 5-6
- [ ] Payment integration
- [ ] Document verification
- [ ] Notifications
- [ ] Analytics & Reports

### Phase 4 (Post-MVP)
- [ ] Geolocation optimization
- [ ] Advanced search & filtering
- [ ] Batch operations
- [ ] Mobile app support

---

## ?? DEPLOYMENT

- Docker контейнеризація ? (вже налаштовано)
- PostgreSQL database
- CI/CD pipeline (GitHub Actions)
- Environment-based configuration

---

**Документ створено:** 2024  
**Версія:** 1.0  
**Статус:** Готово до розробки
