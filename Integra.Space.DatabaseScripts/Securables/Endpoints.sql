CREATE TABLE [space].[endpoints]
(
	[ep_id] UNIQUEIDENTIFIER NOT NULL , 
    [ep_name] NVARCHAR(50) NOT NULL, 
    [srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [owner_id] UNIQUEIDENTIFIER NOT NULL,
    [owner_srv_id] UNIQUEIDENTIFIER NOT NULL, 
    [is_active] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [AK_Endpoint_dp_name] UNIQUE ([ep_name]), 
    CONSTRAINT [FK_Endpoint_Server] FOREIGN KEY ([srv_id]) REFERENCES [space].[servers]([srv_id]), 
    PRIMARY KEY ([ep_id], [srv_id]),
    CONSTRAINT [FK_Endpoints_DatabaseUsers] FOREIGN KEY ([owner_srv_id], [owner_id]) REFERENCES [space].[logins]([srv_id], [lg_id]) , 
	CONSTRAINT [CK_Endpoints_server_ownership] CHECK ([owner_srv_id] = [srv_id])
)
