CREATE TABLE [space].[schema_assigned_permissions_to_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY ([dbusr_id], [dbusr_db_id], [dbusr_srv_id], [sc_id], [gp_id], [sch_srv_id], [sch_db_id], [sch_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToUsers_Sources] FOREIGN KEY ([sch_srv_id], [sch_db_id], [sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToUsers_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_SchemaAssignedPermissionsToUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id])
)
