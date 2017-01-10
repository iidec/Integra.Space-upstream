using Integra.Space.Common;
using System;

namespace Integra.Space.UnitTests
{
    internal class SourceTypeFactory : Compiler.ISourceTypeFactory
    {
        public Type GetSourceType(CommandObject source)
        {
            /*
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                Schema schema = source.GetSchema(dbContext, login);
                Source sourceFromDatabase = dbContext.Sources.Single(x => x.ServerId == schema.ServerId
                                        && x.DatabaseId == schema.DatabaseId
                                        && x.SchemaId == schema.SchemaId
                                        && x.SourceName == source.Name);

                IEnumerable<FieldNode> fields = sourceFromDatabase.Columns.OrderBy(x => x.ColumnIndex).Select(x => new FieldNode(x.ColumnName, Type.GetType(x.ColumnType)));
                string typeSignature = string.Format("{0}_{1}_{2}_{3}", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName, source.Name);

                SourceTypeBuilder typeBuilder = new SourceTypeBuilder(this.config.AsmBuilder, typeSignature, typeof(InputBase), fields);
                Type sourceGeneratedType = typeBuilder.CreateNewType();
                sourceGeneratedType = typeof(System.IObservable<>).MakeGenericType(new Type[] { sourceGeneratedType });
            }
            */


            if (source.Name.ToString().Equals("servers", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Server);
            }
            else if (source.Name.Equals("endpoints", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Endpoint);
            }
            else if (source.Name.Equals("logins", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Login);
            }
            else if (source.Name.Equals("serverroles", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.ServerRole);
            }
            else if (source.Name.Equals("databases", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Database);
            }
            else if (source.Name.Equals("users", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.DatabaseUser);
            }
            else if (source.Name.Equals("databaseroles", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.DatabaseRole);
            }
            else if (source.Name.Equals("schemas", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Schema);
            }
            else if (source.Name.Equals("sources", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Source);
            }
            else if (source.Name.Equals("sourcecolumns", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.SourceColumn);
            }
            else if (source.Name.Equals("streams", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.Stream);
            }
            else if (source.Name.Equals("streamcolumns", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.StreamColumn);
            }
            else if (source.Name.Equals("views", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.View);
            }
            else
            {
                return typeof(TestObject1);
            }
        }
    }
}
