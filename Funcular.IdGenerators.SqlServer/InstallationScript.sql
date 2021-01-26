-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- REVIEW YOUR MASTER KEY ENCRYPTION SETTINGS BEFORE EVEN THINKING ABOUT RUNNING THIS!
-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

-- MAKE SURE THE POST BUILD STEPS FOR COPYING THE .SNK AND .DLL TO C:\temp ARE ENABLED AND SUCCESSFUL 
-- SET @TargetDatabase NAME

-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-- REVIEW YOUR MASTER KEY ENCRYPTION SETTINGS BEFORE EVEN THINKING ABOUT RUNNING THIS!
-- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

USE MASTER 

DECLARE @TargetDatabaseName sysname
SET @TargetDatabaseName = 'Base36IdTesting'

DECLARE @IsMasterKeyEncrypted bit = 0
-- HELPER:		SELECT name, is_master_key_encrypted_by_server FROM sys.databases 
SELECT TOP 1 @IsMasterKeyEncrypted = d.is_master_key_encrypted_by_server
FROM sys.databases d
WHERE d.name = @TargetDatabaseName;

IF @IsMasterKeyEncrypted = 0 BEGIN
CREATE MASTER KEY ENCRYPTION
	BY PASSWORD = 'Funcular.IdGenerators.SqlServer'
END

OPEN MASTER KEY DECRYPTION BY PASSWORD = 'Funcular.IdGenerators.SqlServer'
IF NOT EXISTS (SELECT 1 FROM sys.asymmetric_keys where name = 'Funcular.IdGenerators.SqlServer.AsymmetricKey') BEGIN
	-- HELPER:		DROP ASYMMETRIC KEY [Funcular.IdGenerators.SqlServer.AsymmetricKey]
	CREATE ASYMMETRIC KEY [Funcular.IdGenerators.SqlServer.AsymmetricKey]
	FROM FILE='C:\temp\Funcular.IdGenerators.SqlServer.snk'
END

GRANT VIEW DEFINITION ON ASYMMETRIC KEY::[Funcular.IdGenerators.SqlServer.AsymmetricKey] TO [BUILTIN\Administrators]
GRANT CONTROL ON ASYMMETRIC KEY::[Funcular.IdGenerators.SqlServer.AsymmetricKey] TO [BUILTIN\Administrators]
GRANT REFERENCES ON ASYMMETRIC KEY::[Funcular.IdGenerators.SqlServer.AsymmetricKey] TO [BUILTIN\Administrators]

IF NOT EXISTS (SELECT 1 from sys.sql_logins where name = 'Funcular.IdGenerators.SqlServer.StrongNameKeyLogin') BEGIN
	-- HELPER:		DROP LOGIN [Funcular.IdGenerators.SqlServer.StrongNameKeyLogin] 
	CREATE LOGIN [Funcular.IdGenerators.SqlServer.StrongNameKeyLogin] 
	FROM ASYMMETRIC KEY [Funcular.IdGenerators.SqlServer.AsymmetricKey]
END

GRANT UNSAFE ASSEMBLY TO [Funcular.IdGenerators.SqlServer.StrongNameKeyLogin] 

EXEC ('USE ' + @TargetDatabaseName)
GO

IF EXISTS (SELECT 1 from sys.assemblies where name = 'Funcular.IdGenerators.SqlServer') BEGIN
	DROP ASSEMBLY [Funcular.IdGenerators.SqlServer] 
END
GO

CREATE ASSEMBLY [Funcular.IdGenerators.SqlServer] FROM 'c:\temp\Funcular.IdGenerators.SqlServer.dll'
WITH permission_set = unsafe
GO

EXEC sp_configure 'clr enabled' , '1';  
RECONFIGURE;    
GO

CREATE FUNCTION [dbo].[NewBase36Id]()
RETURNS NVARCHAR (20)
AS
 EXTERNAL NAME [Funcular.IdGenerators.SqlServer].[SqlServerIdGenerator].[NewBase36Id]
GO

CREATE FUNCTION [dbo].[NewBase36IdFromTimestamp](@creationTimestamp AS datetime)
RETURNS NVARCHAR (20)
AS
 EXTERNAL NAME [Funcular.IdGenerators.SqlServer].[SqlServerIdGenerator].[NewBase36IdFromTimestamp]
GO


CREATE FUNCTION [dbo].[NewBase36Id16]()
RETURNS NVARCHAR (16)
AS
 EXTERNAL NAME [Funcular.IdGenerators.SqlServer].[SqlServerIdGenerator].[NewBase36Id16]
GO

CREATE FUNCTION [dbo].[NewBase36Id16FromTimestamp](@creationTimestamp AS datetime)
RETURNS NVARCHAR (16)
AS
 EXTERNAL NAME [Funcular.IdGenerators.SqlServer].[SqlServerIdGenerator].[NewBase36Id16FromTimestamp]
GO
