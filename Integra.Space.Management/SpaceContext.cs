//-----------------------------------------------------------------------
// <copyright file="SpaceContext.cs" company="Integra.Space.Management">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Management
{
    using System.Data.Entity;
    using FirebirdSql.Data.FirebirdClient;

    /// <summary>
    /// Space context class.
    /// </summary>
    internal class SpaceContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpaceContext"/> class.
        /// </summary>
        /// <param name="csb">Firebird connection string builder.</param>
        public SpaceContext(FbConnectionStringBuilder csb) : base(new FbConnection(csb.ToString()), true)
        {
        }
    }
}
