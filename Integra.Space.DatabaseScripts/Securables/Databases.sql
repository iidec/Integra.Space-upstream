CREATE TABLE [space].[databases]
(
	[db_id] UNIQUEIDENTIFIER NOT NULL , 
    [db_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [is_active] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [AK_Database_db_name] UNIQUE ([db_name]), 
    CONSTRAINT [FK_Database_Server] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 
    PRIMARY KEY ([srv_id], [db_id]),
    CONSTRAINT [FK_Databases_Logins] FOREIGN KEY ([owner_srv_id], [owner_id]) REFERENCES [space].[logins]([srv_id], [lg_id]), 
	CONSTRAINT [CK_Databases_server_ownership] CHECK ([owner_srv_id] = [srv_id])
)
