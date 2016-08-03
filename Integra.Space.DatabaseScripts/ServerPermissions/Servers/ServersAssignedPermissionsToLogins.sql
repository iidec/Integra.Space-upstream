CREATE TABLE [space].[servers_assigned_permissions_to_logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_ServersAssignedPermissionsToLogins] PRIMARY KEY ([lg_id], [lg_srv_id], [sc_id], [gp_id], [srv_id]),
	CONSTRAINT [FK_ServersAssignedPermissionsToLogins_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_ServersAssignedPermissionsToLogins_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]), 
    CONSTRAINT [FK_ServersAssignedPermissionsToLogins_Servers] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id])
)
