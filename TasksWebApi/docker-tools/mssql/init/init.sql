USE [master]
GO

IF DB_ID('TasksDb') IS NOT NULL
  set noexec on 

CREATE DATABASE [TasksDb];
GO