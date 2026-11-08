IF OBJECT_ID('dbo.Roles','U') IS NULL
CREATE TABLE dbo.Roles(
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users(
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    RoleId       INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(RoleId),
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME NULL
);
GO

IF OBJECT_ID('dbo.Customers','U') IS NULL
CREATE TABLE dbo.Customers(
    CustomerId     INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(150) NOT NULL,
    Phone          NVARCHAR(30)  NULL,
    Address        NVARCHAR(250) NULL,
    Email          NVARCHAR(150) NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes          NVARCHAR(500) NULL,
    IsActive       BIT NOT NULL DEFAULT 1,
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME NULL
);
GO

IF OBJECT_ID('dbo.Suppliers','U') IS NULL
CREATE TABLE dbo.Suppliers(
    SupplierId     INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(150) NOT NULL,
    Phone          NVARCHAR(30)  NULL,
    Address        NVARCHAR(250) NULL,
    Email          NVARCHAR(150) NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes          NVARCHAR(500) NULL,
    IsActive       BIT NOT NULL DEFAULT 1,
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME NULL
);
GO

IF OBJECT_ID('dbo.Categories','U') IS NULL
CREATE TABLE dbo.Categories(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name       NVARCHAR(100) NOT NULL UNIQUE,
    Notes      NVARCHAR(300) NULL,
    CreatedAt  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.Products','U') IS NULL
CREATE TABLE dbo.Products(
    ProductId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode   NVARCHAR(50) NOT NULL UNIQUE,
    Name          NVARCHAR(150) NOT NULL,
    CategoryId    INT NULL FOREIGN KEY REFERENCES dbo.Categories(CategoryId),
    PurchasePrice DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (PurchasePrice >= 0),
    SalePrice     DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (SalePrice >= 0),
    Quantity      DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
    MinimumStock  DECIMAL(18,2) NOT NULL DEFAULT 0,
    Unit          NVARCHAR(30) NULL,
    Barcode       NVARCHAR(50) NULL,
    Description   NVARCHAR(500) NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME NULL
);
GO

IF OBJECT_ID('dbo.Sales','U') IS NULL
CREATE TABLE dbo.Sales(
    SaleId         INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber  NVARCHAR(30) NOT NULL UNIQUE,
    SaleDate       DATETIME NOT NULL DEFAULT GETDATE(),
    CustomerId     INT NOT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerId),
    Subtotal       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax            DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrandTotal     DECIMAL(18,2) NOT NULL DEFAULT 0,
    Paid           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Remaining      DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentMethod  NVARCHAR(30) NULL,
    Notes          NVARCHAR(500) NULL,
    UserId         INT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.SaleItems','U') IS NULL
CREATE TABLE dbo.SaleItems(
    SaleItemId   INT IDENTITY(1,1) PRIMARY KEY,
    SaleId       INT NOT NULL FOREIGN KEY REFERENCES dbo.Sales(SaleId) ON DELETE CASCADE,
    ProductId    INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductId),
    Quantity     DECIMAL(18,2) NOT NULL CHECK (Quantity > 0),
    SalePrice    DECIMAL(18,2) NOT NULL CHECK (SalePrice >= 0),
    PurchasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount     DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total        DECIMAL(18,2) NOT NULL
);
GO

IF OBJECT_ID('dbo.Purchases','U') IS NULL
CREATE TABLE dbo.Purchases(
    PurchaseId     INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber  NVARCHAR(30) NOT NULL UNIQUE,
    PurchaseDate   DATETIME NOT NULL DEFAULT GETDATE(),
    SupplierId     INT NOT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierId),
    Subtotal       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax            DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrandTotal     DECIMAL(18,2) NOT NULL DEFAULT 0,
    Paid           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Remaining      DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentMethod  NVARCHAR(30) NULL,
    Notes          NVARCHAR(500) NULL,
    UserId         INT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.PurchaseItems','U') IS NULL
CREATE TABLE dbo.PurchaseItems(
    PurchaseItemId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseId     INT NOT NULL FOREIGN KEY REFERENCES dbo.Purchases(PurchaseId) ON DELETE CASCADE,
    ProductId      INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductId),
    Quantity       DECIMAL(18,2) NOT NULL CHECK (Quantity > 0),
    PurchasePrice  DECIMAL(18,2) NOT NULL CHECK (PurchasePrice >= 0),
    Discount       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total          DECIMAL(18,2) NOT NULL
);
GO

