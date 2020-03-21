using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActiveQueueLibrary;

namespace TestTaskWorker
{
    public class ConcreteItemWithPriority : IJob
    {

        public string Name { get; set; }
        public int Priority { get; set; }


        public ConcreteItemWithPriority(string name, string queueKey, int priority = 0)
        {
            Name = name;
            Priority = priority;
            QueueKey = queueKey;
        }

        public int GetPriority()
        {
            return Priority;
        }


        public string QueueKey { get; set; }
        public int CompareTo(object obj)
        {

            ConcreteItemWithPriority other= (ConcreteItemWithPriority) obj;

            if (this.Priority > other.Priority) return -1;
            else if (this.Priority < other.Priority) return 1;
            else return 0;
        }
    }
}
