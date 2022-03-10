CREATE DATABASE [OrderBot]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'OrderBot', FILENAME = N'/var/opt/mssql/data/OrderBot.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'OrderBot_log', FILENAME = N'/var/opt/mssql/data/OrderBot_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [OrderBot].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

USE [master]
GO

CREATE LOGIN [OrderBot] WITH PASSWORD=N'<password>', DEFAULT_DATABASE=[OrderBot], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE [OrderBot]
GO

CREATE USER [OrderBot] FOR LOGIN [OrderBot] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_datareader', 'OrderBot'
EXEC sp_addrolemember 'db_datawriter', 'OrderBot'
