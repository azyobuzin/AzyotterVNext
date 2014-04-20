namespace System.Threading.Tasks
{
    internal static class TaskExEx
    {
        public static Task RunLong(Action action)
        {
            return Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
        }
    }
}
