namespace Xs.Cli.Core.Audit;

public class AuditResult
{
    public bool IsFixed { get; }
    public string Message { get; }

    public AuditResult(
        bool isFixed,
        string message
    )
    {
        IsFixed = isFixed;
        Message = message;
    }
}