namespace Xs.Cli.Core.Audit
{
    public class AuditResult
    {
        public bool IsFixed { get; }

        public string Message { get; }

        internal AuditResult(
            bool isFixed,
            string message
        )
        {
            IsFixed = isFixed;
            Message = message;
        }
    }
}