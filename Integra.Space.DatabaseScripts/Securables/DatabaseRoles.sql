CREATE TABLE [space].[database_roles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_name] NVARCHAR(50) NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [is_active] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_DatabaseRoles_Databases] FOREIGN KEY ([srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    PRIMARY KEY ([dbr_id], [srv_id], [db_id]),
    CONSTRAINT [FK_DatabaseRoles_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]), 
    CONSTRAINT [CK_DatabaseRoles_db_ownership] CHECK ([owner_db_id] = [db_id]),
	CONSTRAINT [CK_DatabaseRoles_server_ownership] CHECK ([owner_srv_id] = [srv_id]), 
    CONSTRAINT [AK_database_roles_name] UNIQUE ([srv_id], [db_id], [dbr_name])
)
