﻿CREATE TABLE [space].[endpoints_assigned_permissions_to_server_roles]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [ep_id] UNIQUEIDENTIFIER NOT NULL, 
    [ep_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_EndpointssAssignedPermissionsToServerRoles] PRIMARY KEY ([ep_srv_id], [lg_id], [lg_srv_id], [sc_id], [gp_id], [ep_id]),
	CONSTRAINT [FK_EndpointssAssignedPermissionsToServerRoles_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_EndpointssAssignedPermissionsToServerRoles_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]),
    CONSTRAINT [FK_EndpointssAssignedPermissionsToServerRoles_Endpoints] FOREIGN KEY ([ep_id], [ep_srv_id]) REFERENCES [space].[endpoints]([ep_id], [srv_id])
)
