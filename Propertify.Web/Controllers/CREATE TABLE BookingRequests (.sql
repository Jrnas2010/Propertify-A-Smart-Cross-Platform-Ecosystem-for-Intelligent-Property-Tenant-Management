CREATE TABLE BookingRequests (
    Id           INT             NOT NULL IDENTITY(1,1),
    VisitorName  NVARCHAR(255)   NOT NULL DEFAULT '',
    Phone        NVARCHAR(20)    NOT NULL DEFAULT '',
    Email        NVARCHAR(255)   NOT NULL DEFAULT '',
    RequestType  NVARCHAR(100)   NOT NULL DEFAULT '',
    UnitNumber   NVARCHAR(50)    NULL,
    Notes        NVARCHAR(MAX)   NOT NULL DEFAULT '',
    SubmittedAt  DATETIME2       NOT NULL DEFAULT GETDATE(),
    IsRead       BIT             NOT NULL DEFAULT 0,

    CONSTRAINT PK_BookingRequests PRIMARY KEY (Id)
);