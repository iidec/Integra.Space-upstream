CREATE TABLE [space].[stream_assigned_permissions_to_dbroles]
(
	[dbr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([dbr_id], [dbr_db_id], [dbr_srv_id], [sc_id], [gp_id], [st_id], [st_srv_id], [st_db_id], [st_sch_id]), 
    CONSTRAINT [FK_StreamAssignedPermissionsToDBRoles_Sources] FOREIGN KEY ([st_id], [st_sch_id], [st_db_id], [st_srv_id]) REFERENCES [space].[streams]([st_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [FK_StreamAssignedPermissionsToDBRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_StreamAssignedPermissionsToDBRoles_DatabaseRoles] FOREIGN KEY ([dbr_id], [dbr_srv_id], [dbr_db_id]) REFERENCES [space].[database_roles]([dbr_id], [srv_id], [db_id])
)
