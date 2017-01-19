CREATE TABLE [space].[endpoints_assigned_permissions_to_logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [ep_id] UNIQUEIDENTIFIER NOT NULL, 
    [ep_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [granted] BIT NOT NULL DEFAULT 0, 
    [denied] BIT NOT NULL DEFAULT 0, 
    [with_grant_option] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_EndpointsAssignedPermissionsToLogins] PRIMARY KEY ([lg_srv_id], [lg_id], [sc_id], [gp_id], [ep_id], [ep_srv_id]),
	CONSTRAINT [FK_EndpointsAssignedPermissionsToLogins_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_EndpointsAssignedPermissionsToLogins_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]), 
    CONSTRAINT [FK_EndpointsAssignedPermissionsToLogins_Endpoints] FOREIGN KEY ([ep_id], [ep_srv_id]) REFERENCES [space].[endpoints]([ep_id], [srv_id])
)
