-- PostgreSQL init script: runs on first container start
-- Creates the StockDb database if it doesn't already exist
SELECT 'CREATE DATABASE "StockDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'StockDb')\gexec
