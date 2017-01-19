CREATE VIEW [space].[vw_source_columns]
	AS 
	SELECT 
	   CAST([srv_id] as varchar(36)) as ServerId
      ,CAST([db_id] as varchar(36)) as DatabaseId
      ,CAST([sch_id] as varchar(36)) as SchemaId
      ,CAST([so_id] as varchar(36)) as SourceId
      ,[so_column_name] as ColumnName
      ,[so_column_type] as ColumnType
      ,CAST([so_culumn_id] as varchar(36)) as ColumnId
      ,[so_column_index] as ColumnIndex
  FROM [space].[source_columns]