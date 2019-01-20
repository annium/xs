namespace Xs.Execution
{
    public static class Exec
    {
        public static StageExecutor Staged() => new StageExecutor();

        public static BatchExecutor Batch() => new BatchExecutor();
    }
}