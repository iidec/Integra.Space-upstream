CREATE TABLE [space].[sources]
(
	[so_id] UNIQUEIDENTIFIER NOT NULL , 
    [so_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY ([so_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [AK_Sources_so_name] UNIQUE ([so_name]), 
    CONSTRAINT [FK_Sources_Schemas] FOREIGN KEY ([srv_id], [db_id], [sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id]),
    CONSTRAINT [FK_Sources_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]), 
    CONSTRAINT [CK_Sources_db_ownership] CHECK ([owner_db_id] = [db_id]),
	CONSTRAINT [CK_Sources_server_ownership] CHECK ([owner_srv_id] = [srv_id])
)
