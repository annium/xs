namespace Server.Main.Tools;

public interface ISecurityManager
{
    string Hash(string data);
}