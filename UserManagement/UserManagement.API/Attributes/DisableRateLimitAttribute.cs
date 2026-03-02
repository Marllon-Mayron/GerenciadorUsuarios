namespace UserManagement.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisableRateLimitAttribute : Attribute
    {
    }
}