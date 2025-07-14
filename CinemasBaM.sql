Create database BaMCinemas
go

Use BaMCinemas
go




create table Customers
(
	CustomerID int primary key,
	FirstName nvarchar (30),
	LastName nvarchar (50),
	Gender nvarchar (10),
	PhoneNumber nvarchar(20),
	DOB Date,
	Email nvarchar (255),
	Image nvarchar (455),
	Address nvarchar (555)
);
go

Create table AccountCustomers 
(
	AccountID int Primary key,
	PasswordHash Nvarchar (455), --SHA256 Hash
	CustomerID int,
	Foreign key (CustomerID) References Customers (CustomerID),
	RegistrationDate Datetime Default Getdate()

);
go

Create table Employees
(
	EmployeeID int Primary key,
	FirstName nvarchar (30),
	LastName nvarchar (50),
	Gender nvarchar (10),
	DOB date,
	Email nvarchar (255),
	PhoneNumber int,
	Roll int, -- 1 Admin (Office, Supervisor, ...), 2 Team Lead, 3 Team Member
	Detail nvarchar (50),
	Address nvarchar (255),
	JobAcceptanceDate Datetime Default GetDate()
);
go

Create table AccountEmployees
(
	AccountID Int Primary key,
	Username nvarchar(20),
	PasswordHash nvarchar (455), --SHA256 Hash
	RegistrationDate Datetime Default Getdate(),
	EmployeeID int,
	Foreign Key (EmployeeID) References Employees (EmployeeID),
);
go




Create Table LoginIdentifiers
(
	LoginID int Primary Key,
	AccountID int,
	Foreign Key (AccountID) References AccountCustomers (AccountID),
	Identifier Nvarchar (155),
	Type Nvarchar(55),
	IsPrimary BIT
);
go

Create Table EmployeeRoles 
(
	RoleID int Primary Key,
	RoleName nvarchar (50),
	SalaryType int, -- 1. TeamLead, 2. Fulltime, 3. Parttime
	BaseSalary Decimal (18,2)
);
go

Create table Cinemas
(
	CinemaID int Primary key,
	CinemaName nvarchar (255),
	CinemaCode nvarchar (50),
	Address nvarchar (255),
	PhoneNumber int
);
go

Create table Cinemas_Employees
(
	EmployeeID int,
	RoleID int,
	CinemaID int,
	Position nvarchar (50),
	Primary key (CinemaID, EmployeeID),
	Foreign Key (RoleID) References EmployeeRoles (RoleID),
	Foreign key (EmployeeID) References Employees (EmployeeID),
	Foreign key (CinemaID) References Cinemas (CinemaID)
);
go

Create Table SeatLayoutConfigs
(
	LayoutID int Primary key,
	StartRow Char(1),
	EndRow Char(1),
	ColumnsPerRow int
);
go

Create table Rooms
(
	RoomID Int Primary Key,
	RoomName Nvarchar(50),
	RoomType Nvarchar(50),
	CinemaID int,
	LayoutID int,
	Foreign Key (LayoutID) References SeatLayoutConfigs (LayoutID),
	Foreign Key (CinemaID) References Cinemas (CinemaID)
);
go

Create Table SeatTypes
(
	TypeID int Primary Key,
	TypeName int, --1. Standard, 2.VIP
	Price Decimal (18,2)
);
go

Create table Seats
(
	SeatID nvarchar (10) Primary key,
	SeatName Varchar(10),
	RowChar Char(1),
	ColumNum int,
	RoomID int,
	TypeID int,
	Foreign Key (RoomID) References Rooms (RoomID),
	Foreign Key (TypeID) References SeatTypes (TypeID)
);
go

Create Table Movies
(
	MovieID int Primary Key,
	Title Nvarchar (255),
	Duration int,
	Genre Nvarchar (100)
);
go

Create Table Showtimes
(
	ShowtimeID int Primary Key,
	MovieID int,
	RoomID int,
	StartTime Datetime,
	EndTime Datetime,
	Foreign Key (MovieID) References Movies (MovieID),
	Foreign Key (RoomID) References Rooms (RoomID)
);
go

