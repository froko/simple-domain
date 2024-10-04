namespace SimpleDomain;

using System.Diagnostics;
using Xunit;

public class RunnableInDebugOnlyAttribute : FactAttribute
{
    public RunnableInDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached) this.Skip = "Only running in debug mode.";
    }
}