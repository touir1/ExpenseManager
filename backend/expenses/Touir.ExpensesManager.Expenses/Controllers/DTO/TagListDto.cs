namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class TagListDto
    {
        public IEnumerable<TagDto> Own { get; set; } = [];
        public IEnumerable<TagDto> Family { get; set; } = [];
    }
}
