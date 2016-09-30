CREATE TABLE [space].[user_assigned_permissions_to_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbusr_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbusr_id], [dbusr_db_id], [dbusr_srv_id], [sc_id], [gp_id], [on_dbusr_srv_id], [on_dbusr_db_id], [on_dbusr_id]), 
    CONSTRAINT [FK_UserAssignedPermissionsToUsers_Sources] FOREIGN KEY ([on_dbusr_id], [on_dbusr_srv_id], [on_dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]), 
    CONSTRAINT [FK_UserAssignedPermissionsToUsers_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_UserAssignedPermissionsToUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id])
)
