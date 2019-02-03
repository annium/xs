namespace Xs.Registry.Core.Security
{
    public interface ISecurityManager
    {
        string Hash(string data);
    }
}