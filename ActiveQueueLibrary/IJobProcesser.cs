namespace ActiveQueueLibrary
{
    public interface IJobProcesser
    {
        void Process(IJob iJob);
    }
}
