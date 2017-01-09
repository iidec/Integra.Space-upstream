USE [Space]
GO

-- CHECK CONSTRAINTS

-- database_roles table

ALTER TABLE [space].[database_roles]  WITH NOCHECK ADD  CONSTRAINT [CK_DatabaseRoles_db_ownership] CHECK  (([owner_db_id]=[db_id]))
GO

ALTER TABLE [space].[database_roles] CHECK CONSTRAINT [CK_DatabaseRoles_db_ownership]
GO

ALTER TABLE [space].[database_roles]  WITH NOCHECK ADD  CONSTRAINT [CK_DatabaseRoles_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[database_roles] CHECK CONSTRAINT [CK_DatabaseRoles_server_ownership]
GO

-- databases table

ALTER TABLE [space].[databases]  WITH NOCHECK ADD  CONSTRAINT [CK_Databases_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[databases] CHECK CONSTRAINT [CK_Databases_server_ownership]
GO

-- endpoints table

ALTER TABLE [space].[endpoints]  WITH NOCHECK ADD  CONSTRAINT [CK_Endpoints_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[endpoints] CHECK CONSTRAINT [CK_Endpoints_server_ownership]
GO

-- schemas table

ALTER TABLE [space].[schemas]  WITH NOCHECK ADD  CONSTRAINT [CK_Schemas_db_ownership] CHECK  (([owner_db_id]=[db_id]))
GO

ALTER TABLE [space].[schemas] CHECK CONSTRAINT [CK_Schemas_db_ownership]
GO

ALTER TABLE [space].[schemas]  WITH NOCHECK ADD  CONSTRAINT [CK_Schemas_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[schemas] CHECK CONSTRAINT [CK_Schemas_server_ownership]
GO

-- sources table

ALTER TABLE [space].[sources]  WITH NOCHECK ADD  CONSTRAINT [CK_Sources_db_ownership] CHECK  (([owner_db_id]=[db_id]))
GO

ALTER TABLE [space].[sources] CHECK CONSTRAINT [CK_Sources_db_ownership]
GO

ALTER TABLE [space].[sources]  WITH NOCHECK ADD  CONSTRAINT [CK_Sources_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[sources] CHECK CONSTRAINT [CK_Sources_server_ownership]
GO

-- streams table

ALTER TABLE [space].[streams]  WITH NOCHECK ADD  CONSTRAINT [CK_Streams_db_ownership] CHECK  (([owner_db_id]=[db_id]))
GO

ALTER TABLE [space].[streams] CHECK CONSTRAINT [CK_Streams_db_ownership]
GO

ALTER TABLE [space].[streams]  WITH NOCHECK ADD  CONSTRAINT [CK_Streams_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[streams] CHECK CONSTRAINT [CK_Streams_server_ownership]
GO

-- views table

ALTER TABLE [space].[views]  WITH NOCHECK ADD  CONSTRAINT [CK_Views_db_ownership] CHECK  (([owner_db_id]=[db_id]))
GO

ALTER TABLE [space].[views] CHECK CONSTRAINT [CK_Views_db_ownership]
GO

ALTER TABLE [space].[views]  WITH NOCHECK ADD  CONSTRAINT [CK_Views_server_ownership] CHECK  (([owner_server_id]=[ser_id]))
GO

ALTER TABLE [space].[views] CHECK CONSTRAINT [CK_Views_server_ownership]
GO