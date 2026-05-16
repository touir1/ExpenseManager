using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<TagListDto> GetVisibleAsync(int userId)
        {
            var ownTask = _tagRepository.GetOwnAsync(userId);
            var familyTask = _tagRepository.GetFamilyAsync(userId);
            await Task.WhenAll(ownTask, familyTask);

            return new TagListDto
            {
                Own = ownTask.Result.Select(MapToDto),
                Family = familyTask.Result.Select(MapToDto)
            };
        }

        public async Task<TagDto> UseTagAsync(string name, int userId)
        {
            var tag = await _tagRepository.GetByNameAsync(name);
            if (tag is null)
            {
                tag = await _tagRepository.AddAsync(new Tag { Name = name });
            }

            await _tagRepository.EnsureUserTagAsync(userId, tag.Id);
            return MapToDto(tag);
        }

        public async Task<bool> RemoveTagAsync(int tagId, int userId)
        {
            return await _tagRepository.RemoveUserTagAsync(userId, tagId);
        }

        private static TagDto MapToDto(Tag tag) => new() { Id = tag.Id, Name = tag.Name };
    }
}
