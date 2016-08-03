CREATE TABLE [space].[logins_assigned_permissions_to_logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id] UNIQUEIDENTIFIER NOT NULL, 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_lg_id] UNIQUEIDENTIFIER NOT NULL, 
    [on_lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_LoginsAssignedPermissionsToLogins] PRIMARY KEY ([lg_id], [lg_srv_id], [sc_id], [gp_id], [on_lg_id], [on_lg_srv_id]),
	CONSTRAINT [FK_LoginsAssignedPermissionsToLogins_PermissionsBySecurables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_LoginsAssignedPermissionsToLogins_Logins1] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]), 
    CONSTRAINT [FK_LoginsAssignedPermissionsToLogins_Logins2] FOREIGN KEY ([on_lg_srv_id], [on_lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id])
)
