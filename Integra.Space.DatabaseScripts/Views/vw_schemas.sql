CREATE VIEW [space].[vw_schemas]
	AS
	SELECT 
	   CAST([sch_id] as varchar(36)) as SchemaId
      ,[sch_name] as SchemaName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_db_id] as varchar(36)) as OwnerDatabaseId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
  FROM [space].[schemas]
