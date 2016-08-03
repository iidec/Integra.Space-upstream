CREATE TABLE [space].[databases]
(
	[db_id] UNIQUEIDENTIFIER NOT NULL , 
    [db_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [AK_Database_db_name] UNIQUE ([db_name]), 
    CONSTRAINT [FK_Database_Server] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 
    PRIMARY KEY ([srv_id], [db_id]),
    CONSTRAINT [FK_Databases_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]) , 
	CONSTRAINT [CK_Databases_server_ownership] CHECK ([owner_srv_id] = [srv_id]) 
)
