using System;

namespace ActiveQueueLibrary
{
    public interface IJob : IPrioritable,IComparable
    {
        string QueueKey { get; set; }
    }
}
