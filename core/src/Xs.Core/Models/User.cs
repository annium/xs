namespace Xs.Core.Models
{
    public class User
    {
        public string Id { get; }

        public string Name { get; }

        public string PasswordHash { get; }

        public string Token { get; }

        public User(
            string id,
            string name,
            string passwordHash,
            string token
        ) : this(name, passwordHash, token)
        {
            Id = id;
        }

        public User(
            string name,
            string passwordHash,
            string token
        )
        {
            Name = name;
            PasswordHash = passwordHash;
            Token = token;
        }
    }
}