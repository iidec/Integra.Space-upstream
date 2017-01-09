CREATE TABLE [space].[schemas]
(
	[sch_id] UNIQUEIDENTIFIER NOT NULL , 
    [sch_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NULL, 
    [owner_db_id] UNIQUEIDENTIFIER NULL, 
    [owner_srv_id] UNIQUEIDENTIFIER NULL, 
    PRIMARY KEY ([srv_id], [db_id], [sch_id]), 
    CONSTRAINT [AK_Schemas_sch_name] UNIQUE ([srv_id], [db_id], [sch_name]), 
    CONSTRAINT [FK_Schemas_Databases] FOREIGN KEY ([srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]) ,
    CONSTRAINT [FK_Schemas_DatabaseUsers] FOREIGN KEY ([owner_id], [owner_srv_id], [owner_db_id]) REFERENCES [space].[database_users]([dbusr_id], [srv_id], [db_id]) , 
    CONSTRAINT [CK_Schemas_db_ownership] CHECK ([owner_db_id] = [db_id]),
	CONSTRAINT [CK_Schemas_server_ownership] CHECK ([owner_srv_id] = [srv_id])
)
