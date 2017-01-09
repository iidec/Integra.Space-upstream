CREATE TABLE [space].[logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL ,
    [lg_name] NVARCHAR(50) NOT NULL, 
    [lg_password] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [default_db_id] UNIQUEIDENTIFIER NOT NULL, 
    [default_db_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [is_active] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [AK_Login_lg_name] UNIQUE ([lg_name]), 
    CONSTRAINT [FK_Login_Server]  FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 	
    PRIMARY KEY ([srv_id], [lg_id]), 
    CONSTRAINT [FK_logins_databases] FOREIGN KEY ([default_db_srv_id], [default_db_id]) REFERENCES [space].[databases]([srv_id], [db_id])
)
