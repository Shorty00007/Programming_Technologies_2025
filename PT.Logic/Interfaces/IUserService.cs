namespace PT.Logic.Interfaces
{
    public interface IUserService
    {
        List<string> GetUserNamesUppercase();
        List<string> GetUserNamesReversed();
    }
}