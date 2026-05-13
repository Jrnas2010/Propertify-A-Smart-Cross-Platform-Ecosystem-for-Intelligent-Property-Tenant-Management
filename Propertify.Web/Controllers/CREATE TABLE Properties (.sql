CREATE TABLE Properties (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(255)   NOT NULL,
    Type        NVARCHAR(100)   NOT NULL,
    Location    NVARCHAR(500)   NOT NULL,
    TotalUnits  INT             NOT NULL DEFAULT 0,
    ImagePath   NVARCHAR(500)   NULL,
    ImageUrl    NVARCHAR(500)   NULL,

    CONSTRAINT PK_Properties PRIMARY KEY (Id)
);