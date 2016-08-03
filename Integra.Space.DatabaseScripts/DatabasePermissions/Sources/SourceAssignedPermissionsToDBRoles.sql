CREATE TABLE [space].[source_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [so_id] UNIQUEIDENTIFIER NOT NULL, 
    [so_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [so_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [so_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY ([dbr_id], [dbr_db_id], [dbr_srv_id], [sc_id], [gp_id], [so_id], [so_srv_id], [so_db_id], [so_sch_id]), 
    CONSTRAINT [FK_SourceAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([so_id], [so_sch_id], [so_db_id], [so_srv_id]) REFERENCES [space].[sources]([so_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [FK_SourceAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_SourceAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
