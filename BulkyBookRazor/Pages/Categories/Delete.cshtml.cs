using BulkyBookRazor.Data;
using BulkyBookRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyBookRazor.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Category category { get; set; }

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet(int id)
        {
            category = _db.Category.Find(id);
        }

        public async Task<IActionResult> OnPost()
        {
            _db.Category.Remove(category);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
