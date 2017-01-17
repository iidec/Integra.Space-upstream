CREATE VIEW [space].[vw_streams]
	AS 
	SELECT 
	   CAST([st_id] as varchar(36)) as StreamId
      ,[st_name] as StreamName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([sch_id] as varchar(36)) as SchemaId
      ,[st_query] as Query
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_db_id] as varchar(36)) as OwnerDatabaseId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
      ,[is_active] as IsActive
  FROM [space].[streams]