CREATE TABLE [space].[schema_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbr_id], [dbr_srv_id], [dbr_db_id], [sc_id], [gp_id], [sch_srv_id], [sch_db_id], [sch_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([sch_srv_id], [sch_db_id], [sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
