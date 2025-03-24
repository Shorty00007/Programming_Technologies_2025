using PT.Data.Interfaces;
using PT.Data.Implementations;
using PT.Data.Models;

namespace PT.Tests
{
    [TestClass]
    public class DataLayerTests
    {
        [TestMethod]
        public void GetUsers_ReturnsCorrectNumberOfUsers()
        {
            IDataLayer dataLayer = new DataLayer();

            List<User> users = dataLayer.GetUsers();

            Assert.AreEqual(2, users.Count, "Expect 2 users");
        }

        [TestMethod]
        public void GetUsers_ReturnsNonEmptyList()
        {
            IDataLayer dataLayer = new DataLayer();

            List<User> users = dataLayer.GetUsers();

            Assert.IsTrue(users.Count > 0, "Users list not empty");
        }
    }
}
