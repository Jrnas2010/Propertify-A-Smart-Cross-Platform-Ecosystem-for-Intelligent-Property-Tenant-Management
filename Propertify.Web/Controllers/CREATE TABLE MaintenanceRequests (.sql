CREATE TABLE MaintenanceRequests (
    Id            INT             NOT NULL IDENTITY(1,1),
    Title         NVARCHAR(255)   NOT NULL DEFAULT 'Maintenance Request',
    Description   NVARCHAR(MAX)   NULL,
    Cost          DECIMAL(18,2)   NOT NULL DEFAULT 0.00,
    Status        NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    Priority      NVARCHAR(50)    NOT NULL DEFAULT 'Normal',
    CreatedAt     DATETIME2       NOT NULL DEFAULT GETDATE(),
    ImagePath     NVARCHAR(500)   NULL,
    PropertyName  NVARCHAR(255)   NULL,
    PropertyId    INT             NOT NULL,
    UnitId        INT             NOT NULL,

    CONSTRAINT PK_MaintenanceRequests PRIMARY KEY (Id),
    CONSTRAINT FK_MaintenanceRequests_Properties FOREIGN KEY (PropertyId)
        REFERENCES Properties(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_MaintenanceRequests_Units FOREIGN KEY (UnitId)
        REFERENCES Units(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_MaintenanceRequests_Status CHECK (Status IN ('Pending', 'InProgress', 'Completed')),
    CONSTRAINT CK_MaintenanceRequests_Priority CHECK (Priority IN ('Low', 'Normal', 'High'))
);