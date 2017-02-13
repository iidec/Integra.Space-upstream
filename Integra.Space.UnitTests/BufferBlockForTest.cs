// <copyright file="BufferBlockForTest.cs" company="ARITEC">
// Copyright (c) ARITEC. All rights reserved.
// </copyright>

namespace Integra.Space.UnitTests
{
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Buffer block for test class.
    /// </summary>
    internal class BufferBlockForTest
    {
        /// <summary>
        /// buffer block to simulate a source.
        /// </summary>
        private static BufferBlock<EventBase> bufferBlock1;

        /// <summary>
        /// Gets the buffer block to simulate a source.
        /// </summary>
        public static BufferBlock<EventBase> BufferBlock1
        {
            get
            {
                if (bufferBlock1 == null)
                {
                    bufferBlock1 = new BufferBlock<EventBase>();
                }

                return bufferBlock1;
            }
        }
    }
}
