namespace Server.Host.Tools;

public interface ISecurityManager
{
    string Hash(string data);
}