IF OBJECT_ID('dbo.Payments','U') IS NULL
CREATE TABLE dbo.Payments(
    PaymentId     INT IDENTITY(1,1) PRIMARY KEY,
    PaymentDate   DATETIME NOT NULL DEFAULT GETDATE(),
    Amount        DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    PaymentMethod NVARCHAR(30) NULL,
    CustomerId    INT NULL FOREIGN KEY REFERENCES dbo.Customers(CustomerId),
    SupplierId    INT NULL FOREIGN KEY REFERENCES dbo.Suppliers(SupplierId),
    SaleId        INT NULL FOREIGN KEY REFERENCES dbo.Sales(SaleId),
    PurchaseId    INT NULL FOREIGN KEY REFERENCES dbo.Purchases(PurchaseId),
    Notes         NVARCHAR(500) NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.Expenses','U') IS NULL
CREATE TABLE dbo.Expenses(
    ExpenseId     INT IDENTITY(1,1) PRIMARY KEY,
    Category      NVARCHAR(100) NOT NULL,
    Description   NVARCHAR(300) NULL,
    Amount        DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    ExpenseDate   DATETIME NOT NULL DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(30) NULL,
    Notes         NVARCHAR(500) NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.Income','U') IS NULL
CREATE TABLE dbo.Income(
    IncomeId      INT IDENTITY(1,1) PRIMARY KEY,
    Category      NVARCHAR(100) NOT NULL,
    Description   NVARCHAR(300) NULL,
    Amount        DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    IncomeDate    DATETIME NOT NULL DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(30) NULL,
    Notes         NVARCHAR(500) NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.StockMovements','U') IS NULL
CREATE TABLE dbo.StockMovements(
    StockMovementId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(ProductId),
    MovementType    NVARCHAR(20) NOT NULL,
    Quantity        DECIMAL(18,2) NOT NULL,
    ReferenceId     INT NULL,
    ReferenceType   NVARCHAR(30) NULL,
    Notes           NVARCHAR(300) NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('dbo.CompanySettings','U') IS NULL
CREATE TABLE dbo.CompanySettings(
    SettingId    INT PRIMARY KEY DEFAULT 1,
    CompanyName  NVARCHAR(200) NOT NULL DEFAULT N'شركتي',
    LogoPath     NVARCHAR(300) NULL,
    Address      NVARCHAR(300) NULL,
    Phone        NVARCHAR(50)  NULL,
    Email        NVARCHAR(150) NULL,
    Currency     NVARCHAR(20)  NOT NULL DEFAULT N'ر.ي',
    TaxRate      DECIMAL(5,2)  NOT NULL DEFAULT 0,
    InvoiceFooter NVARCHAR(300) NULL,
    UpdatedAt    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_CompanySettings_SingleRow CHECK (SettingId = 1)
);
GO

IF NOT EXISTS(SELECT 1 FROM dbo.CompanySettings)
    INSERT INTO dbo.CompanySettings(SettingId) VALUES(1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Admin')
    INSERT INTO dbo.Roles(Name, Description) VALUES (N'Admin', N'مدير النظام');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Manager')
    INSERT INTO dbo.Roles(Name, Description) VALUES (N'Manager', N'مدير');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Employee')
    INSERT INTO dbo.Roles(Name, Description) VALUES (N'Employee', N'موظف');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    DECLARE @roleId INT = (SELECT RoleId FROM dbo.Roles WHERE Name = 'Admin');
    INSERT INTO dbo.Users(Username, PasswordHash, FullName, RoleId)
    VALUES (N'admin', N'$2a$11$REPLACE_ON_FIRST_RUN', N'مدير النظام', @roleId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sales_CustomerId')
    CREATE INDEX IX_Sales_CustomerId ON dbo.Sales(CustomerId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sales_SaleDate')
    CREATE INDEX IX_Sales_SaleDate ON dbo.Sales(SaleDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Purchases_SupplierId')
    CREATE INDEX IX_Purchases_SupplierId ON dbo.Purchases(SupplierId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PurchaseItems_PurchaseId')
    CREATE INDEX IX_PurchaseItems_PurchaseId ON dbo.PurchaseItems(PurchaseId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SaleItems_SaleId')
    CREATE INDEX IX_SaleItems_SaleId ON dbo.SaleItems(SaleId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Barcode')
    CREATE INDEX IX_Products_Barcode ON dbo.Products(Barcode);
GO