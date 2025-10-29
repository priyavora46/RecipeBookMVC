using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace RecipeBookMVC.Models
{
    // =================================================================
    // Spoonacular API Wrapper Models
    // =================================================================

    /// <summary>
    /// This model is essential for deserializing the top-level response 
    /// from the /complexSearch endpoint, which wraps the list of recipes.
    /// </summary>
    public class RecipeSearchResult
    {
        [JsonProperty("results")]
        public List<Recipe> results { get; set; }

        // Optionally: totalResults, offset
    }

    /// <summary>
    /// Model used for API search results.
    /// </summary>
    public class Recipe
    {
        public int id { get; set; }
        public string title { get; set; }
        public string image { get; set; }

        // Used for Veg/Non-Veg filtering
        public bool vegetarian { get; set; }

        // Often null in search results but used in detail view
        public List<AnalyzedInstruction> analyzedInstructions { get; set; }
        public string sourceUrl { get; set; }
    }

    /// <summary>
    /// Model used for detailed recipe information.
    /// </summary>
    public class RecipeDetails : Recipe
    {
        [AllowHtml]
        public string summary { get; set; }

        public List<Ingredient> extendedIngredients { get; set; }
    }

    // =================================================================
    // Sub-Models for Complex Recipe Data
    // =================================================================

    public class AnalyzedInstruction
    {
        public string name { get; set; }
        public List<InstructionStep> steps { get; set; }
    }

    public class InstructionStep
    {
        public int number { get; set; }
        public string step { get; set; }
    }

    public class Ingredient
    {
        public string original { get; set; }
    }
}
