
CREATE DATABASE [TemporalTables-Demo]
GO

USE [TemporalTables-Demo]
GO


--Creating table with visible 
CREATE TABLE [dbo].[Products]
(
	[Id] uniqueidentifier NOT NULL 
		CONSTRAINT [DF_Products_Id] DEFAULT newid(),
	[Name] nvarchar(200) NOT NULL,
	[Price] decimal(19,4) NOT NULL,
	[Quantity] int NOT NULL,
	--Hidden column used to indicate begining of time range when row was "current"
	[StartTime] datetime2 GENERATED ALWAYS AS ROW START --Column can be hidden with adding 'HIDDEN' here
           CONSTRAINT [DF_Products_StartTime] DEFAULT SYSUTCDATETIME(),  
	--Hidden column used to indicate end of time range when row was "current"
    [EndTime] datetime2 GENERATED ALWAYS AS ROW END --Column can be hidden with adding 'HIDDEN' here
           CONSTRAINT [DF_Products_EndTime] DEFAULT CONVERT(datetime2 (0), '9999-12-31 23:59:59'),   
    --Specifying time period from two columns that indicate time range when row was "current"
	PERIOD FOR SYSTEM_TIME ([StartTime], [EndTime]),  
	CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id])
)
WITH
(
	SYSTEM_VERSIONING = ON
	(
		HISTORY_TABLE = [dbo].[ProductsHistory]
	)
);
GO

--Populating table
INSERT INTO [dbo].[Products] ([Name], [Price], [Quantity])
VALUES 
	('Keyboard', 20.00, 100),
	('Mouse', 10.50, 250),
	('Pet rock', 1.99, 1000),
	('Trained pet rock', 2.99, 500),
	('Semicolon', 49.99, 5)
GO

--Let's sell some pet rocks
UPDATE [dbo].[Products]
SET [Quantity] -= 10
WHERE [Name] = N'Pet rock'
GO
--Whoa, peaople are actually buying this stuff, let's raise the price a bit and stock up in pet rocks
UPDATE [dbo].[Products]
SET [Price] *= 1.1
WHERE [Name] = N'Pet rock'
GO

UPDATE [dbo].[Products]
SET [Quantity] += 1000
WHERE [Name] = N'Pet rock'
GO

----
--Actually, it does look a lot better if you have data scattered through larger time range
----

--Some sample queries

--Query for certain point in time
SELECT 
	[Id]
	,[Name]
	,[Price]
	,[Quantity]
FROM [dbo].[Products] 
	FOR SYSTEM_TIME AS OF '2018-02-28 21:50:14'

--Results grouped and aggregated to show min/max values in given time range
SELECT 
	[Id]
	,[Name]
	,MIN([Price]) AS Min_Price
	,MAX([Price]) AS Max_Price
	,MIN([Quantity]) AS Min_Quantity
	,MAX([Quantity]) AS Max_Quantity
FROM [dbo].[Products] 
FOR SYSTEM_TIME    
	BETWEEN '2018-02-1 00:00:00' AND '2018-02-28 23:59:59'
GROUP BY [Id], [Name]

--Same for entire lifetime of a table
SELECT 
	[Id]
	,[Name]
	,MIN([Price]) AS Min_Price
	,MAX([Price]) AS Max_Price
	,MIN([Quantity]) AS Min_Quantity
	,MAX([Quantity]) AS Max_Quantity
FROM [dbo].[Products] 
FOR SYSTEM_TIME ALL
GROUP BY [Id], [Name]

