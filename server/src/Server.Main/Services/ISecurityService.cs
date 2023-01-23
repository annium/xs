namespace Server.Main.Tools;

public interface ISecurityService
{
    string Hash(string data);
}