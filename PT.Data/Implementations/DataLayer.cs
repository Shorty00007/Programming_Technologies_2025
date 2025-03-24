using PT.Data.Interfaces;
using PT.Data.Models;

namespace PT.Data.Implementations
{
    public class DataLayer : IDataLayer
    {
        public List<User> GetUsers()
        {
            return new List<User>
            {
                new User { Id = 1, Name = "Artur" },
                new User { Id = 2, Name = "Dominik" }
            };
        }
    }
}
