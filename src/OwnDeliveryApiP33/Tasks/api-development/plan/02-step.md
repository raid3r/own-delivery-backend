# ?? Step 02: Database Context & Migrations

## ?? Мета
Налаштувати Entity Framework Core DbContext та створити початкову базу даних через migrations.

## ?? Описання
На цьому кроці конфігурується DatabaseContext, встановлюються relationships між entities, налаштовуються constraints, індекси та seed data для розробки.

---

## ? Вимоги до виконання

### 1. **ApplicationDbContext.cs** (Оновити існуючий)

```csharp
// Структура:
public class ApplicationDbContext : DbContext
{
    // DbSets для всіх entities
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<CourierLocation> CourierLocations { get; set; }
    public DbSet<CourierDocument> CourierDocuments { get; set; }
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    // Constructor з options
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }
    
    // OnModelCreating - налаштування всіх relationships та constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Конфігурація для кожної entity:
        // - Primary keys
        // - Foreign keys
        // - Unique constraints
        // - Indexes
        // - Value object mapping
        // - Table names та column names (якщо специфічні)
        // - Seed data
    }
}
```

### 2. **Entity Configurations** (папка: `Infrastructure/Data/Configurations/`)

Створіть IEntityTypeConfiguration для кожної entity:

#### a. **UserConfiguration.cs**
- Primary Key: Id (Guid)
- Required fields: Email (unique), PasswordHash, FullName
- Indexes: Email (для login)
- Relationships: User ? Customer, User ? Courier, User ? Administrator (one-to-one)

#### b. **CustomerConfiguration.cs**
- FK: UserId ? User (one-to-one, cascade delete)
- Relationships: Customer ? Orders (one-to-many)
- Indexes: UserId
- Owned Property: SavedAddresses (EF Core Owned Types)

#### c. **CourierConfiguration.cs**
- FK: UserId ? User (one-to-one)
- Relationships: Courier ? Orders (one-to-many)
- Relationships: Courier ? CourierLocations (one-to-many)
- Relationships: Courier ? CourierDocuments (one-to-many)
- Relationships: Courier ? Ratings (one-to-many)
- Owned Property: CurrentLocation (Location value object)

#### d. **OrderConfiguration.cs**
- Primary Key: Id (Guid)
- Unique: OrderNumber (human-readable, indexed)
- FK: CustomerId ? Customer
- FK: CourierId ? Courier (nullable)
- FK: TariffId ? Tariff
- Owned Properties: PickupAddress, DeliveryAddress, Dimensions
- Relationships: Order ? OrderStatusHistory (one-to-many, cascade delete)
- Relationships: Order ? Ratings (one-to-many)
- Relationships: Order ? Payment (one-to-one)
- Indexes: CustomerId, CourierId, OrderNumber, Status, CreatedAt

#### e. **TariffConfiguration.cs**
- Primary Key: Id (Guid)
- Unique: Name
- Owned Property: MaxDimensions (Dimensions)
- Indexes: IsActive (для фільтрування)

#### f. **CourierLocationConfiguration.cs**
- Primary Key: Id (Guid)
- FK: CourierId ? Courier
- Owned Property: Location (Location value object)
- Indexes: CourierId, Timestamp (для швидкого отримання останньої локації)

#### g. **CourierDocumentConfiguration.cs**
- Primary Key: Id (Guid)
- FK: CourierId ? Courier
- FK: VerifiedBy ? User (nullable)
- Indexes: CourierId, DocumentType, Status

#### h. **OrderStatusHistoryConfiguration.cs**
- Primary Key: Id (Guid)
- FK: OrderId ? Order (cascade delete)
- FK: ChangedBy ? User
- Owned Property: Location (Location value object, nullable)
- Indexes: OrderId, Timestamp

#### i. **RatingConfiguration.cs**
- Primary Key: Id (Guid)
- FK: OrderId ? Order
- FK: CourierId ? Courier
- FK: CustomerId ? Customer
- Indexes: CourierId, CustomerId, CreatedAt

#### j. **PaymentConfiguration.cs**
- Primary Key: Id (Guid)
- FK: OrderId ? Order (one-to-one)
- Unique: TransactionId (nullable)

#### k. **NotificationConfiguration.cs**
- Primary Key: Id (Guid)
- FK: UserId ? User
- Indexes: UserId, IsRead, CreatedAt

#### l. **AdministratorConfiguration.cs**
- FK: UserId ? User (one-to-one, cascade delete)

---

### 3. **EF Core Configuration**

#### a. **Program.cs** (Оновити конфігурацію)
```csharp
// Додати:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OwnDeliveryApiP33.Infrastructure")
    ));
```

#### b. **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=OwnDeliveryDb;Username=postgres;Password=postgres;"
  }
}
```

### 4. **Database Migrations**

Використовуючи EF Core CLI:

```bash
# Додати migration з усіма entities
dotnet ef migrations add AddAllEntities --project OwnDeliveryApiP33 --startup-project OwnDeliveryApiP33

