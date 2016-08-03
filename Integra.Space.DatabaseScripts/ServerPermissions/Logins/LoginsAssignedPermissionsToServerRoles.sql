CREATE TABLE [space].[logins_assigned_permissions_to_server_roles]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sr_id] UNIQUEIDENTIFIER NOT NULL, 
    [sr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_LoginsAssignedPermissionsToServerRoles] PRIMARY KEY ([sr_srv_id], [lg_id], [lg_srv_id], [sc_id], [gp_id], [sr_id]),
	CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]),
    CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_ServerRoles] FOREIGN KEY ([sr_srv_id], [sr_id]) REFERENCES [space].[server_roles]([srv_id], [sr_id])
)
