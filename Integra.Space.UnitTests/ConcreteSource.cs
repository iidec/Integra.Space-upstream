//-----------------------------------------------------------------------
// <copyright file="ConcreteSource.cs" company="ARITEC">
// Copyright (c) ARITEC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.UnitTests
{
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Concrete source class.
    /// </summary>
    internal class ConcreteSource : ISource
    {
        /// <summary>
        /// Simulates send a event to a source.
        /// </summary>
        /// <param name="event">Event to send.</param>
        public void Enqueue(EventBase @event)
        {
            BufferBlockForTest.BufferBlock1.Post<EventBase>(@event);
        }
    }
}
