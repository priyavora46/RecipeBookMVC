using RecipeBookMVC.Models;
using System.Data.Entity;

public class RecipeDBContext : DbContext
{
    public RecipeDBContext() : base("RecipeBookDB_New")  // ✅ same as in SQL Server Object Explorer
    {
    }

    public DbSet<LikedRecipe> LikedRecipes { get; set; }
}
