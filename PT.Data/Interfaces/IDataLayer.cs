using PT.Data.Models;

namespace PT.Data.Interfaces
{
    public interface IDataLayer
    {
        List<User> GetUsers();
    }
}