CREATE TABLE [space].[logins_by_users] (
    [lg_id]        UNIQUEIDENTIFIER NOT NULL,
    [lg_srv_id]    UNIQUEIDENTIFIER NOT NULL,
    [dbusr_db_id]  UNIQUEIDENTIFIER NOT NULL,
    [dbusr_srv_id] UNIQUEIDENTIFIER NOT NULL,
    [dbusr_id]     UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_logins_by_users] PRIMARY KEY CLUSTERED ([lg_id] ASC, [lg_srv_id] ASC, [dbusr_db_id] ASC, [dbusr_srv_id] ASC),
    CONSTRAINT [FK_LoginsByUsers_DatabaseUsers] FOREIGN KEY ([dbusr_id], [dbusr_srv_id], [dbusr_db_id]) REFERENCES [space].[database_users] ([dbusr_id], [srv_id], [db_id]),
    CONSTRAINT [FK_LoginsByUsers_Logins] FOREIGN KEY ([lg_srv_id], [lg_id]) REFERENCES [space].[logins] ([srv_id], [lg_id])
);


