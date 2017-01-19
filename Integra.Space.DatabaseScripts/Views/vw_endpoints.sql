CREATE VIEW [space].[vw_endpoints]
	AS 
	SELECT 
	   CAST([ep_id] as varchar(36)) as EndPointId
      ,[ep_name] as EndpointName
      ,CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([owner_id] as varchar(36)) as OwnerId
      ,CAST([owner_srv_id] as varchar(36)) as OwnerServerId
      ,[is_active] as IsActive
  FROM [space].[endpoints]