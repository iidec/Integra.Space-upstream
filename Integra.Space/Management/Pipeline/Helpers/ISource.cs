//-----------------------------------------------------------------------
// <copyright file="ISource.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space
{
    /// <summary>
    /// Source interface.
    /// </summary>
    internal interface ISource
    {
        /// <summary>
        /// Enqueue an event in the specified source.
        /// </summary>
        /// <param name="event">Event to enqueue.</param>
        void Enqueue(EventBase @event);
    }
}
