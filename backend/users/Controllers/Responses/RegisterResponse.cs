namespace com.touir.expenses.Users.Controllers.Responses
{
    public class RegisterResponse
    {
        public IEnumerable<string>? Errors { get; set; }
        public bool? HasError { get; set; }
    }
}
