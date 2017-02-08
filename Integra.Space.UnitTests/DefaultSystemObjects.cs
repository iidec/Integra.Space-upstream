using Integra.Space.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests
{
    public class DefaultSystemObjects
    {
        #region Default system objects
        private static Server defaultServer = new Server
        {
            ServerName = "TestServer"
        };

        private static Database.Database defaultDatabase = new Database.Database
        {
            Server = defaultServer,
            DatabaseId = Guid.NewGuid(),
            DatabaseName = "TestDatabase",
            Login = defaultLogin,
            IsActive = true
        };

        private static Login defaultLogin = new Login
        {
            Server = defaultServer,
            Database = defaultDatabase,
            LoginId = Guid.NewGuid(),
            LoginName = "sa",
            LoginPassword = "unaContraseñaDePrueba123456:!#$%/)/(",
            IsActive = true,
        };

        private static ServerRole sysAdminServerRole = new ServerRole
        {
            Server = defaultServer,
            ServerRoleId = Guid.NewGuid(),
            ServerRoleName = "sysadmin"
        };

        private static ServerRole sysReaderServerRole = new ServerRole
        {
            Server = defaultServer,
            ServerRoleId = Guid.NewGuid(),
            ServerRoleName = "sysreader"
        };

        private static DatabaseUser defaultSaUser = new DatabaseUser
        {
            Database = defaultDatabase,
            DbUsrId = Guid.NewGuid(),
            DbUsrName = "sa",
            Login = defaultLogin,
            DefaultSchema = defaultSchema,
            IsActive = true
        };

        private static DatabaseUser defaultDboUser = new DatabaseUser
        {
            Database = defaultDatabase,
            DbUsrId = Guid.NewGuid(),
            DbUsrName = "dbo",
            Login = defaultLogin,
            DefaultSchema = defaultSchema,
            IsActive = true
        };

        private static Schema defaultSchema = new Schema
        {
            Database = defaultDatabase,
            SchemaId = Guid.NewGuid(),
            SchemaName = "SchemaTest",
            DatabaseUser = defaultDboUser
        };

        private static Source defaultInputSource = new Source
        {
            Schema = defaultSchema,
            SourceId = Guid.NewGuid(),
            SourceName = "InputSourceTest",
            DatabaseUser = defaultDboUser,
            IsActive = true,
            CacheDurability = 60,
            CacheSize = 1000,
            Persistent = true,
            Columns = CreateInputSourceColumn().ToList()
        };

        private static Source defaultOutputSource = new Source
        {
            Schema = defaultSchema,
            SourceId = Guid.NewGuid(),
            SourceName = "OutputSourceTest",
            DatabaseUser = defaultDboUser,
            IsActive = true,
            CacheDurability = 60,
            CacheSize = 1000,
            Persistent = true
        };

        #region Source columns

        private static SourceColumn inputSourceColumnProcessingCode = new SourceColumn { Source = defaultInputSource, ColumnName = "ProcessingCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("86013C60-3E9A-497F-9AC0-010CF8FE84E4"), ColumnIndex = 3, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnLocalTransactionTime = new SourceColumn { Source = defaultInputSource, ColumnName = "LocalTransactionTime", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("701478E9-7F7C-4267-ADAE-0DB19E3AF99E"), ColumnIndex = 7, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnRetrievalReferenceNumber = new SourceColumn { Source = defaultInputSource, ColumnName = "RetrievalReferenceNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("ADCB9471-6995-4F6D-BB51-12FCAF1FF962"), ColumnIndex = 16, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnTrack2Data = new SourceColumn { Source = defaultInputSource, ColumnName = "Track2Data", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("220E993E-2440-4D90-A3F8-22F05352A50A"), ColumnIndex = 15, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnMessageType = new SourceColumn { Source = defaultInputSource, ColumnName = "MessageType", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("2496719B-353F-43FE-81FE-25857652FF06"), ColumnIndex = 1, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnCardAcceptorIdentificationCode = new SourceColumn { Source = defaultInputSource, ColumnName = "CardAcceptorIdentificationCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("0890AA73-7DA0-45DA-A020-3CB343E1E2B0"), ColumnIndex = 18, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnMerchantType = new SourceColumn { Source = defaultInputSource, ColumnName = "MerchantType", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("58270EE5-8860-4445-84DF-4FE33C2B92C3"), ColumnIndex = 10, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnPointOfServiceEntryMode = new SourceColumn { Source = defaultInputSource, ColumnName = "PointOfServiceEntryMode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("7A5D827E-09F4-45A0-927F-5E5DCF20119C"), ColumnIndex = 12, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnTransactionCurrencyCode = new SourceColumn { Source = defaultInputSource, ColumnName = "TransactionCurrencyCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("65BC026D-D942-49B4-9B50-61B727D6432F"), ColumnIndex = 20, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnPointOfServiceConditionCode = new SourceColumn { Source = defaultInputSource, ColumnName = "PointOfServiceConditionCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("93F6CEC9-E1B8-49BC-8229-69C05EEAD6F6"), ColumnIndex = 13, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnLocalTransactionDate = new SourceColumn { Source = defaultInputSource, ColumnName = "LocalTransactionDate", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("50D35C39-C3A7-4372-B197-6B74FDF7EC4E"), ColumnIndex = 8, ColumnLength = 4000 };       
        private static SourceColumn inputSourceColumnCampo105 = new SourceColumn { Source = defaultInputSource, ColumnName = "Campo105", ColumnType = "System.UInt32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("8275572F-C893-456A-9116-73AB0CFCF031"), ColumnIndex = 23, ColumnLength = null };
        private static SourceColumn inputSourceColumnTransactionAmount = new SourceColumn { Source = defaultInputSource, ColumnName = "TransactionAmount", ColumnType = "System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("19DBBA51-97FF-4917-8E05-7D6F6A09B526"), ColumnIndex = 4, ColumnLength = null };
        private static SourceColumn inputSourceColumnSystemTraceAuditNumber = new SourceColumn { Source = defaultInputSource, ColumnName = "SystemTraceAuditNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("380194AB-5EDF-4339-8F58-8484FDACA88B"), ColumnIndex = 6, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnDateTimeTransmission = new SourceColumn { Source = defaultInputSource, ColumnName = "DateTimeTransmission", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("E2DABDCE-FA62-4B99-92FD-98269DF95594"), ColumnIndex = 5, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnPrimaryAccountNumber = new SourceColumn { Source = defaultInputSource, ColumnName = "PrimaryAccountNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("1E15262D-262C-4576-9189-98AA4131DBA5"), ColumnIndex = 2, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnAccountIdentification1 = new SourceColumn { Source = defaultInputSource, ColumnName = "AccountIdentification1", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("A7F43FF0-F129-44B4-8540-B163D5F92F8C"), ColumnIndex = 21, ColumnLength = 4000 };        
        private static SourceColumn inputSourceColumnCampo104 = new SourceColumn { Source = defaultInputSource, ColumnName = "Campo104", ColumnType = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("40907E6B-B3B8-4679-B4E0-C58DC2D6D501"), ColumnIndex = 22, ColumnLength = null };
        private static SourceColumn inputSourceColumnSetElementDate = new SourceColumn { Source = defaultInputSource, ColumnName = "SetElementDate", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("BDE12DB7-D515-4330-9810-C87A2626434C"), ColumnIndex = 9, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnAcquiringInstitutionIdentificationCode = new SourceColumn { Source = defaultInputSource, ColumnName = "AcquiringInstitutionIdentificationCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("76229424-8CA8-4C4C-A40F-E0B55DD109D1"), ColumnIndex = 14, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnCardAcceptorNameLocation = new SourceColumn { Source = defaultInputSource, ColumnName = "CardAcceptorNameLocation", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("6E23722D-AC3A-4F5C-8665-F21EEFD08B41"), ColumnIndex = 19, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnCardAcceptorTerminalIdentification = new SourceColumn { Source = defaultInputSource, ColumnName = "CardAcceptorTerminalIdentification", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("15829BE8-EF5C-41B1-9056-FC570A72EF56"), ColumnIndex = 17, ColumnLength = 4000 };
        private static SourceColumn inputSourceColumnAcquiringInstitutionCountryCode = new SourceColumn { Source = defaultInputSource, ColumnName = "AcquiringInstitutionCountryCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("68B69FB0-CFF7-4747-B8E6-FEE4C7E9040E"), ColumnIndex = 11, ColumnLength = 4000 };

        private static SourceColumn outPutSourceColumnentero = new SourceColumn { Source = defaultOutputSource, ColumnName = "entero", ColumnType = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("9C3C369E-9E59-4C0A-8F5C-C402EEEDBD10"), ColumnIndex = 2, ColumnLength = null };
        private static SourceColumn outPutSourceColumnserverId = new SourceColumn { Source = defaultOutputSource, ColumnName = "serverId", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("1BF59FBD-F3F6-4555-8700-6E0548FB295A"), ColumnIndex = 1, ColumnLength = 4000 };

        #endregion Source columns

        private static Stream defaultStream = new Stream
        {
            Schema = defaultSchema,
            StreamId = Guid.NewGuid(),
            StreamName = "StreamTest",
            DatabaseUser = defaultDboUser,
            Query = string.Format("from {0} select {1} as {2} into {3}", "sys.streams", "ServerId", outPutSourceColumnserverId.ColumnName, defaultOutputSource.SourceName),
            Assembly = new byte[] { },
            IsActive = true
        };

        #region Stream columns

        private static StreamColumn streamColumnserverId = new StreamColumn { Stream = defaultStream, ColumnName = "serverId", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("1BF59FBD-F3F6-4555-8700-6E0548FB295A") };

        #endregion Stream columns

        #endregion Default system objects

        public static IQueryable<SourceColumn> CreateInputSourceColumn()
        {
            return new List<SourceColumn>
            {
                inputSourceColumnProcessingCode
                ,inputSourceColumnLocalTransactionTime
                ,inputSourceColumnRetrievalReferenceNumber
                ,inputSourceColumnTrack2Data
                ,inputSourceColumnMessageType
                ,inputSourceColumnCardAcceptorIdentificationCode
                ,inputSourceColumnMerchantType
                ,inputSourceColumnPointOfServiceEntryMode
                ,inputSourceColumnTransactionCurrencyCode
                ,inputSourceColumnPointOfServiceConditionCode
                ,inputSourceColumnLocalTransactionDate
                ,inputSourceColumnCampo105
                ,inputSourceColumnTransactionAmount
                ,inputSourceColumnSystemTraceAuditNumber
                ,inputSourceColumnDateTimeTransmission
                ,inputSourceColumnPrimaryAccountNumber
                ,inputSourceColumnAccountIdentification1
                ,inputSourceColumnCampo104
                ,inputSourceColumnSetElementDate
                ,inputSourceColumnAcquiringInstitutionIdentificationCode
                ,inputSourceColumnCardAcceptorNameLocation
                ,inputSourceColumnCardAcceptorTerminalIdentification
                ,inputSourceColumnAcquiringInstitutionCountryCode
            }.AsQueryable();
        }

        public static IQueryable<Source> CreateSources()
        {
            return new List<Source>
            {
                defaultInputSource
                , defaultOutputSource
            }.AsQueryable();
        }

        private IQueryable<Server> CreateDefaultServers()
        {
            return new List<Server>
            {
                defaultServer,
            }
            .AsQueryable();
        }

        private IQueryable<Login> CreateDefaultLogins()
        {
            return new List<Login>
            {
                new Login { Server = defaultServer }
            }
            .AsQueryable();
        }
    }
}
