namespace ContactApi.Dto
{  
    public class CountryDto
    {
        // Maintains the nested Name structure for rich data representation
        public Name Name { get; set; } = new();

        public string Region { get; set; } = string.Empty;

        public string Subregion { get; set; } = string.Empty;

        public long Population { get; set; }

        // Added to support more detailed UI requirements
        public string Capital { get; set; } = string.Empty;

        // Added to store the PNG or SVG URL for the country's flag
        public string FlagUrl { get; set; } = string.Empty;
    }



public class Name
{
    public string common { get; set; } = string.Empty;
    public string official { get; set; } = string.Empty;
}

}