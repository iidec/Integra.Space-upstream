CREATE TABLE [space].[servers]
(
	[srv_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [srv_name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [AK_Server_ser_name] UNIQUE ([srv_name])    
)
