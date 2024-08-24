using System.Collections.Generic;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Core.Audit;

public interface IAuditRule<TProject> : IAuditRule
    where TProject : IProject
{
    IReadOnlyCollection<AuditResult> Execute(IReadOnlyCollection<IProject> projects, TProject project, bool fix);
}

public interface IAuditRule
{
    string Code { get; }
    string Description { get; }
}
