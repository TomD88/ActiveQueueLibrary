using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ActiveQueueLibrary;

namespace TestTaskWorker
{
    public class ConcreteProcesser:IJobProcesser
    {
        public void Process(IJob iJob)
        {
            var item=(ConcreteItemWithPriority)iJob;

            Thread.Sleep(2000);
            Console.WriteLine("Processed {0},{1},{2}",item.Name,item.Priority,item.QueueKey);

        }
    }
}
