CREATE TABLE [space].[source_columns]
(
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
	[so_id] UNIQUEIDENTIFIER NOT NULL , 
    [so_column_name] VARCHAR(50) NOT NULL, 
    [so_column_type] VARCHAR(2000) NOT NULL, 
    [so_culumn_id] UNIQUEIDENTIFIER NOT NULL, 
    [so_column_index] TINYINT NOT NULL, 
    PRIMARY KEY ([so_culumn_id]), 
    CONSTRAINT [FK_source_columns_sources] FOREIGN KEY ([so_id], [sch_id], [db_id], [srv_id]) REFERENCES [space].[sources]([so_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [AK_source_columns_so_id_so_column_name] UNIQUE ([so_id], [sch_id], [db_id], [srv_id], [so_column_name]), 
    CONSTRAINT [AK_source_columns_index] UNIQUE ([srv_id], [db_id], [sch_id], [so_id], [so_column_index])
)
