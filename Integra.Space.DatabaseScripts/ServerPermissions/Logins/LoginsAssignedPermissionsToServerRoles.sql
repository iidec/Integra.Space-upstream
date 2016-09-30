CREATE TABLE [space].[logins_assigned_permissions_to_server_roles]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL,
	[srvrole_id] UNIQUEIDENTIFIER NOT NULL , 
    [srvrole_srv_id] UNIQUEIDENTIFIER NOT NULL,  
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_LoginsAssignedPermissionsToServerRoles] PRIMARY KEY ([srvrole_srv_id], [lg_id], [lg_srv_id], [sc_id], [gp_id], [srvrole_id]),
	CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]),
    CONSTRAINT [FK_LoginsAssignedPermissionsToServerRoles_ServerRoles] FOREIGN KEY ([srvrole_srv_id], [srvrole_id]) REFERENCES [space].[server_roles]([srv_id], [sr_id])
)
