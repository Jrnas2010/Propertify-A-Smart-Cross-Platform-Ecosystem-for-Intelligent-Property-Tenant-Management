CREATE TABLE Units (
    Id                INT              NOT NULL IDENTITY(1,1),
    UnitNumber        NVARCHAR(50)     NOT NULL,
    FloorNumber       INT              NOT NULL DEFAULT 0,
    RentAmount        DECIMAL(18,2)    NOT NULL DEFAULT 0.00,
    Area              FLOAT            NOT NULL DEFAULT 0,
    IsOccupied        BIT              NOT NULL DEFAULT 0,
    Status            NVARCHAR(50)     NOT NULL DEFAULT 'Vacant',
    ElectricityMeter  NVARCHAR(100)    NULL,
    WaterMeter        NVARCHAR(100)    NULL,
    Bedrooms          INT              NOT NULL DEFAULT 0,
    Bathrooms         INT              NOT NULL DEFAULT 0,
    Kitchens          INT              NOT NULL DEFAULT 0,
    LivingRooms       INT              NOT NULL DEFAULT 0,
    Majlis            INT              NOT NULL DEFAULT 0,
    UnitImages        NVARCHAR(MAX)    NULL,
    VideoPath         NVARCHAR(500)    NULL,
    PropertyId        INT              NOT NULL,

    CONSTRAINT PK_Units PRIMARY KEY (Id),
    CONSTRAINT FK_Units_Properties FOREIGN KEY (PropertyId)
        REFERENCES Properties(Id) ON DELETE CASCADE
);