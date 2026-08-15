using System;

namespace Crai.Domain.Runtime;

public record ExecutionScopeId(Guid Value)
{
    public static ExecutionScopeId New() => new(Guid.NewGuid());
}

public record ExecutionRevisionId(Guid Value)
{
    public static ExecutionRevisionId New() => new(Guid.NewGuid());
}

public record WorkItemId(Guid Value)
{
    public static WorkItemId New() => new(Guid.NewGuid());
}
