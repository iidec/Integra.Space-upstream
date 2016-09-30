CREATE TABLE [space].[view_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_ser_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbr_id], [dbr_db_id], [dbr_srv_id], [sc_id], [gp_id], [vw_id], [vw_ser_id], [vw_db_id], [vw_sch_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([vw_id], [vw_sch_id], [vw_db_id], [vw_ser_id]) REFERENCES [space].[views]([vw_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
