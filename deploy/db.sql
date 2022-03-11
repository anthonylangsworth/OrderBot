CREATE DATABASE [OrderBot]
GO

USE [OrderBot]
GO

CREATE LOGIN [OrderBot] WITH PASSWORD=N'<password>', DEFAULT_DATABASE=[OrderBot], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

CREATE USER [OrderBot] FOR LOGIN [OrderBot] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_datareader', 'OrderBot'
EXEC sp_addrolemember 'db_datawriter', 'OrderBot'
