namespace Integra.Space.UnitTests
{
    using System.Threading.Tasks.Dataflow;

    internal class BufferBlockForTest
    {
        private static BufferBlock<EventBase> bufferBlock1;

        public static BufferBlock<EventBase> BufferBlock1
        {
            get
            {
                if(bufferBlock1 == null)
                {
                    bufferBlock1 = new BufferBlock<EventBase>();
                }

                return bufferBlock1;
            }
        }
    }
}
