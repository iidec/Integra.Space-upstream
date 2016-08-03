CREATE TABLE [space].[view_assigned_permissions_to_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY ([dbusr_id], [dbusr_db_id], [dbusr_srv_id], [sc_id], [gp_id], [vw_id], [vw_srv_id], [vw_db_id], [vw_sch_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToUsers_Sources] FOREIGN KEY ([vw_id], [vw_sch_id], [vw_db_id], [vw_srv_id]) REFERENCES [space].[views]([vw_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToUsers_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_ViewAssignedPermissionsToUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id])
)
