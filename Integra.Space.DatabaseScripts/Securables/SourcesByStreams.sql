CREATE TABLE [space].[sources_by_streams]
(    
	[src_id] UNIQUEIDENTIFIER NOT NULL, 
    [src_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [src_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [src_srv_id] UNIQUEIDENTIFIER NOT NULL,
	[st_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [is_input_source] BIT NOT NULL, 
    [relationship_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_sources_by_streams_sources] FOREIGN KEY ([src_id], [src_sch_id], [src_db_id], [src_srv_id]) REFERENCES [space].[sources]([so_id], [sch_id], [db_id], [srv_id]),
	CONSTRAINT [FK_sources_by_streams_streams] FOREIGN KEY ([st_id], [st_sch_id], [st_db_id], [st_srv_id]) REFERENCES [space].[streams]([st_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [PK_sources_by_streams] PRIMARY KEY ([relationship_id])
)
