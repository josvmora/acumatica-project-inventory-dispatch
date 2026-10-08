-- ============================================================
-- Create custom tables: JEDispacht (header) / JEDispachtLine (details)
-- "Dispacht" collapses existing SO/PO documents into a single
-- document, grouped by the user's chosen date. It only reads
-- SOOrder/POOrder to reference them; it never modifies them.
-- Safe to re-run (idempotent via IF NOT EXISTS)
-- ============================================================

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME   = 'JEDispacht'
)
BEGIN
    CREATE TABLE [dbo].[JEDispacht] (

        -- Primary Key / Identity
        [CompanyID]                 INT             NOT NULL DEFAULT(0),
        [DispachtNbr]                NVARCHAR(15)    NOT NULL,

        -- Business Fields
        [BranchID]                  INT             NULL,
        [Descr]                     NVARCHAR(256)   NULL,
        [Status]                    VARCHAR(1)      NOT NULL DEFAULT('H'),
        [DispachtDate]               DATETIME        NOT NULL,
        [ShipDate]                   DATETIME        NULL,
        [CarrierID]                  INT             NULL,
        [PickupDate]                 DATETIME        NULL,
        [DropoffDate]                DATETIME        NULL,
        [ShippingTo]                 NVARCHAR(2000)  NULL,

        -- Acumatica Standard Audit Fields
        [CreatedByID]               UNIQUEIDENTIFIER NOT NULL,
        [CreatedByScreenID]         CHAR(8)          NOT NULL,
        [CreatedDateTime]           DATETIME         NOT NULL,
        [LastModifiedByID]          UNIQUEIDENTIFIER NOT NULL,
        [LastModifiedByScreenID]    CHAR(8)          NOT NULL,
        [LastModifiedDateTime]      DATETIME         NOT NULL,
        [tstamp]                    TIMESTAMP        NULL,
        [NoteID]                    UNIQUEIDENTIFIER NULL,

        CONSTRAINT [JEDispacht_PK] PRIMARY KEY CLUSTERED (
            [CompanyID]    ASC,
            [DispachtNbr]  ASC
        )
    );

    PRINT 'Table [dbo].[JEDispacht] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[JEDispacht] already exists. No action taken.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME   = 'JEDispachtAddRow'
)
BEGIN
    CREATE TABLE [dbo].[JEDispachtAddRow] (
        [CompanyID]  INT            NOT NULL DEFAULT(0),
        [UserID]     UNIQUEIDENTIFIER NOT NULL,
        [Selected]   BIT            NULL,
        [DocType]    VARCHAR(2)     NOT NULL,
        [OrderType]  NVARCHAR(2)    NOT NULL,
        [OrderNbr]   NVARCHAR(15)   NOT NULL,
        [ProjectID]  INT            NULL,
        [Descr]      NVARCHAR(256)  NULL,
        CONSTRAINT [JEDispachtAddRow_PK] PRIMARY KEY CLUSTERED (
            [CompanyID], [UserID], [DocType], [OrderType], [OrderNbr]
        )
    );

    PRINT 'Table [dbo].[JEDispachtAddRow] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[JEDispachtAddRow] already exists. No action taken.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME   = 'JEDispachtAddRow'
      AND COLUMN_NAME  = 'ProjectID'
)
BEGIN
    ALTER TABLE [dbo].[JEDispachtAddRow] ADD [ProjectID] INT NULL;
    PRINT 'Column [ProjectID] added to [dbo].[JEDispachtAddRow].';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME   = 'JEDispachtLine'
)
BEGIN
    CREATE TABLE [dbo].[JEDispachtLine] (

        -- Primary Key / Identity
        [CompanyID]                 INT             NOT NULL DEFAULT(0),
        [DispachtNbr]                NVARCHAR(15)    NOT NULL,
        [LineNbr]                    INT             NOT NULL,

        -- Reference to the source document (SO or PO), read-only link
        [DocType]                    VARCHAR(2)      NOT NULL,
        [OrderType]                  NVARCHAR(2)     NOT NULL,
        [OrderNbr]                   NVARCHAR(15)    NOT NULL,
        [Descr]                      NVARCHAR(256)   NULL,

        -- Acumatica Standard Audit Fields
        [CreatedByID]               UNIQUEIDENTIFIER NOT NULL,
        [CreatedByScreenID]         CHAR(8)          NOT NULL,
        [CreatedDateTime]           DATETIME         NOT NULL,
        [LastModifiedByID]          UNIQUEIDENTIFIER NOT NULL,
        [LastModifiedByScreenID]    CHAR(8)          NOT NULL,
        [LastModifiedDateTime]      DATETIME         NOT NULL,
        [tstamp]                    TIMESTAMP        NULL,
        [NoteID]                    UNIQUEIDENTIFIER NULL,

        CONSTRAINT [JEDispachtLine_PK] PRIMARY KEY CLUSTERED (
            [CompanyID]    ASC,
            [DispachtNbr]  ASC,
            [LineNbr]      ASC
        ),
        CONSTRAINT [JEDispachtLine_JEDispacht_FK] FOREIGN KEY (
            [CompanyID], [DispachtNbr]
        ) REFERENCES [dbo].[JEDispacht] (
            [CompanyID], [DispachtNbr]
        )
    );

    PRINT 'Table [dbo].[JEDispachtLine] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[JEDispachtLine] already exists. No action taken.';
END
GO

-- ============================================================
-- Add ProjectID (PM Project) to JEDispacht header
-- ============================================================
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME   = 'JEDispacht'
      AND COLUMN_NAME  = 'ProjectID'
)
BEGIN
    ALTER TABLE [dbo].[JEDispacht] ADD [ProjectID] INT NULL;
    PRINT 'Column [ProjectID] added to [dbo].[JEDispacht].';
END
ELSE
BEGIN
    PRINT 'Column [ProjectID] already exists on [dbo].[JEDispacht]. No action taken.';
END

GO

-- Add missing shipping columns when the header table already exists.
-- Existing documents remain unassigned; the DAC defaults new records only.
IF COL_LENGTH('dbo.JEDispacht', 'BranchID') IS NULL
    ALTER TABLE dbo.JEDispacht ADD BranchID INT NULL;
GO
IF COL_LENGTH('dbo.JEDispacht', 'ShipDate') IS NULL
    ALTER TABLE dbo.JEDispacht ADD ShipDate DATETIME NULL;
GO
IF COL_LENGTH('dbo.JEDispacht', 'CarrierID') IS NULL
    ALTER TABLE dbo.JEDispacht ADD CarrierID INT NULL;
GO
IF COL_LENGTH('dbo.JEDispacht', 'PickupDate') IS NULL
    ALTER TABLE dbo.JEDispacht ADD PickupDate DATETIME NULL;
GO
IF COL_LENGTH('dbo.JEDispacht', 'DropoffDate') IS NULL
    ALTER TABLE dbo.JEDispacht ADD DropoffDate DATETIME NULL;
GO
IF COL_LENGTH('dbo.JEDispacht', 'ShippingTo') IS NULL
    ALTER TABLE dbo.JEDispacht ADD ShippingTo NVARCHAR(2000) NULL;
GO
