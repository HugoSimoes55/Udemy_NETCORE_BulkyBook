using BulkyBookRazor.Data;
using BulkyBookRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyBookRazor.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public List<Category> Categories { get; set; }

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public void OnGet()
    {
        Categories = _db.Category.ToList();
    }
}
