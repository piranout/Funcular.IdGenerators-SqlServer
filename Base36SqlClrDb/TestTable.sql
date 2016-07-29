CREATE TABLE [dbo].[TestTable]
(
	[Id] CHAR(20) NOT NULL PRIMARY KEY, 
    [DateCreatedUtc] DATETIMEOFFSET NOT NULL DEFAULT getutcdate()
)
