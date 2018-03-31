CREATE DATABASE [CollectionsProcessingDemo]
GO

USE [CollectionsProcessingDemo]
GO

CREATE TABLE [dbo].[RawSales]
(
	[Id] uniqueidentifier NOT NULL 
	[Amount] int NOT NULL,
	[DateTime] datetime NOT NULL,
	[EmployeeId] uniqueidentifier NOT NULL 
)
GO

--Populate table with 10 million random rows
BEGIN	
	DECLARE @n int = 0;
	DECLARE @limit int = 10000000;
	DECLARE @employeeId1 uniqueidentifier = NEWID();
	DECLARE @employeeId2 uniqueidentifier = NEWID();
	DECLARE @employeeId3 uniqueidentifier = NEWID();
	DECLARE @employeeId4 uniqueidentifier = NEWID();
	DECLARE @employeeId5 uniqueidentifier = NEWID();
	DECLARE @employeeId6 uniqueidentifier = NEWID();
	DECLARE @employeeId7 uniqueidentifier = NEWID();
	DECLARE @employeeId8 uniqueidentifier = NEWID();
	DECLARE @employeeId9 uniqueidentifier = NEWID();
	DECLARE @employeeId10 uniqueidentifier = NEWID();
	WHILE (@n < @limit)
	BEGIN
		--random 1-10 NUMBER
		DECLARE @x int = ABS(CHECKSUM(NEWID()) % 10) + 1;
		DECLARE @currentEmployeeId uniqueidentifier;
		SET @currentEmployeeId = 
			CASE @x
				WHEN 1 THEN @employeeId1
				WHEN 2 THEN @employeeId2
				WHEN 3 THEN @employeeId3
				WHEN 4 THEN @employeeId4
				WHEN 5 THEN @employeeId5
				WHEN 6 THEN @employeeId6
				WHEN 7 THEN @employeeId7
				WHEN 8 THEN @employeeId8
				WHEN 9 THEN @employeeId9
				WHEN 10 THEN @employeeId10
			END  

		INSERT INTO [dbo].[RawSales] ([Id], [Amount], [DateTime], [EmployeeId])
		VALUES 
		(
			DEFAULT,
			ABS(CHECKSUM(NEWID()) % 90) + 10,
			DATETIMEFROMPARTS ( 2018, DATEPART(month, GETUTCDATE()), @n % 28 + 1, @n % 24, @n % 60, 0, 0 ),
			@currentEmployeeId
		);
		SET @n += 1;
	END;
END

CREATE TABLE [dbo].[IndexedSales]
(
	[Id] uniqueidentifier NOT NULL 
		CONSTRAINT [DF_[IndexedSales_Id] DEFAULT newid(),
	[Amount] int NOT NULL,
	[DateTime] datetime NOT NULL,
	[EmployeeId] uniqueidentifier NOT NULL 
		CONSTRAINT [DF_[IndexedSales_EmployeeId] DEFAULT newid(),
)
GO

INSERT INTO [dbo].[IndexedSales] 
	([Id], [Amount], [DateTime], [EmployeeId])
SELECT 
	[Id], 
	[Amount], 
	[DateTime], 
	[EmployeeId] 
FROM [dbo].[RawSales]
GO

CREATE NONCLUSTERED INDEX [Ix_IndexedSales_Report_Covering_Index]
ON [dbo].[IndexedSales] ([EmployeeId])
	INCLUDE ([Amount],[DateTime])

CREATE TABLE [dbo].[IndexedSalesWithComputedColumn]
(
	[Id] uniqueidentifier NOT NULL 
		CONSTRAINT [DF_IndexedSalesWithComputedColumn_Id] DEFAULT newid(),
	[Amount] int NOT NULL,
	[DateTime] datetime NOT NULL,
	[Date] AS CONVERT(date, [DateTime]) PERSISTED NOT NULL,
	[EmployeeId] uniqueidentifier NOT NULL 
		CONSTRAINT [DF_IndexedSalesWithComputedColumn_EmployeeId] DEFAULT newid(),
)
GO

INSERT INTO [dbo].[IndexedSalesWithComputedColumn] 
	([Id], [Amount], [DateTime], [EmployeeId])
SELECT 
	[Id], 
	[Amount], 
	[DateTime], 
	[EmployeeId] 
FROM [dbo].[RawSales]
GO

CREATE NONCLUSTERED INDEX [Ix_IndexedSalesWithComputedColumn_Report_Covering_Index]
ON [dbo].[IndexedSalesWithComputedColumn] ([EmployeeId])
	INCLUDE ([Amount],[Date])
GO

CREATE VIEW [dbo].[Vw_SalesReport]
WITH SCHEMABINDING
AS
(
	SELECT 
		[Date] AS SalesDate, 
		SUM([Amount]) AS AmountSum,
		[EmployeeId],
		COUNT_BIG(*) AS RequiredCount
	FROM [dbo].[IndexedSalesWithComputedColumn]
	GROUP BY [Date], [EmployeeId]
)
GO

CREATE UNIQUE CLUSTERED INDEX [Ix_SalesReportView] 
	ON [dbo].[Vw_SalesReport] 
	( [EmployeeId], [SalesDate] DESC )
GO

--Querying against indexed view
SELECT [SalesDate]
      ,[AmountSum]
  FROM [dbo].[Vw_SalesReport] 
	WITH (NOEXPAND) -- This line is required to query against index if you're working with SQL Server Standard or lower
  WHERE [EmployeeId] = 'someid'