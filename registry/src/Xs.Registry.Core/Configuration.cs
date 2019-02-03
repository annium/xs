namespace Xs.Registry.Core
{
    public class Configuration
    {
        public DatabaseConfiguration Database { get; set; }
    }

    public class DatabaseConfiguration
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Name { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool LogQueries { get; set; }
    }
}