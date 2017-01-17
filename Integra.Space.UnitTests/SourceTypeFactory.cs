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
                return typeof(Database.ServerView);
            }
            else if (source.Name.Equals("endpoints", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.EndpointView);
            }
            else if (source.Name.Equals("logins", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.LoginView);
            }
            else if (source.Name.Equals("serverroles", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.ServerRoleView);
            }
            else if (source.Name.Equals("databases", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.DatabaseView);
            }
            else if (source.Name.Equals("users", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.UserView);
            }
            else if (source.Name.Equals("databaseroles", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.DatabaseRoleView);
            }
            else if (source.Name.Equals("schemas", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.SchemaView);
            }
            else if (source.Name.Equals("sources", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.SourceView);
            }
            else if (source.Name.Equals("sourcecolumns", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.SourceColumnView);
            }
            else if (source.Name.Equals("streams", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.StreamView);
            }
            else if (source.Name.Equals("streamcolumns", StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(Database.StreamColumnView);
            }
            else
            {
                return typeof(TestObject1);
            }
        }
    }
}
