CREATE TABLE Users (
    Id        INT             NOT NULL IDENTITY(1,1),
    FullName  NVARCHAR(255)   NOT NULL,
    Email     NVARCHAR(255)   NOT NULL,
    Password  NVARCHAR(500)   NOT NULL,
    Role      NVARCHAR(50)    NOT NULL,
    TenantId  INT             NULL,

    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE SET NULL,
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Owner', 'Tenant'))
)