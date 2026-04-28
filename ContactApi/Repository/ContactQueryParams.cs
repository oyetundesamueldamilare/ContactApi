namespace ContactApi.Repository
{
    public class ContactQueryParams
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ContactId { get; set; }

        // Sorting
        public string? SortBy { get; set; }        // name | email | phoneNumber
        public bool SortDesc { get; set; } = false;

        // Paging
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 50 ? 50 : value < 1 ? 1 : value;
        }
    }
}