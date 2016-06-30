//-----------------------------------------------------------------------
// <copyright file="Test.cs" company="Integra.Space.Management">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Management
{
    using FirebirdSql.Data.FirebirdClient;

    /// <summary>
    /// Test class.
    /// </summary>
    internal class Test
    {
        /// <summary>
        /// Doc goes here.
        /// </summary>
        public void CreateDatabase()
        {
            FbConnectionStringBuilder csb = new FbConnectionStringBuilder();

            csb.ServerType = FbServerType.Embedded;
            csb.UserID = "SYSDBA";
            csb.Password = "masterkey";
            csb.Dialect = 3;
            csb.Database = @"datadatabase.fdb";
            csb.Charset = "UTF8";
        }
    }
}
