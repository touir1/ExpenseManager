namespace Touir.ExpensesManager.Expenses.Infrastructure.Options
{
    public class ReceiptStorageOptions
    {
        public string Endpoint { get; set; } = "127.0.0.1:9001";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = "expenses-manager-expenses-artifacts";
    }
}
