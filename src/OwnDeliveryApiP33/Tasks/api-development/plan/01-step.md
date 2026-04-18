# ?? Step 01: Domain Entities & Enums

## ?? Мета
Створити базову структуру domain entities та enums для усієї системи на основі уточненого технічного завдання.

## ?? Описання
Цей крок фокусується на визначенні всіх domain entities (сутностей) та enum типів, які будуть використовуватися в системі. Це фундамент для всіх подальших шарів архітектури.

---

## ? Вимоги до виконання

### 1. **Enum Типи** (папка: `Domain/Enums/`)

Створіть наступні enums:

#### a. **UserRole.cs**
```csharp
public enum UserRole
{
    Customer = 0,
    Courier = 1,
    Administrator = 2
}
```

#### b. **UserStatus.cs**
```csharp
public enum UserStatus
{
    Active = 0,
    Blocked = 1,
    Deleted = 2,
    Pending = 3
}
```

#### c. **OrderStatus.cs**
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

#### d. **CourierStatus.cs**
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

#### e. **DocumentStatus.cs**
```csharp
public enum DocumentStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
    Expired = 3
}
```

#### f. **DocumentType.cs**
```csharp
public enum DocumentType
{
    License = 0,
    Insurance = 1,
    VehicleRegistration = 2
}
```

#### g. **PaymentStatus.cs**
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

#### h. **PaymentMethod.cs**
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

#### i. **NotificationType.cs**
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

#### j. **RatingType.cs**
```csharp
public enum RatingType
{
    Courier = 0,
    Delivery = 1,
    Speed = 2
}
```

### 2. **Base Entities** (папка: `Domain/Entities/`)

#### a. **BaseEntity.cs** (Абстрактний базовий клас)
Всі entities повинні наслідуватися від цього класу з:
- `Id: Guid`
- `CreatedAt: DateTime`
- `UpdatedAt: DateTime`

#### b. **User.cs** (Base для всіх користувачів)
Властивості:
- Id, Email, PasswordHash, FullName, PhoneNumber
- AvatarUrl, Role, Status
- IsEmailVerified, EmailVerificationToken
- LastLoginAt, CreatedAt, UpdatedAt

#### c. **Customer.cs** (extends User)
Властивості:
- UserId (FK to User)
- PreferredDeliveryAddress
- SavedAddresses (List<Address>)
- PhoneNumbers (List<string>)
- AverageRating, TotalOrders
- SuccessfulOrders, CancelledOrders

#### d. **Courier.cs** (Оновити існуючий)
Властивості:
- UserId (FK to User)
- LicenseNumber, IsVerified, VerificationDate
- CurrentStatus (CourierStatus enum)
- CurrentLocation (Location obj)
- PhoneNumbers (List<string>)
- Documents (List<CourierDocument>)
- AverageRating, TotalDeliveries, CompletedDeliveries
- CancelledDeliveries, AverageDeliveryTime
- TotalEarnings, BankAccount

#### e. **Administrator.cs** (extends User)
Властивості:
- UserId (FK to User)
- Permissions (List<string>)
- AssignedRegions (List<string>)
- CreatedUsers (List<Guid>) - audit trail

### 3. **Value Objects** (папка: `Domain/ValueObjects/`)

#### a. **Address.cs**
```csharp
public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public string BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string PostalCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description { get; set; }
}
```

#### b. **Location.cs**
```csharp
public class Location
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Accuracy { get; set; }
    public decimal? Altitude { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? Speed { get; set; }
}
```

#### c. **Dimensions.cs**
```csharp
public class Dimensions
{
    public decimal Width { get; set; }  // см
    public decimal Length { get; set; } // см
    public decimal Height { get; set; } // см
    
    public decimal GetVolume() => Width * Length * Height;
}
```

### 4. **Aggregate Entities** (папка: `Domain/Entities/`)

#### a. **Order.cs**
Властивості:
- Id, OrderNumber (unique, human-readable)
- CustomerId (FK), CourierId (FK, nullable)
- Status (OrderStatus enum)
- PickupAddress, DeliveryAddress (Address objects)
- Weight, Dimensions
- Description, SpecialInstructions
- CreatedAt, ScheduledDeliveryTime
- ActualPickupTime, ActualDeliveryTime
- EstimatedDeliveryTime
- Cost, TariffId (FK)
- PaymentStatus, PaymentMethod
- Notes, CancelReason
- StatusHistory (List<OrderStatusHistory>)
- Ratings (List<Rating>)

#### b. **CourierLocation.cs**
Властивості:
- Id, CourierId (FK)
- Location (Location value object)
- Timestamp

