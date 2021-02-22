using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Receiver
{
    public interface IControlChannel
    {
        ChannelReader<ControlMessage> Reader { get; }
        bool WriteToChannel(ControlMessage item);
    }

    public class ControlChannel : IControlChannel
    {
        private readonly Channel<ControlMessage> _channel;

        public ChannelReader<ControlMessage> Reader => _channel.Reader;

        public ControlChannel()
        {
            _channel = Channel.CreateUnbounded<ControlMessage>();
        }

        public bool WriteToChannel(ControlMessage item)
        {
            return _channel.Writer.TryWrite(item);
        }
    }

   
}
