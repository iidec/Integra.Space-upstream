CREATE TABLE [space].[securable_classes]
(
	[sc_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [sc_name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [AK_SecurableClass_soc_name] UNIQUE ([sc_name]) 
)