#### c. **CourierDocument.cs**
Властивості:
- Id, CourierId (FK)
- DocumentType (enum)
- DocumentNumber, DocumentUrl
- ExpirationDate, Status
- VerifiedAt, VerifiedBy (FK, nullable)
- RejectionReason, UploadedAt

#### d. **Tariff.cs**
Властивості:
- Id, Name, Description
- BaseCost, PricePerKm, PricePerKg
- EstimatedDeliveryTime
- MaxWeight, MaxDimensions
- IsActive
- CreatedAt, UpdatedAt

#### e. **OrderStatusHistory.cs**
Властивості:
- Id, OrderId (FK)
- OldStatus, NewStatus (OrderStatus enum)
- ChangedBy (FK)
- Reason, Timestamp
- Location (Location object, nullable)
- ProofUrl (для фото)

#### f. **Rating.cs**
Властивості:
- Id, OrderId (FK), CourierId (FK), CustomerId (FK)
- Score (1-5)
- Comment, Type (RatingType enum)
- CreatedAt

#### g. **Payment.cs**
Властивості:
- Id, OrderId (FK)
- Amount, Currency
- Status (PaymentStatus enum)
- Method (PaymentMethod enum)
- TransactionId, CreatedAt, CompletedAt
- FailureReason

#### h. **Notification.cs**
Властивості:
- Id, UserId (FK)
- Type (NotificationType enum)
- Title, Message
- Metadata (JSON string)
- IsRead, CreatedAt, ReadAt

---

## ?? Unit Tests

Створіть unit tests у папці `OwnDeliveryApiP33.Tests.Unit/Domain/` для:

### 1. **Value Objects Tests**
- ? Address validation (координати, вулиця, місто)
- ? Location validation (координати в межах)
- ? Dimensions calculation (volume)

### 2. **Entity Validation Tests**
- ? User creation з обов'язковими полями
- ? Order creation з правильним статусом
- ? Courier with location update
- ? Invalid enum values

### 3. **Business Logic Tests**
- ? OrderNumber generation (unique)
- ? Address validation (not null)
- ? Rating score validation (1-5)

---

## ?? Очікувані результати

1. ? Всі enum типи створені та задокументовані
2. ? Всі entity классі створені з правильною структурою
3. ? Value objects реалізовані для Address, Location, Dimensions
4. ? Relationships mellem entities налаштовані (FK)
5. ? Unit tests покривають >80% коду
6. ? Код відповідає SOLID принципам
7. ? Успішна компіляція без помилок
8. ? Entity diagram документований

---

## ?? Критерії приймання

- [ ] Всі 10 enums створені та протестовані
- [ ] Всі entities створені з правильними властивостями
- [ ] Value objects правильно реалізовані
- [ ] Unit test покриття >85%
- [ ] Нема компіляційних помилок
- [ ] DbContext буде перелічені в наступному кроці

---

## ?? Файли для створення

```
Domain/
??? Enums/
?   ??? UserRole.cs
?   ??? UserStatus.cs
?   ??? OrderStatus.cs
?   ??? CourierStatus.cs
?   ??? DocumentStatus.cs
?   ??? DocumentType.cs
?   ??? PaymentStatus.cs
?   ??? PaymentMethod.cs
?   ??? NotificationType.cs
?   ??? RatingType.cs
??? Entities/
?   ??? BaseEntity.cs
?   ??? User.cs
?   ??? Customer.cs
?   ??? Courier.cs (update existing)
?   ??? Administrator.cs
?   ??? Order.cs
?   ??? CourierLocation.cs
?   ??? CourierDocument.cs
?   ??? Tariff.cs
?   ??? OrderStatusHistory.cs
?   ??? Rating.cs
?   ??? Payment.cs
?   ??? Notification.cs
??? ValueObjects/
    ??? Address.cs
    ??? Location.cs
    ??? Dimensions.cs
```

---

## ?? Примітки для розробника

1. **Принцип DDD (Domain-Driven Design):**
   - Entities повинні містити бізнес-логіку
   - Value Objects повинні бути immutable
   - Aggregates (Order, Courier) повинні бути self-contained

2. **Database Constraints:**
   - Email повинен бути unique
   - OrderNumber повинен бути unique
   - DocumentNumber повинен бути unique

3. **Inheritance vs Composition:**
   - Розглядайте TPH (Table-Per-Hierarchy) для User, Customer, Courier
   - Або окремі таблиці для специфічних ролей

4. **Performance Tips:**
   - Додайте індекси на часто шукані поля (Email, OrderNumber)
   - Розглядайте lazy loading для collections

---

**Крок 1 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 4-6 годин
