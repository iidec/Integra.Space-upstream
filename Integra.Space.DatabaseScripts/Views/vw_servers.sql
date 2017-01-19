CREATE VIEW [space].[vw_servers]
	AS 
	SELECT 
	 CAST([srv_id]  as varchar(36)) as ServerId
    ,[srv_name] as ServerName
	FROM [space].[servers]
