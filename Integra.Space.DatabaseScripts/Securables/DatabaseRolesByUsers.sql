CREATE TABLE [space].[database_roles_by_users]
(
    [dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_id] UNIQUEIDENTIFIER NOT NULL, 
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_DatabaseRolesByUsers] PRIMARY KEY ([dbusr_db_id], [dbr_srv_id], [dbusr_srv_id], [dbusr_id], [dbr_id], [dbr_db_id]), 
    CONSTRAINT [FK_DatabaseRolesByUsers_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseRolesByUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]),
)
