using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using RecipeBookMVC.Models;

namespace RecipeBookMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly RecipeDBContext db = new RecipeDBContext();
        private readonly SpoonacularService _spoonacularService = new SpoonacularService();

        // ---------------------------------------------------------------------
        // 🔹 Simulated User Authentication (for demo)
        // ---------------------------------------------------------------------
        private string GetUserId()
        {
            if (Session["UserId"] == null)
                Session["UserId"] = Guid.NewGuid().ToString(); // Generate a unique session ID
            return Session["UserId"].ToString();
        }

        // ---------------------------------------------------------------------
        // 🏠 HOME / INDEX – Search + Filter
        // ---------------------------------------------------------------------
        public async Task<ActionResult> Index(string search, string diet)
        {
            List<Recipe> recipes = new List<Recipe>();
            string effectiveDiet = string.IsNullOrWhiteSpace(diet) ? null : diet.ToLower();
            string searchTerm = string.IsNullOrWhiteSpace(search) ? null : search;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                recipes = await _spoonacularService.SearchRecipes(searchTerm, effectiveDiet);
                ViewBag.DisplayQuery = $"Search results for '{searchTerm}'";
            }
            else if (!string.IsNullOrWhiteSpace(effectiveDiet))
            {
                string query = effectiveDiet == "non-vegetarian" ? "chicken, beef, fish" : "vegetable, beans, lentil";
                recipes = await _spoonacularService.SearchRecipes(query, effectiveDiet);
                ViewBag.DisplayQuery = $"Trending {effectiveDiet} recipes";
            }
            else
            {
                recipes = await _spoonacularService.SearchRecipes("trending");
                ViewBag.DisplayQuery = "Trending Recipes";
            }

            ViewBag.SearchQuery = search;
            ViewBag.CurrentDiet = string.IsNullOrEmpty(diet) ? "all" : diet;

            return View(recipes);
        }

        // ---------------------------------------------------------------------
        // 🍽️ RECIPES PAGE (Filters)
        // ---------------------------------------------------------------------
        public async Task<ActionResult> Recipes(string diet = "", string cuisine = "", string type = "")
        {
            ViewBag.CurrentDiet = diet;
            ViewBag.Cuisine = cuisine;
            ViewBag.CurrentType = type;

            var recipes = await _spoonacularService.GetRecipesAsync(diet, cuisine, type);
            ViewBag.DisplayQuery = $"Results for {(string.IsNullOrEmpty(diet) ? "All Diets" : diet)}";

            return View(recipes);
        }

        // ---------------------------------------------------------------------
        // 📄 RECIPE DETAILS PAGE
        // ---------------------------------------------------------------------
        public async Task<ActionResult> RecipePage(int id)
        {
            if (id <= 0) return RedirectToAction("Index");

            var recipe = await _spoonacularService.GetRecipeDetails(id);
            if (recipe == null) return View("Error");

            return View(recipe);
        }

        // ---------------------------------------------------------------------
        // ❤️ LIKE RECIPE (AJAX)
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LikeRecipe(LikedRecipe likedRecipe)
        {
            if (likedRecipe == null)
                return Json(new { success = false, message = "No recipe data received." });

            if (likedRecipe.RecipeId <= 0)
                return Json(new { success = false, message = "Invalid Recipe ID." });

            likedRecipe.UserId = GetUserId();
            likedRecipe.DateLiked = DateTime.Now;

            try
            {
                var exists = await db.LikedRecipes
                    .FirstOrDefaultAsync(l => l.UserId == likedRecipe.UserId && l.RecipeId == likedRecipe.RecipeId);

                if (exists != null)
                    return Json(new { success = false, message = "You already liked this recipe." });

                db.LikedRecipes.Add(likedRecipe);
                await db.SaveChangesAsync(); // ✅ ensures it gets stored

                return Json(new { success = true, message = "Recipe liked successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving like: " + ex.Message });
            }
        }

        // ---------------------------------------------------------------------
        // 💾 VIEW LIKED RECIPES
        // ---------------------------------------------------------------------
        public async Task<ActionResult> Liked(string diet)
        {
            string userId = GetUserId();
            var query = db.LikedRecipes.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(diet))
            {
                if (diet.ToLower() == "vegetarian") query = query.Where(l => l.IsVegetarian);
                if (diet.ToLower() == "non-vegetarian") query = query.Where(l => !l.IsVegetarian);
            }

            var liked = await query.OrderByDescending(l => l.DateLiked).ToListAsync();
            ViewBag.CurrentDiet = string.IsNullOrEmpty(diet) ? "all" : diet;
            ViewBag.Title = "Your Liked Recipes";

            return View(liked);
        }

        // ---------------------------------------------------------------------
        // ❌ REMOVE LIKED RECIPE
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLikedRecipe(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid recipe ID." });

            string userId = GetUserId();
            var likedRecipe = await db.LikedRecipes.FirstOrDefaultAsync(l => l.UserId == userId && l.Id == id);

            if (likedRecipe == null)
                return Json(new { success = false, message = "Recipe not found." });

            db.LikedRecipes.Remove(likedRecipe);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Recipe removed successfully!" });
        }

        // ---------------------------------------------------------------------
        // 🧾 ABOUT PAGE
        // ---------------------------------------------------------------------
        public ActionResult About()
        {
            ViewBag.Title = "About Recipe Book";
            ViewBag.Message = "Discover, save, and organize your favorite recipes easily.";
            return View();
        }

        // ---------------------------------------------------------------------
        // 📞 CONTACT PAGE
        // ---------------------------------------------------------------------
        public ActionResult Contact()
        {
            ViewBag.Title = "Contact Us";
            ViewBag.Message = "We’d love to hear your feedback or suggestions!";
            return View();
        }

        // ---------------------------------------------------------------------
        // ♻️ CLEANUP
        // ---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
