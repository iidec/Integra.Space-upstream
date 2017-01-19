CREATE VIEW [space].[vw_users]
	AS
	SELECT 
	   CAST([dbusr_id] as varchar(36)) as UserId
      ,[dbusr_name] as UserName
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([default_sch_id] as varchar(36)) as DefaultSchemaId
      ,CAST([default_sch_db_id] as varchar(36)) as DefaultSchemaDatabaseId
      ,CAST([default_sch_srv_id] as varchar(36)) as DefaultSchemaServerId
      ,CAST([lg_id] as varchar(36)) as LoginId
      ,CAST([lg_srv_id] as varchar(36)) as LoginServerId
      ,[is_active] as IsActive
  FROM [space].[database_users]