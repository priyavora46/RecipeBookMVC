using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBookMVC.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid Email Format")]
        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [DataType(DataType.MultilineText)]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Message { get; set; }

        public DateTime DateSent { get; set; } = DateTime.Now;
    }
}