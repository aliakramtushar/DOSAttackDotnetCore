namespace DOSAttackDotnetCore.Webapi.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; } = String.Empty;
        public decimal Price { get; set; }
        public IFormFile ImageFile { get; set; } // For image upload
    }
}
