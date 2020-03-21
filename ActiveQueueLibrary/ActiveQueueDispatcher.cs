using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveQueueLibrary
{
    public class ActiveQueueDispatcher : IDisposable
    {

        public delegate void OnJobDelegate(IJob element);
        public event OnJobDelegate OnJobAdded;
        public event OnJobDelegate OnJobProcessed;
        public event OnJobDelegate OnJobPreProcessed;

        public delegate void OnSingleActiveQueueDelegate(string queueKey);
        public event OnSingleActiveQueueDelegate OnQueueDepleted;

        public delegate void OnAllActiveQueueDelegate();
        public event OnAllActiveQueueDelegate OnAllQueuesDepleted;

        //OnPause,OnAbort,OnResume
        public event OnAllActiveQueueDelegate OnStart;
        public event OnAllActiveQueueDelegate OnStarted;
        public event OnAllActiveQueueDelegate OnPause;
        public event OnAllActiveQueueDelegate OnPaused;
        public event OnAllActiveQueueDelegate OnAbort;
        public event OnAllActiveQueueDelegate OnAborted;
        public event OnAllActiveQueueDelegate OnResume;
        public event OnAllActiveQueueDelegate OnResumed;

        private List<Task> tasks = new List<Task>();
        private readonly IJobProcesser _processer;
        private CancellationTokenSource _cancellationTokenSource;
        private Dictionary<string, BlockingCollection<IJob>> _queueDictionary = new Dictionary<string, BlockingCollection<IJob>>();

        private int _jobNumber;
        private readonly object _lockJobNumber = new object();
        private readonly object _lockDispatcherStatus = new object();
        private readonly object _lockActiveDispatcher = new object();


        private DispatcherStatus _dispatcherStatus;
        public DispatcherStatus DispatcherStatus
        {
            get
            {
                DispatcherStatus dispatcherStatusCopy;
                //internal lock to avoid change the status when someone take the value from outside.
                lock (_lockDispatcherStatus) dispatcherStatusCopy = _dispatcherStatus;
                return dispatcherStatusCopy;
            }
        }

        public int JobNumber
        {
            get
            {
                int jobNumberCopy;
                lock (_lockJobNumber) jobNumberCopy = _jobNumber;
                return jobNumberCopy;
            }
        }

        /// <summary>
        /// Create the dispatcher with the passed IJobProcesser
        /// </summary>
        /// <param name="processer"></param>
        public ActiveQueueDispatcher(IJobProcesser processer)
        {
            _processer = processer;
            _cancellationTokenSource = new CancellationTokenSource();
            _dispatcherStatus = DispatcherStatus.Running;
        }


        /// <summary>
        /// Create an active queue to dispatch jobs. The queue need to implement the IProducerConsumerCollection<IJob>
        /// for the dispatcher to work correctly. The queues starts if the dispatcher is running.
        /// </summary>
        /// <param name="queueKey">A string that represent the queue Identifier</param>
        /// <param name="queue">The IProducerConsumerCollection<IJob> queue concrete implementation</param>
        public void CreateActiveQueue(string queueKey, IProducerConsumerCollection<IJob> queue)
        {
            BlockingCollection<IJob> activeQueue = new BlockingCollection<IJob>(queue);
            _queueDictionary.Add(queueKey, activeQueue);

            if (DispatcherStatus.Equals(DispatcherStatus.Running))
                lock (_lockActiveDispatcher) CreateActiveQueueTask(queueKey, activeQueue);
        }




        /// <summary>
        /// Used to create the task that will work on a specific queue
        /// </summary>
        /// <param name="queueKey">A string that represent the queue Identifier</param>
        /// <param name="activeQueue">The BlockingCollection<IJob> that encapsulate a queue</param>
        private void CreateActiveQueueTask(string queueKey, BlockingCollection<IJob> activeQueue)
        {
            tasks.Add(Task.Factory.StartNew(() =>
            {
                bool takeNext = true;
                while (takeNext)
                {
                    try
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        IJob elem = activeQueue.Take(_cancellationTokenSource.Token);
                        DecrementJobNumber();

                        if (_processer != null)
                        {
                            PreProcess(elem);
                            _processer.Process(elem);
                            PostProcess(queueKey, activeQueue, elem);

                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        takeNext = false;
                    }
                }
            }, _cancellationTokenSource.Token));
        }

        /// <summary>
        /// Add an element to a working queue. The element implement IJob to let the dispatcher extract the QueueKey property
        /// </summary>
        /// <param name="elem">The IJob that will be dispatched</param>
        public void Add(IJob elem)
        {

            if (!_queueDictionary.ContainsKey(elem.QueueKey)) return;

            Task.Factory.StartNew(() =>
            {
                if (_queueDictionary[elem.QueueKey].TryAdd(elem))
                {//add the cancellation token?
                    IncrementJobNumber();
                    if (OnJobAdded != null) OnJobAdded(elem);
                }
            });
        }

        /// <summary>
        /// Pause all queues stopping all tasks. If a task is working on and IJob this will be finished before cancelling.
        /// </summary>
        public void Pause()
        {
            if (OnPause != null) OnPause();
            PauseAllQueues();
            if (OnPaused != null) OnPaused();
        }

        /// <summary>
        /// Resume all queues recreating the worker tasks.
        /// </summary>
        public void Resume()
        {
            if (OnResume != null) OnResume();
            ReactivateAllQueues();
            if (OnResumed != null) OnResumed();

        }


        /// <summary>
        /// Stop all tasks. In the future will also delete all elements in the queues
        /// </summary>
        public void Abort()
        {
            if (OnAbort != null) OnAbort();
            //qui deve chiamare pause e svuotare o rimuovere le code
            StopTasks();
            _dispatcherStatus = DispatcherStatus.Stopped;
            //delete queue or queue content
            if (OnAborted != null) OnAborted();
        }

        private void StopTasks()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                if (tasks.Count > 0)
                    Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e)
            {
                foreach (var v in e.InnerExceptions) Console.WriteLine("msg: " + v.Message);
            }
            finally
            {
                tasks.Clear();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void IncrementJobNumber()
        {
            lock (_lockJobNumber)
            {
                _jobNumber++;
            }
        }

        private void DecrementJobNumber()
        {
            lock (_lockJobNumber)
            {
                _jobNumber--;
            }
        }

        private void PreProcess(IJob elem)
        {
            if (OnJobPreProcessed != null) OnJobPreProcessed(elem);
        }

        private void PostProcess(string queueKey, BlockingCollection<IJob> activeQueue, IJob elem)
        {
            if (OnJobProcessed != null) OnJobProcessed(elem);
            if (OnQueueDepleted != null && activeQueue.Count == 0) OnQueueDepleted(queueKey);
            if (OnAllQueuesDepleted != null && JobNumber == 0) OnAllQueuesDepleted();
        }

        private void ReactivateAllQueues()
        {
            lock (_lockActiveDispatcher)
            {
                foreach (var activeQueue in _queueDictionary)
                    CreateActiveQueueTask(activeQueue.Key, activeQueue.Value);

                _dispatcherStatus = DispatcherStatus.Running;
            }
        }


        private void PauseAllQueues()
        {
            lock (_lockActiveDispatcher)
            {
                StopTasks();
                _dispatcherStatus = DispatcherStatus.Paused;
            }
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources  
                OnJobAdded = null;
                OnJobProcessed = null;
                OnJobPreProcessed = null;

                OnQueueDepleted = null;
                OnAllQueuesDepleted = null;

                OnPause = null;
                OnPaused = null;
                OnAbort = null;
                OnAborted = null;
                OnResume = null;
                OnResumed = null;

                _cancellationTokenSource.Dispose();
                // free native resources if there are any.  

            }
        }
    }



}