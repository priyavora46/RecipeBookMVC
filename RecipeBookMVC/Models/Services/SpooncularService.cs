using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using Newtonsoft.Json;
using RecipeBookMVC.Models;

public class SpoonacularService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.spoonacular.com/";

    public SpoonacularService()
    {
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        _apiKey = WebConfigurationManager.AppSettings["SpoonacularApiKey"];

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("❌ Spoonacular API Key is missing in Web.config.");
    }

    // ---------------------------------------------------------------
    // 1️⃣ Search Recipes (complexSearch)
    // ---------------------------------------------------------------
    public async Task<List<Recipe>> SearchRecipes(string query, string diet = null, string cuisine = null, string type = null)
    {
        string safeQuery = Uri.EscapeDataString(query ?? "");
        string url = $"{BaseUrl}recipes/complexSearch?query={safeQuery}&number=12&addRecipeInformation=true&apiKey={_apiKey}";

        if (!string.IsNullOrWhiteSpace(diet) && diet.Equals("vegetarian", StringComparison.OrdinalIgnoreCase))
            url += "&diet=vegetarian";

        if (!string.IsNullOrWhiteSpace(cuisine))
            url += $"&cuisine={Uri.EscapeDataString(cuisine)}";

        if (!string.IsNullOrWhiteSpace(type))
            url += $"&type={Uri.EscapeDataString(type)}";

        try
        {
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"API Search Failed: {response.StatusCode}");
                return new List<Recipe>();
            }

            string json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RecipeSearchResult>(json);
            return result?.results ?? new List<Recipe>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Search Error: {ex.Message}");
            return new List<Recipe>();
        }
    }

    // ---------------------------------------------------------------
    // 2️⃣ Get Recipe Details
    // ---------------------------------------------------------------
    public async Task<RecipeDetails> GetRecipeDetails(int id)
    {
        string url = $"{BaseUrl}recipes/{id}/information?includeNutrition=false&analyzedInstructions=true&apiKey={_apiKey}";

        try
        {
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"API Detail Failed: {response.StatusCode}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RecipeDetails>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Detail Error: {ex.Message}");
            return null;
        }
    }

    // ---------------------------------------------------------------
    // 3️⃣ Get Random Recipes (for homepage or categories)
    // ---------------------------------------------------------------
    public async Task<List<Recipe>> GetRecipesAsync(string diet = null, string cuisine = null, string type = null)
    {
        // ✅ Build tags (Spoonacular expects comma-separated tags)
        List<string> tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(diet)) tags.Add(diet);
        if (!string.IsNullOrWhiteSpace(cuisine)) tags.Add(cuisine);
        if (!string.IsNullOrWhiteSpace(type)) tags.Add(type);

        string tagParam = tags.Count > 0 ? $"&tags={Uri.EscapeDataString(string.Join(",", tags))}" : "";

        string url = $"{BaseUrl}recipes/random?number=10{tagParam}&apiKey={_apiKey}";

        try
        {
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"API Random Fetch Failed: {response.StatusCode}");
                return new List<Recipe>();
            }

            string json = await response.Content.ReadAsStringAsync();
            RecipeRandomResult result = JsonConvert.DeserializeObject<RecipeRandomResult>(json);
            return result?.Recipes ?? new List<Recipe>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Random Error: {ex.Message}");
            return new List<Recipe>();
        }
    }
}
