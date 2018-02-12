
CREATE DATABASE [RowLevelSecurityPolicy-Demo]
GO

USE [RowLevelSecurityPolicy-Demo]
GO

--Create and populate table
CREATE TABLE [dbo].[Employees]
(
	[Id] bigint IDENTITY(1,1) NOT NULL,
	[CompanyId] bigint NOT NULL,
	[FirstName] nvarchar(200) NOT NULL,
	[LastName] nvarchar(200) NOT NULL,
	[Email] nvarchar(260),
	CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

INSERT INTO [dbo].[Employees] ([CompanyId], [FirstName], [LastName], [Email])
VALUES 
	(1 , 'Rafal', 'Hryniewski' , 'my@email.com'),
	(1, 'John' , 'Smith', 'notmy@email.com'),
	(1, 'Some', 'Guy', 'someguy@email.com'),
	(2, 'Other', 'Guy', 'otherguy@mail.net'),
	(2, 'That', 'Guy', 'thatguy@mail.net')
GO


--Schema and security predicate along with attaching filter
CREATE SCHEMA [Security]  
GO 

CREATE FUNCTION [Security].[FN_SecurityPredicate](@CompanyId bigint)  
    RETURNS TABLE  
    WITH SCHEMABINDING  
AS
	RETURN 
		--Access is granted for row only when this predicate returns 1
		SELECT 1 AS Result
		WHERE 
			CAST(SESSION_CONTEXT(N'CompanyId') AS int) = @CompanyId;   
GO  

CREATE SECURITY POLICY [Security].[SalesFilter]
    ADD FILTER PREDICATE [Security].[FN_SecurityPredicate](CompanyId)   
        ON [dbo].[Employees]
    WITH (STATE = ON);  
GO

-- Setting CompanyId in SQL Session Example
EXEC SP_SET_SESSION_CONTEXT 
	@key=N'CompanyId' 
	,@value=2
