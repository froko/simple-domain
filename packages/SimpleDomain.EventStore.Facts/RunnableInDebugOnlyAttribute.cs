namespace SimpleDomain;

using Xunit;

public class RunnableInDebugOnlyAttribute : FactAttribute
{
    public RunnableInDebugOnlyAttribute()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            this.Skip = "Only running in debug mode.";
        }
    }
}