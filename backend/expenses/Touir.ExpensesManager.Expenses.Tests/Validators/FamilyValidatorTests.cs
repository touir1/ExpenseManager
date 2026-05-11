using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Validators;

namespace Touir.ExpensesManager.Expenses.Tests.Validators
{
    public class FamilyValidatorTests
    {
        // ── CreateFamilyRequestValidator ──────────────────────────────────────

        [Fact]
        public async Task CreateFamily_Valid_PassesValidation()
        {
            var result = await new CreateFamilyRequestValidator().ValidateAsync(new CreateFamilyRequest { Name = "My Family" });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task CreateFamily_EmptyName_FailsWithRequired()
        {
            var result = await new CreateFamilyRequestValidator().ValidateAsync(new CreateFamilyRequest { Name = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "FAMILY_NAME_REQUIRED");
        }

        [Fact]
        public async Task CreateFamily_NameTooLong_FailsWithTooLong()
        {
            var result = await new CreateFamilyRequestValidator().ValidateAsync(new CreateFamilyRequest { Name = new string('a', 101) });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "FAMILY_NAME_TOO_LONG");
        }

        [Fact]
        public async Task CreateFamily_NameExactlyMaxLength_PassesValidation()
        {
            var result = await new CreateFamilyRequestValidator().ValidateAsync(new CreateFamilyRequest { Name = new string('a', 100) });
            Assert.True(result.IsValid);
        }

        // ── RenameFamilyRequestValidator ──────────────────────────────────────

        [Fact]
        public async Task RenameFamily_Valid_PassesValidation()
        {
            var result = await new RenameFamilyRequestValidator().ValidateAsync(new RenameFamilyRequest { Name = "New Name" });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task RenameFamily_EmptyName_FailsWithRequired()
        {
            var result = await new RenameFamilyRequestValidator().ValidateAsync(new RenameFamilyRequest { Name = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "FAMILY_NAME_REQUIRED");
        }

        [Fact]
        public async Task RenameFamily_NameTooLong_FailsWithTooLong()
        {
            var result = await new RenameFamilyRequestValidator().ValidateAsync(new RenameFamilyRequest { Name = new string('x', 101) });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "FAMILY_NAME_TOO_LONG");
        }

        // ── InviteMemberRequestValidator ──────────────────────────────────────

        [Fact]
        public async Task InviteMember_Valid_PassesValidation()
        {
            var result = await new InviteMemberRequestValidator().ValidateAsync(new InviteMemberRequest { Email = "user@example.com" });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task InviteMember_EmptyEmail_FailsWithRequired()
        {
            var result = await new InviteMemberRequestValidator().ValidateAsync(new InviteMemberRequest { Email = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "EMAIL_REQUIRED");
        }

        [Fact]
        public async Task InviteMember_InvalidEmail_FailsWithInvalid()
        {
            var result = await new InviteMemberRequestValidator().ValidateAsync(new InviteMemberRequest { Email = "not-an-email" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "EMAIL_INVALID");
        }

        [Fact]
        public async Task InviteMember_EmailTooLong_FailsWithTooLong()
        {
            // 257 chars total: "a@" + 251 'b's + ".com" = 2 + 251 + 4 = 257
            var domain = new string('b', 251);
            var result = await new InviteMemberRequestValidator().ValidateAsync(new InviteMemberRequest { Email = $"a@{domain}.com" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "EMAIL_TOO_LONG");
        }

        // ── ChangeMemberRoleRequestValidator ──────────────────────────────────

        [Theory]
        [InlineData("Head")]
        [InlineData("Member")]
        [InlineData("head")]
        [InlineData("MEMBER")]
        public async Task ChangeMemberRole_ValidRole_PassesValidation(string role)
        {
            var result = await new ChangeMemberRoleRequestValidator().ValidateAsync(new ChangeMemberRoleRequest { Role = role });
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ChangeMemberRole_EmptyRole_FailsWithRequired()
        {
            var result = await new ChangeMemberRoleRequestValidator().ValidateAsync(new ChangeMemberRoleRequest { Role = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "ROLE_REQUIRED");
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("SuperAdmin")]
        [InlineData("Owner")]
        public async Task ChangeMemberRole_InvalidRole_FailsWithInvalid(string role)
        {
            var result = await new ChangeMemberRoleRequestValidator().ValidateAsync(new ChangeMemberRoleRequest { Role = role });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "ROLE_INVALID");
        }
    }
}
