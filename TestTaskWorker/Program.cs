using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiveQueueLibrary;
using ActiveQueueLibrary.SafeCollections;

namespace TestTaskWorker
{
    class Program
    {
        static void Main(string[] args)
        {

            PrioritySafeQueue<ConcreteItemWithPriority> queue = new PrioritySafeQueue<ConcreteItemWithPriority>();
            var bc = new BlockingCollection<ConcreteItemWithPriority>(queue);


            ActiveQueueDispatcher disp= new ActiveQueueDispatcher(new ConcreteProcesser());


            disp.OnPause += () => { Console.WriteLine("Queue pausing"); };
            disp.OnPaused += () => { Console.WriteLine("Queue paused"); };
            disp.OnResume += () => { Console.WriteLine("Queue resuming"); };
            disp.OnResumed += () => { Console.WriteLine("Queue resumed"); };


            disp.CreateActiveQueue("A",new PrioritySafeQueue<IJob>());
            disp.CreateActiveQueue("B", new PrioritySafeQueue<IJob>());

            disp.Pause();


            Task.Factory.StartNew(() =>
            {

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 10));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 4));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 4));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));


            });


            Task.Factory.StartNew(() =>
            {

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 2));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 2));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 2));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 2));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 0));

                disp.Add(new ConcreteItemWithPriority("Elemento1", "A", 5));
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 3));


            });


            
            Task.Factory.StartNew(() =>
            {
                
                disp.Add(new ConcreteItemWithPriority("Elemento1", "B", 7000));

            });

            Console.ReadKey();
            disp.Resume();

            Console.ReadKey();
        }
    }
}
