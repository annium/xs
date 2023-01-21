using LinqToDB.Data;

namespace Server.Db;

internal interface IContext
{
    DataConnection GetDataConnection();
}