CREATE TABLE [space].[views]
(
	[vw_id] UNIQUEIDENTIFIER NOT NULL , 
    [vw_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [vw_predicate] TEXT NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    PRIMARY KEY ([vw_id], [sch_id], [db_id], [srv_id]), 
    CONSTRAINT [AK_Views_so_name] UNIQUE ([vw_name]), 
    CONSTRAINT [FK_Views_Schemas] FOREIGN KEY ([srv_id], [db_id], [sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id]), 
    CONSTRAINT [FK_Views_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]) , 
    CONSTRAINT [CK_Views_db_ownership] CHECK ([owner_db_id] = [db_id]),
	CONSTRAINT [CK_Views_server_ownership] CHECK ([owner_srv_id] = [srv_id]) 
)
