CREATE VIEW [space].[vw_logins]
	AS
	SELECT
	   CAST([lg_id] as varchar(36)) as LoginId
      ,[lg_name] as LoginName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([default_db_id] as varchar(36)) as DefaultDatabaseId
      ,CAST([default_db_srv_id] as varchar(36)) DefaultDatabaseServerId
      ,[is_active] as IsActive
  FROM [space].[logins]