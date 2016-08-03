CREATE TABLE [space].[logins]
(
	[lg_id] UNIQUEIDENTIFIER NOT NULL , 
    [lg_name] NVARCHAR(50) NOT NULL, 
    [lg_password] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [AK_Login_lg_name] UNIQUE ([lg_name]), 
    CONSTRAINT [FK_Login_Server]  FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 
    PRIMARY KEY ([srv_id], [lg_id])
)
