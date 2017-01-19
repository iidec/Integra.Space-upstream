CREATE SCHEMA [space]
    AUTHORIZATION [dbo];


GO

GRANT ALTER
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT CONTROL
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT CREATE SEQUENCE
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT DELETE
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT EXECUTE
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT INSERT
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT REFERENCES
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT SELECT
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT TAKE OWNERSHIP
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT UPDATE
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT VIEW CHANGE TRACKING
    ON SCHEMA::[space] TO [space_dev_usr];
GO

GRANT VIEW DEFINITION
    ON SCHEMA::[space] TO [space_dev_usr];
GO

