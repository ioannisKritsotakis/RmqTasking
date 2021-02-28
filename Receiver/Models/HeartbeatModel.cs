namespace Receiver.Models
{
    public class HeartbeatModel
    {
        public long Milliseconds;

        public HeartbeatModel()
        {
            Milliseconds = 0;
        }
        public HeartbeatModel(long milliseconds)
        {
            Milliseconds = milliseconds;
        }

        public bool IsEmpty()
        {
            return Milliseconds == 0;
        }
    }
}