Create Table Tickets
(
	TicketID Int Primary key,
	CustomerID int,
	ShowtimeID int,
	SeatID Nvarchar(10),
	ReceiptID int,
	Price decimal (18,2),
	Foreign Key (CustomerID) References Customers (CustomerID),
	Foreign Key (ShowtimeID) References Showtimes (ShowtimeID),
	Foreign Key (SeatID) References Seats (SeatID),
	FOREIGN KEY (ReceiptID) REFERENCES Receipts(ReceiptID)
);
go


Create table Coupons
(
	CouponID int Primary key,
	CouponName nvarchar (255),
	DiscountAmount Decimal (18,2),
	Description nvarchar (255),
	IsActive bit
);
go

Create Table Receipts
(
	ReceiptID int Primary Key,
	CustomerID int,
	EmployeeID int,
	PaymentID int,
	CinemaID int,
	Discount decimal (18,2),
	PurchaseDate datetime default Getdate(),
	TotalPrice decimal (18,2),
	FinalAmount decimal (18,2),
	Note nvarchar (max)
	Foreign key (CustomerID) References Customers (CustomerID),
	Foreign key (EmployeeID) References Employees (EmployeeID),
	Foreign key (PaymentID) References Payments (PaymentID),
	Foreign key (ReceiptID) References Receipts (ReceiptID)
);
go

Create Table Payments
(
	PaymentID int Primary key,
	Amount Decimal (18,2),
	PaymentMethod nvarchar (55),
	PaymentDate Datetime Default Getdate(),
	PaymentStatus nvarchar(55),
	Note nvarchar (255),
);
go



Create table Foods
(
	FoodID int Primary key,
	Name Nvarchar (255),
	Price Decimal (18,2),
	Image varchar(1555),
	Decription nvarchar(755)
);
go


Create table Foods_Cinemas
(
	FoodID int,
	CinemaID int,
	Status int,
	Primary key (CinemaID, FoodID),
	Foreign Key (FoodID) References Foods (FoodID),
	Foreign Key (CinemaID) References Cinemas (CinemaID)
);
go

CREATE TABLE Receipts_Coupons
(
    ReceiptID INT,
    CouponID INT,
    DiscountAmount DECIMAL(10,2),
    PRIMARY KEY (ReceiptID, CouponID),
    FOREIGN KEY (ReceiptID) REFERENCES Receipts(ReceiptID),
    FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID)
);
go



Create Table Combos
(
	ComboID int Primary Key,
	CouponID int,
	ComboName nvarchar (125),
	Description nvarchar (355),
	Price decimal (18,2),
	Image nvarchar (355)
	FOREIGN KEY (CouponID) REFERENCES Coupons (CouponID)
);
go



Create table ComboDetails
(
	ComDetailID int primary key,
	ComboID int,
	FoodID int,
	Quantity int,
	Note nvarchar (1024)
	FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
    FOREIGN KEY (FoodID) REFERENCES Foods(FoodID)
);
go

CREATE TABLE Receipt_Combos 
(
    ReceiptID INT,
    ComboID INT,
	CouponID int,
    Quantity INT,
    DiscountApplied DECIMAL(10,2),
    FinalAmount DECIMAL(10,2),
    PRIMARY KEY (ReceiptID, ComboID),
    FOREIGN KEY (ReceiptID) REFERENCES Receipts(ReceiptID),
    FOREIGN KEY (ComboID) REFERENCES Combos(ComboID),
	FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID)
);
go

CREATE TABLE Receipt_Foods 
(
    ReceiptID INT,
    FoodID INT,
    Quantity INT,
    UnitPrice DECIMAL(10,2),
    FinalAmount DECIMAL(10,2),
    PRIMARY KEY (ReceiptID, FoodID),
    FOREIGN KEY (ReceiptID) REFERENCES Receipts(ReceiptID),
    FOREIGN KEY (FoodID) REFERENCES Foods(FoodID)
);
go

CREATE TABLE Shifts 
(
    ShiftID INT PRIMARY KEY,
	CinemaID int,
    StartTime TIME,
    EndTime TIME,
    ShiftDate DATE,
	FOREIGN KEY (CinemaID) REFERENCES Cinemas (CinemaID)
);
go

