namespace ContactApi.Dto
{
    public class CountryDto
    {

        public Name Name { get; set; } = new Name();
        public string Region { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Subregion { get; set; } = string.Empty;
        public long Population { get; set; }
    }

    public class Name
    {
        public string common { get; set; }       = string.Empty;
        public string official { get; set; }      = string.Empty;
    }
}
