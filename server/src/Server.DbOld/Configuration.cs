namespace Server.Db;

internal class Configuration
{
    public string Host { get; set; }

    public int Port { get; set; }

    public string Name { get; set; }

    public string User { get; set; }

    public string Pass { get; set; }

    public bool LogQueries { get; set; }
}