# Оновити БД
dotnet ef database update --project OwnDeliveryApiP33
```

#### Migration має содержати:
- ? Таблиці для всіх entities
- ? Foreign keys з правильними constraints
- ? Unique constraints (Email, OrderNumber, DocumentNumber)
- ? Indexes для performance
- ? Check constraints для enums (якщо RDBMS підтримує)

### 5. **Seed Data** (для розробки)

У DbContext.OnModelCreating():

```csharp
// Sample Admin User
modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Email = "admin@owndelivery.local",
        PasswordHash = "hashed_password_here",
        FullName = "Admin User",
        PhoneNumber = "+380501234567",
        Role = UserRole.Administrator,
        Status = UserStatus.Active,
        IsEmailVerified = true,
        CreatedAt = DateTime.UtcNow
    }
);

// Sample Customer User
modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        Email = "customer@example.com",
        PasswordHash = "hashed_password_here",
        FullName = "John Doe",
        PhoneNumber = "+380501234568",
        Role = UserRole.Customer,
        Status = UserStatus.Active,
        IsEmailVerified = true,
        CreatedAt = DateTime.UtcNow
    }
);

// Sample Tariff
modelBuilder.Entity<Tariff>().HasData(
    new Tariff
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Name = "Standard Delivery",
        Description = "Standard delivery within city",
        BaseCost = 50m,
        PricePerKm = 10m,
        PricePerKg = 5m,
        EstimatedDeliveryTime = TimeSpan.FromHours(2),
        MaxWeight = 50m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    }
);
```

---

## ?? Unit Tests

Створіть tests у `OwnDeliveryApiP33.Tests.Unit/Infrastructure/Data/`:

### 1. **DbContext Configuration Tests**
- ? DbContext can be instantiated
- ? All DbSets are configured
- ? Entity relationships are correct
- ? Unique constraints are enforced

### 2. **Entity Mapping Tests**
- ? Value objects are correctly mapped
- ? Foreign keys are correctly configured
- ? Cascade deletes work correctly
- ? Owned properties are saved correctly

### 3. **Seed Data Tests**
- ? Admin user is seeded correctly
- ? Default tariffs exist
- ? Sample customers can be created

---

## ?? Очікувані результати

1. ? ApplicationDbContext налаштований з усіма entities
2. ? Всі relationships правильно сконфігуровані
3. ? Migrations створені та застосовані
4. ? Database таблиці існують у PostgreSQL
5. ? Seed data завантажена при першому запуску
6. ? Unit tests покривають конфігурацію
7. ? Немає миграційних помилок

---

## ?? Критерії приймання

- [ ] ApplicationDbContext повністю сконфігурований
- [ ] Всі 13 DbSets присутні
- [ ] Migrations успішно застосовані
- [ ] Таблиці існують в БД з правильною схемою
- [ ] Foreign keys встановлені правильно
- [ ] Unique constraints працюють
- [ ] Seed data завантажена
- [ ] Unit test покриття >80%
- [ ] Немає SQL errors при операціях

---

## ?? Файли для створення/оновлення

```
Infrastructure/
??? Data/
?   ??? ApplicationDbContext.cs (update)
?   ??? Configurations/
?   ?   ??? UserConfiguration.cs
?   ?   ??? CustomerConfiguration.cs
?   ?   ??? CourierConfiguration.cs
?   ?   ??? AdministratorConfiguration.cs
?   ?   ??? OrderConfiguration.cs
?   ?   ??? CourierLocationConfiguration.cs
?   ?   ??? CourierDocumentConfiguration.cs
?   ?   ??? TariffConfiguration.cs
?   ?   ??? OrderStatusHistoryConfiguration.cs
?   ?   ??? RatingConfiguration.cs
?   ?   ??? PaymentConfiguration.cs
?   ?   ??? NotificationConfiguration.cs
?   ??? Migrations/
?       ??? [Generated by EF Core]

OwnDeliveryApiP33.Tests.Unit/
??? Infrastructure/
    ??? Data/
        ??? DbContextTests.cs
```

---

## ?? Примітки для розробника

1. **TPH (Table Per Hierarchy) Inheritance:**
   - Розглядайте USE TPH для User/Customer/Courier/Administrator
   - PostgreSQL добре підтримує це через discriminator column

2. **Owned Types (Value Objects):**
   ```csharp
   modelBuilder.Entity<Order>()
       .OwnsOne(o => o.PickupAddress)
       .Property(a => a.City).HasColumnName("PickupCity");
   ```

3. **Indexes для Performance:**
   ```csharp
   modelBuilder.Entity<Order>()
       .HasIndex(o => o.OrderNumber)
       .IsUnique();
   ```

4. **Cascade Delete Rules:**
   - User delete ? Customer/Courier delete
   - Order delete ? OrderStatusHistory, Payments delete
   - Courier delete ? Locations, Documents delete

5. **Shadow Properties** (якщо потрібні):
   - For audit: CreatedBy, ModifiedBy, etc.

---

**Крок 2 з 12**  
**Статус:** ?? Готово до виконання  
**Очікуваний час:** 6-8 годин
