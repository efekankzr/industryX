using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class ApplicationUserFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        public int? WarehouseId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Warehouse")]
        public int? WarehouseId { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Dropdown sources
        public List<string> Roles { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();
    }

    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Warehouse")]
        public int? WarehouseId { get; set; }

        // Dropdown sources
        public List<string> Roles { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();
    }

    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = "-";
        public string Role { get; set; } = "-";
        public string? WarehouseName { get; set; }
    }
}
