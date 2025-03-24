using PT.Logic.Interfaces;
using PT.Logic.Implementations;
using PT.Data.Interfaces;
using PT.Data.Implementations;

namespace PT.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        private IUserService _userService;

        [TestInitialize]
        public void SetUp()
        {
            IDataLayer dataLayer = new DataLayer();
            _userService = new UserService(dataLayer);
        }

        [TestMethod]
        public void GetUserNamesUppercase_ReturnsUppercaseNames()
        {
            List<string> result = _userService.GetUserNamesUppercase();

            CollectionAssert.AreEqual(new List<string> { "ARTUR", "DOMINIK" }, result, "Uppercase user names");
        }
        [TestMethod]
        public void GetUserNamesReversed_ReturnsReversedNames()
        {
            List<string> result = _userService.GetUserNamesReversed();

            CollectionAssert.AreEqual(new List<string> { "rutrA", "kinimoD" }, result, "Reversed user names");
        }
    }
}
