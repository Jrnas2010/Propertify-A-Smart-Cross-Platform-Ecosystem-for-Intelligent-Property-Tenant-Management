CREATE TABLE SystemMessages (
    MessageId INT PRIMARY KEY IDENTITY(1,1),
    Subject NVARCHAR(200),
    Content NVARCHAR(MAX),
    SentAt DATETIME DEFAULT GETDATE(),
    Target NVARCHAR(50), -- e.g., All, Building A
    IsBroadcast BIT DEFAULT 1
);