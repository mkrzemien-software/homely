-- Init script for E2E Testing Environment
-- Simplified setup: Creates auth schema with minimal auth.users table
-- Test users will be created via SQL in global-setup.ts

-- Create auth schema
CREATE SCHEMA IF NOT EXISTS auth;

-- Create Supabase roles (needed for RLS policies in migrations)
DO
$$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'anon') THEN
    CREATE ROLE anon NOLOGIN NOINHERIT;
  END IF;

  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'authenticated') THEN
    CREATE ROLE authenticated NOLOGIN NOINHERIT;
  END IF;

  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'service_role') THEN
    CREATE ROLE service_role NOLOGIN NOINHERIT BYPASSRLS;
  END IF;

  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'supabase_admin') THEN
    CREATE ROLE supabase_admin NOLOGIN NOINHERIT BYPASSRLS;
  END IF;
END
$$;

-- Grant permissions
GRANT USAGE ON SCHEMA public TO anon, authenticated, service_role, supabase_admin;
GRANT USAGE ON SCHEMA auth TO anon, authenticated, service_role, supabase_admin;
GRANT ALL ON SCHEMA auth TO postgres, supabase_admin;
GRANT ALL ON SCHEMA public TO postgres, supabase_admin;

-- Create Supabase helper functions for RLS policies
-- These functions are used in migrations for Row Level Security

-- auth.uid() - Returns the user ID from JWT claims (simplified for E2E)
CREATE OR REPLACE FUNCTION auth.uid()
RETURNS uuid
LANGUAGE sql STABLE
AS $$
  SELECT COALESCE(
    NULLIF(current_setting('request.jwt.claim.sub', true), '')::uuid,
    NULLIF(current_setting('app.current_user_id', true), '')::uuid
  );
$$;

-- auth.role() - Returns the user role from JWT claims (simplified for E2E)
CREATE OR REPLACE FUNCTION auth.role()
RETURNS text
LANGUAGE sql STABLE
AS $$
  SELECT COALESCE(
    NULLIF(current_setting('request.jwt.claim.role', true), ''),
    NULLIF(current_setting('app.current_user_role', true), ''),
    'anon'
  )::text;
$$;

-- auth.email() - Returns the user email from JWT claims (simplified for E2E)
CREATE OR REPLACE FUNCTION auth.email()
RETURNS text
LANGUAGE sql STABLE
AS $$
  SELECT COALESCE(
    NULLIF(current_setting('request.jwt.claim.email', true), ''),
    NULLIF(current_setting('app.current_user_email', true), '')
  )::text;
$$;

COMMENT ON FUNCTION auth.uid() IS 'Returns the user ID from JWT or session (E2E simplified version)';
COMMENT ON FUNCTION auth.role() IS 'Returns the user role from JWT or session (E2E simplified version)';
COMMENT ON FUNCTION auth.email() IS 'Returns the user email from JWT or session (E2E simplified version)';

-- Create minimal auth.users table for E2E testing
CREATE TABLE IF NOT EXISTS auth.users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(255) UNIQUE NOT NULL,
  encrypted_password VARCHAR(255),
  email_confirmed_at TIMESTAMPTZ DEFAULT NOW(),
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  raw_user_meta_data JSONB DEFAULT '{}'::jsonb,
  raw_app_meta_data JSONB DEFAULT '{}'::jsonb
);

-- Grant permissions to postgres superuser
GRANT ALL ON SCHEMA auth TO postgres;
GRANT ALL ON auth.users TO postgres;

-- Log completion
DO $$
BEGIN
  RAISE NOTICE 'E2E database initialization completed successfully';
  RAISE NOTICE 'Schema "auth" and table "auth.users" are ready';
END
$$;
