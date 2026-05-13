CREATE TABLE Tenants (
    Id               INT             NOT NULL IDENTITY(1,1),
    FirstNameAr      NVARCHAR(100)   NOT NULL,
    SecondNameAr     NVARCHAR(100)   NULL,
    ThirdNameAr      NVARCHAR(100)   NULL,
    LastNameAr       NVARCHAR(100)   NOT NULL,
    FirstNameEn      NVARCHAR(100)   NOT NULL,
    SecondNameEn     NVARCHAR(100)   NULL,
    ThirdNameEn      NVARCHAR(100)   NULL,
    LastNameEn       NVARCHAR(100)   NOT NULL,
    IdNumber         NVARCHAR(50)    NOT NULL,
    IdDocumentPath   NVARCHAR(500)   NULL,
    Phone            NVARCHAR(20)    NOT NULL,
    LeaseStartDate   DATETIME2       NOT NULL DEFAULT GETDATE(),
    LeaseEndDate     DATETIME2       NOT NULL,
    IsArchived       BIT             NOT NULL DEFAULT 0,
    UnitId           INT             NOT NULL,

    CONSTRAINT PK_Tenants PRIMARY KEY (Id),
    CONSTRAINT FK_Tenants_Units FOREIGN KEY (UnitId)
        REFERENCES Units(Id) ON DELETE NO ACTION
);