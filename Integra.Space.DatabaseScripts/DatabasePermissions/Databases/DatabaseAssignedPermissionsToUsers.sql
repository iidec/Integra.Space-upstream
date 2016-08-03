CREATE TABLE [space].[database_assigned_permissions_to_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY ([dbusr_id], [dbusr_db_id], [dbusr_srv_id], [sc_id], [gp_id], [db_srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToUsers_Sources] FOREIGN KEY ([db_srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToUsers_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id])
)
