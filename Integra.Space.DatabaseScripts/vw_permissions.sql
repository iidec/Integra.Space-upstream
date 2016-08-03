CREATE VIEW [space].[vw_permissions]
	AS
	-- Servers
	SELECT null as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [srv_id] as securableId,
	       [lg_srv_id] as serverIdOfPrincipal, null as databaseIdOfPrincipal, [lg_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [servers_assigned_permissions_to_logins]
	UNION
	SELECT null as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [srv_id] as securableId,
	       [lg_srv_id] as serverIdOfPrincipal, null as databaseIdOfPrincipal, [lg_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [servers_assigned_permissions_to_server_roles]
	Union
	-- Logins
	SELECT [on_lg_srv_id] as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [on_lg_id] as securableId,
	       [lg_srv_id] as serverIdOfPrincipal, null as databaseIdOfPrincipal, [lg_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [logins_assigned_permissions_to_logins]
	UNION
	SELECT [lg_srv_id] as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [lg_id] as securableId,
	       [lg_srv_id] as serverIdOfPrincipal, null as databaseIdOfPrincipal, [lg_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [logins_assigned_permissions_to_server_roles]
	Union
	-- Databases
	SELECT [db_srv_id] as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [db_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [database_assigned_permissions_to_users]
	UNION
	SELECT [db_srv_id] as serverIdOfSecurable ,null as databaseIdOfSecurable, null as schemaIdOfSecurable, [db_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [database_assigned_permissions_to_dbroles]
	Union
	-- Roles
	SELECT [dbr_srv_id] as serverIdOfSecurable ,[dbr_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [dbr_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [dbroles_assigned_permissions_to_users]
	UNION
	SELECT [on_dbr_srv_id] as serverIdOfSecurable ,[on_dbr_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [on_dbr_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [dbroles_assigned_permissions_to_dbroles]
	Union
	-- Users
	SELECT [on_dbusr_srv_id] as serverIdOfSecurable ,[on_dbusr_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [on_dbusr_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [user_assigned_permissions_to_users]
	UNION
	SELECT [dbusr_srv_id] as serverIdOfSecurable ,[dbusr_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [dbusr_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [user_assigned_permissions_to_dbroles]
	Union
	-- Views
	SELECT [vw_srv_id] as serverIdOfSecurable ,[vw_db_id] as databaseIdOfSecurable, [vw_sch_id] as schemaIdOfSecurable, [vw_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [view_assigned_permissions_to_users]
	UNION
	SELECT [vw_ser_id] as serverIdOfSecurable ,[vw_db_id] as databaseIdOfSecurable, [vw_sch_id] as schemaIdOfSecurable, [vw_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [view_assigned_permissions_to_dbroles]
	Union
	-- Streams
	SELECT [st_srv_id] as serverIdOfSecurable ,[st_db_id] as databaseIdOfSecurable, [st_sch_id] as schemaIdOfSecurable, [st_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [stream_assigned_permissions_to_users]
	UNION
	SELECT [st_srv_id] as serverIdOfSecurable ,[st_db_id] as databaseIdOfSecurable, [st_sch_id] as schemaIdOfSecurable, [st_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [stream_assigned_permissions_to_dbroles]
	Union
	-- Schemas
	SELECT [sch_srv_id] as serverIdOfSecurable ,[sch_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [sch_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [schema_assigned_permissions_to_users]
	UNION
	SELECT [sch_srv_id] as serverIdOfSecurable ,[sch_db_id] as databaseIdOfSecurable, null as schemaIdOfSecurable, [sch_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [schema_assigned_permissions_to_dbroles]
	Union
	-- Sources
	SELECT [so_srv_id] as serverIdOfSecurable ,[so_db_id] as databaseIdOfSecurable ,[so_sch_id] as schemaIdOfSecurable, [so_id] as securableId,
	       [dbusr_srv_id] as serverIdOfPrincipal, [dbusr_db_id] as databaseIdOfPrincipal, [dbusr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [source_assigned_permissions_to_users]
	UNION
	SELECT [so_srv_id] as serverIdOfSecurable ,[so_db_id] as databaseIdOfSecurable ,[so_sch_id] as schemaIdOfSecurable, [so_id] as securableId,
	       [dbr_srv_id] as serverIdOfPrincipal, [dbr_db_id] as databaseIdOfPrincipal, [dbr_id] as principalId,
		   [sc_id] as securableClassId, [gp_id] as granularPermissionId
    FROM [source_assigned_permissions_to_dbroles]