CREATE TABLE ShiftSwaps (
    SwapID INT PRIMARY KEY,
	CinemaID int,
    FromEmployeeID INT,
    ToEmployeeID INT,
    ShiftID INT,
    Reason NVARCHAR(255),
    Confirmed BIT DEFAULT 0,
    FOREIGN KEY (FromEmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (ToEmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (ShiftID) REFERENCES Shifts(ShiftID),
	FOREIGN KEY (CinemaID) REFERENCES Cinemas (CinemaID)
);
go

CREATE TABLE PassRequest 
(
    RequestID INT PRIMARY KEY,
    EmployeeID INT,
    ShiftID INT,
    Reason NVARCHAR(255),
    Confirmed BIT DEFAULT 0,
	ConfirmBy nvarchar (155),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (ShiftID) REFERENCES Shifts(ShiftID)
);
go

CREATE TABLE SalarySummary (
    SummaryID INT PRIMARY KEY,
    EmployeeID INT,
    Month INT,
    Year INT,
    TotalHours INT,
    TotalEarnings DECIMAL(10,2),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
go

CREATE TABLE SystemLogs (
    LogID INT PRIMARY KEY,
    UserID INT,
    Action NVARCHAR(255),
    LogTime DATETIME DEFAULT GETDATE()
);
go

CREATE TABLE History (
    HistoryID INT PRIMARY KEY,
    EntityName NVARCHAR(255),
    EntityID INT,
    ChangeTime DATETIME DEFAULT GETDATE(),
    Action NVARCHAR(100),
    ChangedBy INT,
    CinemaID INT NULL,
    Note NVARCHAR(1000),
    FOREIGN KEY (ChangedBy) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (CinemaID) REFERENCES Cinemas(CinemaID)
);
go

CREATE TABLE DeletedReceipts (
    ReceiptID INT PRIMARY KEY,
    DeletedAt DATETIME DEFAULT GETDATE(),
    DeletedBy INT,
    Reason NVARCHAR(1000),
    FinalAmount DECIMAL(18,2),
    Note NVARCHAR(MAX),
    FOREIGN KEY (DeletedBy) REFERENCES Employees(EmployeeID)
);
go

CREATE TABLE Items (
    ItemID INT PRIMARY KEY,
    ItemName NVARCHAR(255),
    Unit NVARCHAR(50),
	QuanlityPerUnit int,
    Category NVARCHAR(100),
    Description NVARCHAR(500)
);
go

CREATE TABLE Cinemas_ItemsStock (
    StockID INT PRIMARY KEY IDENTITY(1,1),
    CinemaID INT,
    ItemID INT,
    Quantity INT DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE(),
	IsActive int,
    Note NVARCHAR(255),
    FOREIGN KEY (CinemaID) REFERENCES Cinemas(CinemaID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);
go


CREATE TABLE Requests (
    RequestID INT PRIMARY KEY,
    RequestDate DATE Default Getdate(),
    ApprovedDate DATE,
    ConfirmDate DATE,
    Status INT,
    RequestType INT, -- 1 = Nhập hàng, 2 = Xuất, 3 = Hủy, ...
    EmployeeID INT,
    CinemaID INT,
    Note NVARCHAR(2048),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (CinemaID) REFERENCES Cinemas(CinemaID)
);
go

CREATE TABLE RequestDetails (
    DetailID INT PRIMARY KEY,
    RequestID INT,
    ItemID INT,
    BaseUnit INT,
    QuantityPerUnit INT,
    Note NVARCHAR(1024),
    FOREIGN KEY (RequestID) REFERENCES Requests(RequestID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);
go

CREATE TABLE ItemDisposals (
    DisposalID INT PRIMARY KEY IDENTITY(1,1),
    CinemaID INT,
    ItemID INT,
    Quantity INT,
    DisposalDate DATETIME DEFAULT GETDATE(),
    Reason NVARCHAR(500),
    ApprovedBy INT, -- EmployeeID người duyệt
    RequestedBy INT, -- EmployeeID người đề xuất
    Note NVARCHAR(1000),
	image nvarchar (2000),
    FOREIGN KEY (CinemaID) REFERENCES Cinemas(CinemaID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID),
    FOREIGN KEY (ApprovedBy) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (RequestedBy) REFERENCES Employees(EmployeeID)
);
go

CREATE TABLE Foods_CinemasStock (
    StockID INT PRIMARY KEY IDENTITY(1,1),
    CinemaID INT,
    FoodID INT,
    Quantity INT DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE(),
    Note NVARCHAR(255),
    FOREIGN KEY (CinemaID) REFERENCES Cinemas(CinemaID),
    FOREIGN KEY (FoodID) REFERENCES Foods(FoodID)
);
go



CREATE TRIGGER trg_DecreaseFoodStock
ON Receipt_Foods
AFTER INSERT
AS
BEGIN
    UPDATE FCS
    SET FCS.Quantity = FCS.Quantity - i.Quantity,
        FCS.LastUpdated = GETDATE()
    FROM Foods_CinemasStock FCS
    JOIN inserted i ON i.FoodID = FCS.FoodID
    JOIN Receipts r ON r.ReceiptID = i.ReceiptID
    WHERE r.CinemaID = FCS.CinemaID;
END;
GO




CREATE TRIGGER trg_LogDeletedReceipt
ON Receipts
INSTEAD OF DELETE
AS
BEGIN
    -- Ghi log trước khi xóa
    INSERT INTO DeletedReceipts (ReceiptID, DeletedAt, DeletedBy, Reason, FinalAmount, Note)
    SELECT r.ReceiptID, GETDATE(), NULL, 'Lý do chưa cung cấp (bắt buộc nhập ở frontend)', r.FinalAmount, r.Note
    FROM deleted r;

    -- Xóa dữ liệu liên quan
    DELETE FROM Tickets WHERE ReceiptID IN (SELECT ReceiptID FROM deleted);
    DELETE FROM Receipt_Foods WHERE ReceiptID IN (SELECT ReceiptID FROM deleted);
    DELETE FROM Receipt_Combos WHERE ReceiptID IN (SELECT ReceiptID FROM deleted);
    DELETE FROM Receipts_Coupons WHERE ReceiptID IN (SELECT ReceiptID FROM deleted);
    DELETE FROM Receipts WHERE PaymentID IN (SELECT PaymentID FROM deleted);
    DELETE FROM Receipts WHERE ReceiptID IN (SELECT ReceiptID FROM deleted);
END;
GO

CREATE PROCEDURE sp_FinalizeReceipt
    @ReceiptID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalTickets DECIMAL(10,2) = 0;
    DECLARE @TotalFoods DECIMAL(10,2) = 0;
    DECLARE @TotalCombos DECIMAL(10,2) = 0;
    DECLARE @TotalDiscount DECIMAL(10,2) = 0;

    -- Tổng giá vé từ bảng Tickets
    SELECT @TotalTickets = ISNULL(SUM(Price), 0)
    FROM Tickets
    WHERE ReceiptID = @ReceiptID;

    -- Tổng món ăn từ Receipt_Foods
    SELECT @TotalFoods = ISNULL(SUM(FinalAmount), 0)
    FROM Receipt_Foods
    WHERE ReceiptID = @ReceiptID;

    -- Tổng combo từ Receipt_Combos
    SELECT @TotalCombos = ISNULL(SUM(FinalAmount), 0)
    FROM Receipt_Combos
    WHERE ReceiptID = @ReceiptID;

    -- Tổng giảm giá từ Receipt_CouponApplications
    SELECT @TotalDiscount = ISNULL(SUM(DiscountAmount), 0)
    FROM Receipts_Coupons
    WHERE ReceiptID = @ReceiptID;

    -- Cập nhật lại hoá đơn chính
    UPDATE Receipts
    SET TotalPrice = @TotalTickets + @TotalFoods + @TotalCombos,
        Discount = @TotalDiscount,
        FinalAmount = (@TotalTickets + @TotalFoods + @TotalCombos) - @TotalDiscount
    WHERE ReceiptID = @ReceiptID;
END;
GO

USE master;
GO

-- Đặt database về chế độ single user để kill kết nối
ALTER DATABASE BaMCinemas SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Xóa database
DROP DATABASE BaMCinemas;
GO