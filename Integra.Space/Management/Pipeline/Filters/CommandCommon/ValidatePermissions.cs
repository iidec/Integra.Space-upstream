﻿//-----------------------------------------------------------------------
// <copyright file="ValidatePermissions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class ValidatePermissions : CommandFilter
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