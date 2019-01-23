namespace Xs.Execution
{
    public static class Executor
    {
        public static StageExecutor Staged() => new StageExecutor();

        public static BatchExecutor Batch() => new BatchExecutor();
    }
}