namespace KvBot.DataAccess.Contract
{
    public interface IPredefinedCommandQuery
    {
        string Execute(string key);
    }
}
