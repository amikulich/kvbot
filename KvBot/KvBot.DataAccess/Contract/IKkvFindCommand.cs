namespace KvBot.DataAccess.Contract
{
    public interface IKkvFindCommand
    {
        string Execute(string key, string userId);
    }
}
