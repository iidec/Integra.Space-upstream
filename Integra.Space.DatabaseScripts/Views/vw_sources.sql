CREATE VIEW [space].[vw_sources]
	AS 
	SELECT 
	   CAST([so_id] as varchar(36)) as SourceId
      ,[so_name] as SourceName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([sch_id] as varchar(36)) as SchemaId
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_db_id] as varchar(36)) as OwnerDatabaseId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
      ,[is_active] as IsActive
      ,[cache_durability] as CacheDurability
      ,[cache_size] as CacheSize
      ,[persistent] as Persistent
  FROM [space].[sources]