using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public string ISBN { get; set; }

    [Required]
    public string Author { get; set; }

    [Required]
    [Range(1, 10000)]
    [DisplayName("List Price")]
    public decimal ListPrice { get; set; }

    [Required]
    [Range(1, 10000)]
    [DisplayName("Price for 1-49")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, 10000)]
    [DisplayName("Price for 50-99")]
    public decimal Price50 { get; set; }

    [Required]
    [Range(1, 10000)]
    [DisplayName("Price for 100+")]
    public decimal Price100 { get; set; }

    [ValidateNever]
    public string ImageURL { get; set; }

    [Required]
    [DisplayName("Category")]
    public int CategoryId { get; set; }

    [ValidateNever]
    public Category Category { get; set; }

    [Required]
    public int CoverTypeId { get; set; }

    [ValidateNever]
    [DisplayName("Type of Cover")]
    public CoverType CoverType { get; set; }
}
