CREATE TABLE [dbo].[UtilityBills ] (
    BillId           INT             NOT NULL IDENTITY(1,1),
    ServiceType      NVARCHAR(50)    NOT NULL,   -- 'Electricity' or 'Water'
    PreviousReading  DECIMAL(18,2)   NOT NULL,
    CurrentReading   DECIMAL(18,2)   NOT NULL,
    TotalAmount      DECIMAL(18,2)   NOT NULL DEFAULT 0.00,
    IssueDate        DATETIME2       NOT NULL DEFAULT GETDATE(),
    UnitId           INT             NOT NULL,
    TenantId         INT             NOT NULL,

    CONSTRAINT PK_UtilityBills PRIMARY KEY (BillId),
    CONSTRAINT FK_UtilityBills_Units FOREIGN KEY (UnitId)
        REFERENCES Units(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_UtilityBills_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_UtilityBills_ServiceType CHECK (ServiceType IN ('Electricity', 'Water'))
);

