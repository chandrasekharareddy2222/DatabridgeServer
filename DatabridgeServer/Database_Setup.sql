-- =============================================
-- DatabridgeDB Database Creation Script
-- =============================================

USE master;
GO

-- Create Database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DatabridgeDB')
BEGIN
    CREATE DATABASE [DatabridgeDB];
    PRINT 'Database DatabridgeDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database DatabridgeDB already exists.';
END
GO

-- Switch to the DatabridgeDB database
USE [DatabridgeDB];
GO

-- Enable Read Committed Snapshot Isolation
IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    ALTER DATABASE [DatabridgeDB] SET READ_COMMITTED_SNAPSHOT ON;
END
GO

-- =============================================
-- Create Products Table
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [Stock] INT NOT NULL DEFAULT(0),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table Products created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Products already exists.';
END
GO

-- =============================================
-- Create Indexes (Optional - for better performance)
-- =============================================

-- Index on Name for faster searches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Name' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_Name] 
    ON [dbo].[Products] ([Name] ASC);
    PRINT 'Index IX_Products_Name created successfully.';
END
GO

-- Index on CreatedAt for sorting
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_CreatedAt] 
    ON [dbo].[Products] ([CreatedAt] DESC);
    PRINT 'Index IX_Products_CreatedAt created successfully.';
END
GO

-- =============================================
-- Insert Sample Data (Optional)
-- =============================================

IF NOT EXISTS (SELECT * FROM [dbo].[Products])
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [Stock], [CreatedAt])
    VALUES 
        ('Laptop', 'High-performance laptop for professionals', 999.99, 50, GETUTCDATE()),
        ('Mouse', 'Wireless optical mouse', 29.99, 150, GETUTCDATE()),
        ('Keyboard', 'Mechanical gaming keyboard', 79.99, 100, GETUTCDATE()),
        ('Monitor', '27-inch 4K display', 399.99, 75, GETUTCDATE()),
        ('Headphones', 'Noise-cancelling headphones', 149.99, 120, GETUTCDATE());
    
    PRINT 'Sample data inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Products table already contains data.';
END
GO

-- =============================================
-- Verify Table Creation
-- =============================================

SELECT 
    'Database: ' + DB_NAME() AS [Information],
    COUNT(*) AS [Total Products]
FROM [dbo].[Products];
GO

PRINT 'Database setup completed successfully!';
GO
