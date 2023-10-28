namespace Server.Main.Services;

public interface ISecurityService
{
    string Hash(string data);
}
