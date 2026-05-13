CREATE TABLE Contracts (
    Id           INT             NOT NULL IDENTITY(1,1),
    StartDate    DATETIME2       NOT NULL,
    EndDate      DATETIME2       NOT NULL,
    RentAmount   DECIMAL(18,2)   NOT NULL DEFAULT 0.00,
    MonthlyRent  DECIMAL(18,2)   NOT NULL DEFAULT 0.00,
    Status       NVARCHAR(50)    NOT NULL DEFAULT 'Active',
    TenantId     INT             NOT NULL,
    UnitId       INT             NOT NULL,

    CONSTRAINT PK_Contracts PRIMARY KEY (Id),
    CONSTRAINT FK_Contracts_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Contracts_Units FOREIGN KEY (UnitId)
        REFERENCES Units(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Contracts_Status CHECK (Status IN ('Active', 'Expired', 'Terminated'))
);