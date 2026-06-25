using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class FamilyControllerTests
    {
        // JWT sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static FamilyController CreateController(
            IFamilyService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new FamilyController(service ?? Mock.Of<IFamilyService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static FamilyDto MakeDto(int id = 1, bool isDefault = false) => new()
        {
            Id = id,
            Name = isDefault ? "Default" : $"Family {id}",
            IsDefault = isDefault,
            IsArchived = false,
            UserRole = "Head",
            CreatedAt = DateTime.UtcNow
        };

        private static FamilyDetailDto MakeDetailDto(int id = 1) => new()
        {
            Id = id,
            Name = $"Family {id}",
            IsDefault = false,
            IsArchived = false,
            UserRole = "Head",
            CreatedAt = DateTime.UtcNow,
            Members = []
        };

        // ── GetByUserAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUser_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetByUserAsync();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetByUser_Returns200_WithFamilyList()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByUserAsync(42)).ReturnsAsync([MakeDto(1), MakeDto(2)]);

            var result = await CreateController(service.Object).GetByUserAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<FamilyDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetByIdAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetById_Returns200_WhenFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByIdAsync(1, 42)).ReturnsAsync(MakeDetailDto(1));

            var result = await CreateController(service.Object).GetByIdAsync(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<FamilyDetailDto>(ok.Value);
        }

        [Fact]
        public async Task GetById_Returns404_WhenNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByIdAsync(99, 42)).ThrowsAsync(new FamilyNotFoundException());

            var result = await CreateController(service.Object).GetByIdAsync(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByIdAsync(1, 42)).ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).GetByIdAsync(1);
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).CreateAsync(new CreateFamilyRequest { Name = "X" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Create_Returns201_OnSuccess()
        {
            var dto = MakeDto(5);
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.CreateAsync("My Family", 42)).ReturnsAsync(dto);

            var result = await CreateController(service.Object).CreateAsync(new CreateFamilyRequest { Name = "My Family" });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task Create_Returns400_OnException()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).CreateAsync(new CreateFamilyRequest { Name = "X" });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("SERVER_ERROR", err.Message);
        }

        [Fact]
        public async Task Create_Returns409_OnFamilyConflictException()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyConflictException("FAMILY_NAME_ALREADY_EXISTS"));

            var result = await CreateController(service.Object).CreateAsync(new CreateFamilyRequest { Name = "Duplicate" });

            Assert.IsType<ConflictObjectResult>(result);
        }

        // ── RenameAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task Rename_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).RenameAsync(1, new RenameFamilyRequest { Name = "X" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Rename_Returns200_OnSuccess()
        {
            var dto = MakeDto(1);
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RenameAsync(1, "New Name", 42)).ReturnsAsync(dto);

            var result = await CreateController(service.Object).RenameAsync(1, new RenameFamilyRequest { Name = "New Name" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Rename_Returns404_WhenNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RenameAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException());

            var result = await CreateController(service.Object).RenameAsync(1, new RenameFamilyRequest { Name = "X" });
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Rename_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RenameAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).RenameAsync(1, new RenameFamilyRequest { Name = "X" });
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        // ── ArchiveAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task Archive_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).ArchiveAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Archive_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ArchiveAsync(1, 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).ArchiveAsync(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Archive_Returns403_ForDefaultFamily()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ArchiveAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException("FAMILY_CANNOT_ARCHIVE_DEFAULT"));

            var result = await CreateController(service.Object).ArchiveAsync(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, obj.StatusCode);
            var err = Assert.IsType<ErrorResponse>(obj.Value);
            Assert.Equal("FAMILY_CANNOT_ARCHIVE_DEFAULT", err.Message);
        }

        // ── UnarchiveAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task Unarchive_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.UnarchiveAsync(1, 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).UnarchiveAsync(1);
            Assert.IsType<NoContentResult>(result);
        }

        // ── InviteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task Invite_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).InviteAsync(1, new InviteMemberRequest { Email = "x@x.com" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Invite_Returns200_WithToken()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.InviteAsync(1, "x@x.com", 42)).ReturnsAsync("some-token");

            var result = await CreateController(service.Object).InviteAsync(1, new InviteMemberRequest { Email = "x@x.com" });
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Invite_Returns409_WhenAlreadyMember()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.InviteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyConflictException());

            var result = await CreateController(service.Object).InviteAsync(1, new InviteMemberRequest { Email = "x@x.com" });
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Invite_Returns403_WhenNotHead()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.InviteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).InviteAsync(1, new InviteMemberRequest { Email = "x@x.com" });
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task Invite_Returns404_WhenUserNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.InviteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException("USER_NOT_FOUND"));

            var result = await CreateController(service.Object).InviteAsync(1, new InviteMemberRequest { Email = "x@x.com" });
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(nf.Value);
            Assert.Equal("USER_NOT_FOUND", err.Message);
        }

        // ── AcceptInviteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AcceptInvite_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).AcceptInviteAsync("tok");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AcceptInvite_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync("tok", 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).AcceptInviteAsync("tok");
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AcceptInvite_Returns400_WhenTokenInvalid()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyInvitationException("FAMILY_INVITATION_INVALID"));

            var result = await CreateController(service.Object).AcceptInviteAsync("bad");
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("FAMILY_INVITATION_INVALID", err.Message);
        }

        [Fact]
        public async Task AcceptInvite_Returns400_WhenExpired()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyInvitationException("FAMILY_INVITATION_EXPIRED"));

            var result = await CreateController(service.Object).AcceptInviteAsync("old");
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("FAMILY_INVITATION_EXPIRED", err.Message);
        }

        [Fact]
        public async Task AcceptInvite_Returns409_WhenAlreadyMember()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyConflictException());

            var result = await CreateController(service.Object).AcceptInviteAsync("tok");
            Assert.IsType<ConflictObjectResult>(result);
        }

        // ── RemoveMemberAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task RemoveMember_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).RemoveMemberAsync(1, 20);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RemoveMember_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RemoveMemberAsync(1, 20, 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).RemoveMemberAsync(1, 20);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveMember_Returns404_WhenNotMember()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException("FAMILY_NOT_MEMBER"));

            var result = await CreateController(service.Object).RemoveMemberAsync(1, 99);
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(nf.Value);
            Assert.Equal("FAMILY_NOT_MEMBER", err.Message);
        }

        [Fact]
        public async Task RemoveMember_Returns403_WhenNotHead()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).RemoveMemberAsync(1, 20);
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        // ── LeaveAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task Leave_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).LeaveAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Leave_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.LeaveAsync(1, 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).LeaveAsync(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Leave_Returns403_WhenLastHead()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.LeaveAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException("FAMILY_CANNOT_LEAVE_LAST_HEAD"));

            var result = await CreateController(service.Object).LeaveAsync(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, obj.StatusCode);
            var err = Assert.IsType<ErrorResponse>(obj.Value);
            Assert.Equal("FAMILY_CANNOT_LEAVE_LAST_HEAD", err.Message);
        }

        [Fact]
        public async Task Leave_Returns404_WhenNotMember()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.LeaveAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException());

            var result = await CreateController(service.Object).LeaveAsync(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── ChangeMemberRoleAsync ─────────────────────────────────────────────

        [Fact]
        public async Task ChangeRole_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).ChangeMemberRoleAsync(1, 20, new ChangeMemberRoleRequest { Role = "Member" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ChangeRole_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ChangeRoleAsync(1, 20, It.IsAny<string>(), 42)).Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).ChangeMemberRoleAsync(1, 20, new ChangeMemberRoleRequest { Role = "Member" });
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ChangeRole_Returns403_WhenNotHead()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ChangeRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).ChangeMemberRoleAsync(1, 20, new ChangeMemberRoleRequest { Role = "Head" });
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task ChangeRole_Returns404_WhenTargetNotMember()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ChangeRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException("FAMILY_NOT_MEMBER"));

            var result = await CreateController(service.Object).ChangeMemberRoleAsync(1, 99, new ChangeMemberRoleRequest { Role = "Member" });
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── Exception fallback coverage ───────────────────────────────────────

        [Fact]
        public async Task GetByUser_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByUserAsync(It.IsAny<int>())).ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).GetByUserAsync();
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetById_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).GetByIdAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Rename_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RenameAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).RenameAsync(1, new RenameFamilyRequest { Name = "X" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Archive_Returns404_WhenNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ArchiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new FamilyNotFoundException());
            var result = await CreateController(service.Object).ArchiveAsync(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Archive_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ArchiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).ArchiveAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Unarchive_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).UnarchiveAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Unarchive_Returns404_WhenNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.UnarchiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new FamilyNotFoundException());
            var result = await CreateController(service.Object).UnarchiveAsync(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Unarchive_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.UnarchiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new FamilyForbiddenException());
            var result = await CreateController(service.Object).UnarchiveAsync(1);
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task Unarchive_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.UnarchiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).UnarchiveAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Invite_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.InviteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).InviteAsync(1, new InviteMemberRequest { Email = "x@y.com" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AcceptInvite_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException("FAMILY_CANNOT_INVITE_DEFAULT"));
            var result = await CreateController(service.Object).AcceptInviteAsync("tok");
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task AcceptInvite_Returns404_WhenNotFound()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyNotFoundException("FAMILY_NOT_FOUND"));
            var result = await CreateController(service.Object).AcceptInviteAsync("tok");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AcceptInvite_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.AcceptInviteAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).AcceptInviteAsync("tok");
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RemoveMember_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).RemoveMemberAsync(1, 99);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Leave_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.LeaveAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).LeaveAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ChangeMemberRole_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.ChangeRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).ChangeMemberRoleAsync(1, 99, new ChangeMemberRoleRequest { Role = "Member" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── GetPendingInvitationsAsync ────────────────────────────────────────

        [Fact]
        public async Task GetPendingInvitations_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetPendingInvitationsAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetPendingInvitations_Returns200_WithList()
        {
            var dto = new FamilyPendingInvitationDto
            {
                Token = "tok",
                InviteeEmail = "x@y.com",
                InvitedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6)
            };
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetPendingInvitationsAsync(1, 42))
                .ReturnsAsync([dto]);

            var result = await CreateController(service.Object).GetPendingInvitationsAsync(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<FamilyPendingInvitationDto>>(ok.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetPendingInvitations_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetPendingInvitationsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());
            var result = await CreateController(service.Object).GetPendingInvitationsAsync(1);
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task GetPendingInvitations_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.GetPendingInvitationsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).GetPendingInvitationsAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── RevokeInvitationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task RevokeInvitation_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).RevokeInvitationAsync(1, "tok");
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RevokeInvitation_Returns204_OnSuccess()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RevokeInvitationAsync(1, "tok", 42))
                .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).RevokeInvitationAsync(1, "tok");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RevokeInvitation_Returns403_WhenForbidden()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RevokeInvitationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyForbiddenException());
            var result = await CreateController(service.Object).RevokeInvitationAsync(1, "tok");
            Assert.Equal(403, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task RevokeInvitation_Returns400_WhenInvalidInvitation()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RevokeInvitationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new FamilyInvitationException("FAMILY_INVITATION_INVALID"));
            var result = await CreateController(service.Object).RevokeInvitationAsync(1, "bad-tok");
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RevokeInvitation_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IFamilyService>();
            service.Setup(s => s.RevokeInvitationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).RevokeInvitationAsync(1, "tok");
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
