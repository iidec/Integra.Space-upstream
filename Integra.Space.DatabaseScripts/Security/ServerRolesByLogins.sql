CREATE TABLE [space].[server_roles_by_logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [sr_id] UNIQUEIDENTIFIER NOT NULL, 
    [sr_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_ServerRolesByLogins_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins]([srv_id], [lg_id]), 
    CONSTRAINT [FK_ServerRolesByLogins_ServerRoles] FOREIGN KEY ([sr_srv_id], [sr_id]) REFERENCES [space].[server_roles]([srv_id], [sr_id]), 
    PRIMARY KEY ([sr_srv_id], [lg_id], [lg_srv_id], [sr_id])
)
