CREATE VIEW [space].[vw_database_roles]
	AS 
	SELECT 
	   CAST([dbr_id] as varchar(36)) as DatabaseRoleId
      ,[dbr_name] as DatabaseRoleName
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_db_id] as varchar(36)) as OwnerDatabaseId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
      ,[is_active] as IsActive
  FROM [space].[database_roles]
