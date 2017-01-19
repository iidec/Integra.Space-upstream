CREATE TABLE [space].[streams]
(
	[st_id] UNIQUEIDENTIFIER NOT NULL , 
    [st_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_query] TEXT NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [st_assembly] VARBINARY(MAX) NOT NULL, 
    [is_active] BIT NOT NULL DEFAULT 1, 
    PRIMARY KEY ([st_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [AK_Streams_so_name] UNIQUE ([st_name]), 
    CONSTRAINT [FK_Streams_Schemas] FOREIGN KEY ([srv_id], [db_id], [sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id]) ,
    CONSTRAINT [FK_Streams_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]) , 
    CONSTRAINT [CK_Streams_db_ownership] CHECK ([owner_db_id] = [db_id]),
	CONSTRAINT [CK_Streams_server_ownership] CHECK ([owner_srv_id] = [srv_id])
)
