CREATE TABLE [space].[database_assigned_permissions_to_dbroles]
(
	[dbrole_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbrole_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbrole_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL,
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbrole_id], [dbrole_db_id], [dbrole_srv_id], [sc_id], [gp_id], [db_srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_Databases] FOREIGN KEY ([db_srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbrole_id], [dbrole_srv_id], [dbrole_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
