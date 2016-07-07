//-----------------------------------------------------------------------
// <copyright file="ManagementSchedulerFactory.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space
{
    using System;
    using System.Configuration;
    using System.Linq.Expressions;
    using System.Reactive.Concurrency;
    using Integra.Space.Scheduler;

    /// <summary>
    /// Management scheduler factory class.
    /// </summary>
    internal class ManagementSchedulerFactory : IQuerySchedulerFactory
    {
        /// <summary>
        /// Current factory.
        /// </summary>
        private static ManagementSchedulerFactory factory = new ManagementSchedulerFactory();
        
        /// <summary>
        /// Gets the current factory.
        /// </summary>
        public static ManagementSchedulerFactory Current
        {
            get
            {
                return factory;
            }
        }
                
        /// <inheritdoc />
        public Expression GetScheduler()
        {
            int selectedScheduler = int.Parse(ConfigurationManager.AppSettings["QueryScheduler"]);

            switch (selectedScheduler)
            {
                case (int)SchedulerTypeEnum.CurrentThreadScheduler:
                    return Expression.Property(null, typeof(CurrentThreadScheduler).GetProperty("Instance"));
                case (int)SchedulerTypeEnum.DefaultScheduler:
                    return Expression.Property(null, typeof(DefaultScheduler).GetProperty("Instance"));
                case (int)SchedulerTypeEnum.EventLoopScheduler:
                    return Expression.New(typeof(EventLoopScheduler));
                case (int)SchedulerTypeEnum.HistoricalScheduler:
                    return Expression.New(typeof(HistoricalScheduler));
                case (int)SchedulerTypeEnum.ImmediateScheduler:
                    return Expression.Property(null, typeof(ImmediateScheduler).GetProperty("Instance"));
                case (int)SchedulerTypeEnum.NewThreadScheduler:
                    return Expression.Property(null, typeof(NewThreadScheduler).GetProperty("Default"));
                case (int)SchedulerTypeEnum.Scheduler:
                    return Expression.Property(null, typeof(System.Reactive.Concurrency.Scheduler).GetProperty("Default"));
                case (int)SchedulerTypeEnum.TaskPoolScheduler:
                    return Expression.Property(null, typeof(TaskPoolScheduler).GetProperty("Default"));
                case (int)SchedulerTypeEnum.ThreadPoolScheduler:
                    return Expression.Property(null, typeof(ThreadPoolScheduler).GetProperty("Instance"));
                default:
                    throw new Exception("Undefined or not suported scheduler.");
            }
        }
    }
}
