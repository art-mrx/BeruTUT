-- Создаёт роль и БД для локальной разработки Store.
-- Запуск: "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -f scripts/setup-local-db.sql

DO
$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'store') THEN
      CREATE ROLE store LOGIN PASSWORD 'store';
   END IF;
END
$$;

SELECT 'CREATE DATABASE store OWNER store'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'store')\gexec
