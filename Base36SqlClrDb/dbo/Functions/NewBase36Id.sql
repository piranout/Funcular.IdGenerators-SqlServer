CREATE FUNCTION [dbo].[NewBase36]
( )
RETURNS NVARCHAR (50)
AS
 EXTERNAL NAME [Funcular.IdGenerators.SqlServer].[SqlServerIdGenerator].[NewBase36Id]

