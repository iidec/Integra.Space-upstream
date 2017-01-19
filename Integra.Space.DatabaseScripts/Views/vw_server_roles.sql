CREATE VIEW [space].[vw_server_roles]
	AS 
	SELECT 
	 CAST([sr_id]  as varchar(36)) as ServerRoleId
    ,[sr_name] as ServerRoleName
    ,CAST([srv_id]  as varchar(36)) as ServerId
	FROM [space].[server_roles]

