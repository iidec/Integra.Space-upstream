CREATE TABLE [space].[permissions_by_securables]
(
	[sc_id] UNIQUEIDENTIFIER NOT NULL , 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id_parent] UNIQUEIDENTIFIER NULL, 
    [gp_id_parent] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_PermissionBySecurable] PRIMARY KEY ([sc_id], [gp_id]), 
    CONSTRAINT [FK_PermissionBySecurable_SecurableClass] FOREIGN KEY ([sc_id]) REFERENCES [space].[securable_classes]([sc_id]), 
    CONSTRAINT [FK_PermissionBySecurable_GranularPermission] FOREIGN KEY ([gp_id]) REFERENCES [space].[granular_permissions]([gp_id]), 
    CONSTRAINT [FK_PermissionsBySecurables_PermissionsBySecurables] FOREIGN KEY ([sc_id_parent], [gp_id_parent]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]) 
)
