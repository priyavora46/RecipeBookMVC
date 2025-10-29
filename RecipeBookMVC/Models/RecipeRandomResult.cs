using System.Collections.Generic;
using Newtonsoft.Json;

namespace RecipeBookMVC.Models
{
    /// <summary>
    /// Model for handling random recipe results from Spoonacular API.
    /// </summary>
    public class RecipeRandomResult
    {
        [JsonProperty("recipes")]
        public List<Recipe> Recipes { get; set; }
    }
}
