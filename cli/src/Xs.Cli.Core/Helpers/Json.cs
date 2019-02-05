using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Xs.Cli.Core.Helpers
{
    public static class Json
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static T Read<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static T ReadFile<T>(string file)
        {
            return Read<T>(File.ReadAllText(file));
        }

        public static string Write<T>(T data)
        {
            return JsonConvert.SerializeObject(data, serializerSettings);
        }

        public static void WriteFile<T>(string file, T data)
        {
            File.WriteAllText(file, Write(data));
        }
    }
}