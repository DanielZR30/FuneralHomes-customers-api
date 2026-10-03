CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE TABLE customers (
        id uuid NOT NULL,
        customer_type character varying(20) NOT NULL,
        name character varying(255) NOT NULL,
        identification_type character varying(20) NOT NULL,
        identification_number character varying(50) NOT NULL,
        email character varying(100),
        phone character varying(30),
        address character varying(255),
        status character varying(20) NOT NULL DEFAULT 'ACTIVE',
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_customers" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE TABLE members (
        id uuid NOT NULL,
        subject_type character varying(20) NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100),
        identification_type character varying(20),
        identification_number character varying(50),
        birth_date date NOT NULL,
        derived_age integer NOT NULL,
        email character varying(100),
        phone character varying(30),
        CONSTRAINT "PK_members" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE TABLE customer_subscriptions (
        id uuid NOT NULL,
        customer_id uuid NOT NULL,
        external_plan_id uuid NOT NULL,
        max_beneficiaries integer NOT NULL DEFAULT 1,
        start_date date NOT NULL,
        end_date date,
        status character varying(20) NOT NULL DEFAULT 'ACTIVE',
        CONSTRAINT "PK_customer_subscriptions" PRIMARY KEY (id),
        CONSTRAINT "FK_customer_subscriptions_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES customers (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE TABLE beneficiaries (
        id uuid NOT NULL,
        subscription_id uuid NOT NULL,
        member_id uuid NOT NULL,
        beneficiary_type character varying(20) NOT NULL,
        relationship_type character varying(30) NOT NULL,
        status character varying(20) NOT NULL DEFAULT 'ACTIVE',
        joined_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        removed_at timestamp with time zone,
        CONSTRAINT "PK_beneficiaries" PRIMARY KEY (id),
        CONSTRAINT "FK_beneficiaries_customer_subscriptions_subscription_id" FOREIGN KEY (subscription_id) REFERENCES customer_subscriptions (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_beneficiaries_members_member_id" FOREIGN KEY (member_id) REFERENCES members (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE TABLE beneficiary_audit_log (
        id uuid NOT NULL,
        subscription_id uuid NOT NULL,
        member_id uuid NOT NULL,
        action character varying(30) NOT NULL,
        event_published boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_beneficiary_audit_log" PRIMARY KEY (id),
        CONSTRAINT "FK_beneficiary_audit_log_customer_subscriptions_subscription_id" FOREIGN KEY (subscription_id) REFERENCES customer_subscriptions (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_beneficiary_audit_log_members_member_id" FOREIGN KEY (member_id) REFERENCES members (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_customers_identification_number" ON customers (identification_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE INDEX "IX_customer_subscriptions_customer_id" ON customer_subscriptions (customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE INDEX "IX_beneficiaries_member_id" ON beneficiaries (member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE INDEX "IX_beneficiaries_subscription_id" ON beneficiaries (subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE INDEX "IX_beneficiary_audit_log_member_id" ON beneficiary_audit_log (member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    CREATE INDEX "IX_beneficiary_audit_log_subscription_id" ON beneficiary_audit_log (subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261003181200_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261003181200_InitialCreate', '10.0.4');
    END IF;
END $EF$;
COMMIT;

