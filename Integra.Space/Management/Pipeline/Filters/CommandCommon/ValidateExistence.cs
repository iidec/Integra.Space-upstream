//-----------------------------------------------------------------------
// <copyright file="ValidateExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Class to validate whether the existence of the specified space objects applies to the command.
    /// </summary>
    internal class ValidateExistence : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            foreach (Tuple<SystemObjectEnum, string> @object in context.Command.GetUsedSpaceObjects())
            {
                MethodInfo method2 = typeof(ResolutionExtensions).GetMethods()
                    .First(m => m.Name == "Get" && m.GetParameters()[1].ParameterType.Equals(typeof(Ninject.Parameters.IParameter).MakeArrayType()))
                    .MakeGenericMethod(new Type[] { typeof(IRepository<>).MakeGenericType(Type.GetType("Integra.Space.Models." + @object.Item1.ToString())) });

                var repo = method2.Invoke(null, new object[] { context.Kernel, new Ninject.Parameters.IParameter[] { } });
                SystemObject systemObject = (SystemObject)repo.GetType().GetMethod("FindByName").Invoke(repo, new object[] { @object.Item2 });

                if (context.Command.Action == ActionCommandEnum.Create)
                {
                    if (systemObject != null)
                    {
                        throw new Exception(string.Format("The {0} '{1}' already exist.", @object.Item1, @object.Item2));
                    }
                }
                else
                {
                    if (systemObject == null)
                    {
                        throw new Exception(string.Format("The {0} '{1}' does not exist.", @object.Item1, @object.Item2));
                    }
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
