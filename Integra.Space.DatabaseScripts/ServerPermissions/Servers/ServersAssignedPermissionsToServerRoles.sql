CREATE TABLE [space].[servers_assigned_permissions_to_server_roles]
(
	[srvrole_id] UNIQUEIDENTIFIER NOT NULL , 
    [srvrole_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL,
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_ServersAssignedPermissionsToServerRoles] PRIMARY KEY ([srvrole_id], [srvrole_srv_id], [sc_id], [gp_id], [srv_id]),
	CONSTRAINT [FK_ServersAssignedPermissionsToServerRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_ServersAssignedPermissionsToServerRoles_ServerRoles] FOREIGN KEY ([srvrole_srv_id], [srvrole_id]) REFERENCES [space].[server_roles]([srv_id], [sr_id]),
    CONSTRAINT [FK_ServersAssignedPermissionsToServerRoles_Servers] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id])
)
