CREATE TABLE [space].[database_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_name] NVARCHAR(50) NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [default_sch_id] UNIQUEIDENTIFIER NOT NULL, 
    [default_sch_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [default_sch_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [dbusr_enabled] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [AK_DatabaseUsers_dbu_name] UNIQUE ([dbusr_name]), 
    CONSTRAINT [FK_DatabaseUsers_Databases] FOREIGN KEY ([srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    PRIMARY KEY ([dbusr_id], [srv_id], [db_id]), 
    CONSTRAINT [FK_database_users_schemas] FOREIGN KEY ([default_sch_srv_id], [default_sch_db_id], [default_sch_id]) REFERENCES [space].[schemas]([srv_id], [db_id], [sch_id])
)
