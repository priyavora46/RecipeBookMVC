using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeBookMVC.Models
{
    public class LikedRecipe
    {
        [Key]
        public int Id { get; set; } // Must exist for EF to map the table properly

        public string UserId { get; set; }
        public int RecipeId { get; set; }
        public string RecipeTitle { get; set; }
        public string RecipeImageUri { get; set; }
        public bool IsVegetarian { get; set; }
        public DateTime DateLiked { get; set; } = DateTime.Now;
    }
}
