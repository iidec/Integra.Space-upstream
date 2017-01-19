CREATE TABLE [space].[server_roles]
(
	[sr_id] UNIQUEIDENTIFIER NOT NULL , 
    [sr_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [AK_ServerRoles_ser_name] UNIQUE ([sr_name]), 
    CONSTRAINT [FK_ServerRoles_Server] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 
    PRIMARY KEY ([srv_id], [sr_id])
)
