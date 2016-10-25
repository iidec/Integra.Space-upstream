CREATE TABLE [space].[hierarchy_permissions]
(
	[sc_id] UNIQUEIDENTIFIER NOT NULL , 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id_parent] UNIQUEIDENTIFIER NULL, 
    [gp_id_parent] UNIQUEIDENTIFIER NULL, 
    [hp_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    CONSTRAINT [FK_hierarchy_permissions_permissions_by_securables1] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [FK_hierarchy_permissions_permissions_by_securables2] FOREIGN KEY ([sc_id_parent], [gp_id_parent]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id]), 
    CONSTRAINT [PK_hierarchy_permissions] PRIMARY KEY ([hp_id]), 
    CONSTRAINT [AK_hierarchy_permissions_permission_with_parent] UNIQUE ([sc_id], [gp_id], [sc_id_parent], [gp_id_parent])
)
