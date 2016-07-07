//-----------------------------------------------------------------------
// <copyright file="VerifyExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;

    /// <summary>
    /// Class to verify existence of the specified space objects.
    /// </summary>
    internal class VerifyExistence : Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext>
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext input)
        {
            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext e)
        {
            throw new NotImplementedException();
        }
    }
}
