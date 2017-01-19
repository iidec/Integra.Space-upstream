CREATE VIEW [space].[vw_databases]
	AS 
	SELECT
	   CAST([db_id] as varchar(36)) as DatabaseId
      ,[db_name] as DatabaseName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
      ,[is_active] as IsActive
  FROM [space].[databases]
