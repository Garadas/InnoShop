public class ProductClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public ProductClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _baseUrl = cfg["ProductService:BaseUrl"] ?? throw new ArgumentNullException("ProductService:BaseUrl");
    }

    public async Task DeactivateUserProductsAsync(Guid userId)
    {
        var response = await _http.PostAsync($"{_baseUrl}/api/UserProducts/{userId}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateUserProductsAsync(Guid userId)
    {
        var response = await _http.PostAsync($"{_baseUrl}/api/UserProducts/{userId}/activate", null);
        response.EnsureSuccessStatusCode();
    }
}
