CREATE TABLE [space].[granular_permissions]
(
	[gp_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [gp_name] NVARCHAR(50) NOT NULL, 
    [gp_code] NVARCHAR(5) NULL, 
    CONSTRAINT [AK_GranularPermission_gp_name] UNIQUE ([gp_name])
)
