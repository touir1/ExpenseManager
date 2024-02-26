﻿namespace com.touir.expenses.Users.Models
{
    public class RequestAccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public bool IsAuthenticationNeeded { get; set; }
        public Application Application { get; set; }
    }
}
