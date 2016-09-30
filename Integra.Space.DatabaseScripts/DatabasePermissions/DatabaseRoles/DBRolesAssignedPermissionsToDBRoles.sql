CREATE TABLE [space].[dbroles_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_dbr_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbr_id], [dbr_db_id], [dbr_srv_id], [sc_id], [gp_id], [on_dbr_srv_id], [on_dbr_db_id], [on_dbr_id]), 
    CONSTRAINT [FK_DBRolesAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([on_dbr_id], [on_dbr_srv_id], [on_dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id]), 
    CONSTRAINT [FK_DBRolesAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_DBRolesAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
