// using System;
// using System.Collections.Generic;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using MongoDB.Driver.Core.Events;

// namespace Xs.Registry.Core
// {
//     public static class DatabaseAccessor
//     {
//         public static IMongoDatabase GetDatabase(
//             string host,
//             int port,
//             string name,
//             string user,
//             string pass,
//             bool logQueries
//         )
//         {
//             var settings = new MongoClientSettings();
//             settings.Credential = MongoCredential.CreateCredential("admin", user, pass);
//             settings.Server = new MongoServerAddress(host, port);
//             if (logQueries)
//                 settings.ClusterConfigurator = cb =>
//                 {
//                     var unloggedCommands = new List<string>() { "isMaster", "buildInfo" };
//                     cb.Subscribe<CommandStartedEvent>(e =>
//                     {
//                         if (unloggedCommands.Contains(e.CommandName))
//                             return;

//                         Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
//                     });
//                 };

//             var client = new MongoClient(settings);

//             return client.GetDatabase(name);
//         }
//     }
// }