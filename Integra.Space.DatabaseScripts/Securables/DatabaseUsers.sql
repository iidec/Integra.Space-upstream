CREATE TABLE [space].[database_users]
(
	[dbusr_id] UNIQUEIDENTIFIER NOT NULL , 
    [dbusr_name] NVARCHAR(50) NOT NULL, 
    [db_id] UNIQUEIDENTIFIER NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [AK_DatabaseUsers_dbu_name] UNIQUE ([dbusr_name]), 
    CONSTRAINT [FK_DatabaseUsers_Databases] FOREIGN KEY ([srv_id], [db_id]) REFERENCES [space].[databases]([srv_id], [db_id]), 
    PRIMARY KEY ([dbusr_id], [srv_id], [db_id])
)
