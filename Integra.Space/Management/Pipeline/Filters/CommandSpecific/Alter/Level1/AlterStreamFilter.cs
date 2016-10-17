//-----------------------------------------------------------------------
// <copyright file="AlterStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Database;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterStreamFilter : AlterEntityFilter<Language.AlterStreamNode, Common.StreamOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(Language.AlterStreamNode command, Dictionary<Common.StreamOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            Space.Database.Stream stream = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.StreamName == command.MainCommandObject.Name);
                        
            if (options.ContainsKey(Common.StreamOptionEnum.Query) && !string.IsNullOrWhiteSpace(options[Common.StreamOptionEnum.Query] as string))
            {
                // especifico el assembly.
                string assemblyPath = System.IO.Path.Combine(Environment.CurrentDirectory, "TempQueryAssemblies", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName, command.MainCommandObject.Name + Language.Runtime.SpaceAssemblyBuilder.FILEEXTENSION);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        stream.Assembly = File.ReadAllBytes(assemblyPath);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Can't read the bytes of the assembly of the stream '{0}' at the schema '{1}', database '{2}' and server '{3}'", stream.StreamName, stream.Schema.SchemaName, stream.Schema.Database.DatabaseName, stream.Schema.Database.Server.ServerName), e);
                    }
                }
                else
                {
                    throw new FileNotFoundException(string.Format("The assembly of the stream '{0}' at the schema '{1}', database '{2}' and server '{3}' was not found.", stream.StreamName, stream.Schema.SchemaName, stream.Schema.Database.DatabaseName, stream.Schema.Database.Server.ServerName));
                }
                
                stream.Query = options[Common.StreamOptionEnum.Query].ToString();
            }

            if (options.ContainsKey(Common.StreamOptionEnum.Name))
            {
                stream.StreamName = options[Common.StreamOptionEnum.Name].ToString();
            }

            if (command.Options.ContainsKey(Common.StreamOptionEnum.Status))
            {
                stream.IsActive = (bool)command.Options[Common.StreamOptionEnum.Status];
            }

            databaseContext.SaveChanges();
        }
    }
}
