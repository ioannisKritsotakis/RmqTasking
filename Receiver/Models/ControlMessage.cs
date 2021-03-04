using System;
using System.Collections.Generic;
using System.Text;

namespace Receiver
{
    public class ControlMessage
    {
        public static ControlMessage CreateInstance(string taskType, ControlMessageType type = ControlMessageType.TASK_EXECUTIONER_FAILED)
        {
            return new ControlMessage();
        }

        public ControlMessageType Type;
        public string TaskType;
    }

    public enum ControlMessageType
    {
        TASK_EXECUTIONER_FAILED = 1
    }
}
