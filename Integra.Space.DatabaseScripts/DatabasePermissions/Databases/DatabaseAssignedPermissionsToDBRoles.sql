CREATE TABLE [space].[database_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY ([dbr_id], [dbr_db_id], [dbr_srv_id], [sc_id], [gp_id], [db_srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([db_srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_DatabaseAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
