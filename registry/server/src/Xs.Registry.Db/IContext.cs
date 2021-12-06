using LinqToDB.Data;

namespace Xs.Registry.Db;

internal interface IContext
{
    DataConnection GetDataConnection();
}