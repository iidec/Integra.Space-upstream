using System;
using System.Threading.Tasks.Dataflow;

namespace Integra.Space.UnitTests
{
    internal class ConcreteSource : ISource
    {
        public void Enqueue(EventBase @event)
        {
            BufferBlockForTest.BufferBlock1.Post<EventBase>(@event);
        }
    }
}
