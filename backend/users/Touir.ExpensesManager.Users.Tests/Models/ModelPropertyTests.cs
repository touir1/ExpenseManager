using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Tests.Models
{
    public class ModelPropertyTests
    {
        [Fact]
        public void User_NavigationPropertySetters()
        {
            var creator = new User { Id = 2, FirstName = "C", LastName = "D", Email = "c@d.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };
            var user = new User { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            user.CreatedById = 2;
            user.CreatedBy = creator;
            user.LastUpdatedById = 2;
            user.LastUpdatedBy = creator;
            user.Authentication = auth;

            Assert.Equal(2, user.CreatedById);
            Assert.Same(creator, user.CreatedBy);
            Assert.Equal(2, user.LastUpdatedById);
            Assert.Same(creator, user.LastUpdatedBy);
            Assert.Same(auth, user.Authentication);
        }

        [Fact]
        public void UserDto_NavigationPropertySetters()
        {
            var createdBy = new UserDto { Id = 2 };
            var lastUpdatedBy = new UserDto { Id = 3 };
            var dto = new UserDto();

            dto.CreatedBy = createdBy;
            dto.LastUpdatedBy = lastUpdatedBy;

            Assert.Same(createdBy, dto.CreatedBy);
            Assert.Same(lastUpdatedBy, dto.LastUpdatedBy);
        }

        [Fact]
        public void UserRole_CreatedBySetterWorks()
        {
            var creator = new User { Id = 5, FirstName = "E", LastName = "F", Email = "e@f.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRole = new UserRole { UserId = 1, RoleId = 1, CreatedAt = DateTime.UtcNow };

            userRole.CreatedBy = creator;

            Assert.Same(creator, userRole.CreatedBy);
        }

        [Fact]
        public void RefreshToken_IdSetterWorks()
        {
            var token = new RefreshToken();
            token.Id = 42;
            Assert.Equal(42, token.Id);
        }
    }
}
