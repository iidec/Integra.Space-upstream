//-----------------------------------------------------------------------
// <copyright file="PipelineExecutorFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;

    /// <summary>
    /// Pipeline executor filter class.
    /// </summary>
    internal class PipelineExecutorFilter : FirstLevelCommandFilter
    {
        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            PipelineExecutor cpe = null;

            foreach (CommandPipelineNode commandNode in context.Commands)
            {
                if (commandNode.Pipeline == null)
                {
                    continue;
                }

                cpe = new PipelineExecutor(commandNode.Pipeline);
                cpe.Execute(new PipelineContext(((Language.CompiledCommand)commandNode.Command).CommandText, commandNode.Command, context.Login, context.Kernel));

                /*System.Data.Common.DbTransaction tran = context.Kernel.Get<SpaceDbContext>().Database.Connection.BeginTransaction();
                
                DbContextTransaction tran = context.Kernel.Get<SpaceDbContext>().Database.CurrentTransaction;

                if (tran == null)
                {
                    tran = context.Kernel.Get<SpaceDbContext>().Database.BeginTransaction();
                }

                using (tran)
                {
                    try
                    {
                        cpe.Execute(new PipelineContext(((Language.CompiledCommand)commandNode.Command).CommandText, commandNode.Command, context.Login, context.Kernel));
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw new Exception(string.Format("Error executing the command:\n{0}", ((Language.CompiledCommand)commandNode.Command).CommandText), e);
                    }
                }*/
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(FirstLevelPipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
