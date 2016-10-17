CREATE TABLE [space].[hierarchy_permissions]
(
	[sc_id] UNIQUEIDENTIFIER NOT NULL , 
    [gp_id] UNIQUEIDENTIFIER NOT NULL, 
    [sc_id_parent] UNIQUEIDENTIFIER NULL, 
    [gp_id_parent] UNIQUEIDENTIFIER NULL, 
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    CONSTRAINT [FK_hierarchy_permissions_permissions_by_securables] FOREIGN KEY ([sc_id], [gp_id]) REFERENCES [space].[permissions_by_securables]([sc_id], [gp_id])
)
