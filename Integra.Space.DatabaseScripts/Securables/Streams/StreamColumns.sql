CREATE TABLE [space].[stream_columns]
(
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
	[st_id] UNIQUEIDENTIFIER NOT NULL , 
    [st_column_name] VARCHAR(50) NOT NULL, 
    [st_column_type] VARCHAR(2000) NOT NULL, 
    [st_culumn_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_source_columns_streams] FOREIGN KEY ([st_id], [sch_id], [db_id], [srv_id]) REFERENCES [space].[streams]([st_id], [sch_id], [db_id], [srv_id]),     
    CONSTRAINT [PK_stream_columns] PRIMARY KEY ([st_culumn_id]), 
    CONSTRAINT [AK_stream_columns] UNIQUE ([srv_id], [db_id], [sch_id], [st_id], [st_column_name])
)
