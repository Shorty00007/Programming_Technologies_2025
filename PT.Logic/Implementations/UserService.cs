using PT.Data.Interfaces;
using PT.Logic.Interfaces;

namespace PT.Logic.Implementations
{
    public class UserService : IUserService
    {
        private readonly IDataLayer _dataLayer;

        public UserService(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public List<string> GetUserNamesUppercase()
        {
            var users = _dataLayer.GetUsers();
            return users.Select(u => u.Name.ToUpper()).ToList();
        }
    }
}