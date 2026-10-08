-- Version 2: execute AFTER JEDispacht.sql and BEFORE publishing the new DACs.
-- No inventory balances are modified by this script. No historical snapshot is invented.
IF OBJECT_ID(N'dbo.JEINDispachtMaterial', N'V') IS NOT NULL
    THROW 50001, 'JEINDispachtMaterial exists as a SQL view. Review it before installing the new table.', 1;
GO
IF OBJECT_ID(N'dbo.JEINDispachtMaterial', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JEINDispachtMaterial (
        CompanyID INT NOT NULL DEFAULT(0),
        DispachtNbr NVARCHAR(15) NOT NULL,
        DispachtLineNbr INT NOT NULL,
        SourceLineNbr INT NOT NULL,
        DocType CHAR(2) NULL,
        OrderType NVARCHAR(2) NULL,
        OrderNbr NVARCHAR(15) NULL,
        InventoryID INT NOT NULL,
        SubItemID INT NULL,
        Descr NVARCHAR(256) NULL,
        Qty DECIMAL(25,6) NULL,
        UOM NVARCHAR(6) NOT NULL,
        SiteID INT NOT NULL,
        LocationID INT NOT NULL,
        TaskID INT NULL,
        CostCodeID INT NULL,
        DispatchQty DECIMAL(25,6) NOT NULL DEFAULT(0),
        CreatedByID UNIQUEIDENTIFIER NULL,
        CreatedDateTime DATETIME NULL,
        LastModifiedByID UNIQUEIDENTIFIER NULL,
        LastModifiedDateTime DATETIME NULL,
        tstamp ROWVERSION NOT NULL,
        CONSTRAINT JEINDispachtMaterial_PK PRIMARY KEY (CompanyID, DispachtNbr, DispachtLineNbr, SourceLineNbr),
        CONSTRAINT JEINDispachtMaterial_Qty CHECK (DispatchQty >= 0)
    );
END;
GO
IF OBJECT_ID(N'dbo.JEDispachtIssue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JEDispachtIssue (
        CompanyID INT NOT NULL DEFAULT(0),
        DispachtNbr NVARCHAR(15) NOT NULL,
        SiteID INT NOT NULL,
        DocType CHAR(1) NOT NULL,
        RefNbr NVARCHAR(15) NOT NULL,
        ReasonCode NVARCHAR(20) NULL,
        CreatedByID UNIQUEIDENTIFIER NULL,
        CreatedDateTime DATETIME NULL,
        tstamp ROWVERSION NOT NULL,
        CONSTRAINT JEDispachtIssue_PK PRIMARY KEY (CompanyID, DispachtNbr, SiteID),
        CONSTRAINT JEDispachtIssue_Ref UNIQUE (CompanyID, DocType, RefNbr)
    );
END;
GO
