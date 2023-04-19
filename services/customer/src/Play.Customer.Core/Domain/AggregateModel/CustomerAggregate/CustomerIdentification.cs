namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    public readonly struct CustomerIdentification
    {
        public CustomerIdentification(string id, string email, string document)
        {
            Id = id;
            Email = email;
            Document = document;
        }

        public string Id { get; }
        public string Email { get; }
        public string Document { get; }

        public string Value
        {
            get
            {
                if (!string.IsNullOrEmpty(Id)
                    && !string.IsNullOrEmpty(Email)
                    && !string.IsNullOrEmpty(Document))
                    return $"{nameof(Customer).ToLowerInvariant()}-{Id}-{Email}-{Document}";

                return string.Empty;
            }
        }
    }
}