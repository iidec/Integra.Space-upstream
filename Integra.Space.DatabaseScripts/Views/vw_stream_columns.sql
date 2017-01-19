CREATE VIEW [space].[vw_stream_columns]
	AS 
	SELECT
	   CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([sch_id] as varchar(36)) as SchemaId
      ,CAST([st_id] as varchar(36)) as StreamId
      ,[st_column_name] as ColumnName
      ,[st_column_type] as ColumnType
      ,CAST([st_culumn_id] as varchar(36)) as ColumnId
  FROM [space].[stream_columns]