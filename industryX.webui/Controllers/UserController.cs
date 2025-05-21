using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWarehouseService _warehouseService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            IUserService userService,
            IWarehouseService warehouseService,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _warehouseService = warehouseService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            var model = new List<UserListViewModel>();

            var targetRoles = new[] { "WarehouseManager", "Driver", "ProductionManager", "SalesManager" };

            foreach (var user in users)
            {
                var roles = await _userService.GetRolesAsync(user.Id);

                if (roles.Any(r => targetRoles.Contains(r)))
                {
                    model.Add(new UserListViewModel
                    {
                        Id = user.Id,
                        Fullname = $"{user.Firstname} {user.Surname}",
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber ?? "-",
                        Role = roles.FirstOrDefault() ?? "-",
                        WarehouseName = user.Warehouse?.Name ?? "-"
                    });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> CustomerList()
        {
            var users = await _userService.GetAllAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userService.GetRolesAsync(user.Id);

                if (roles.Contains("Customer"))
                {
                    model.Add(new UserListViewModel
                    {
                        Id = user.Id,
                        Fullname = $"{user.Firstname} {user.Surname}",
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber ?? "-",
                        Role = "Customer",
                        WarehouseName = user.Warehouse?.Name ?? "-"
                    });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            return View(await BuildCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(await BuildCreateViewModel(model));

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Firstname = model.Firstname,
                Surname = model.Surname,
                PhoneNumber = model.PhoneNumber,
                WarehouseId = model.WarehouseId
            };

            var (success, error) = await _userService.CreateAsync(user, model.Password, model.Role, model.WarehouseId);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(await BuildCreateViewModel(model));
            }

            ShowAlert("Success", "User created successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                ShowAlert("Error", "User not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Surname = user.Surname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = (await _userService.GetRolesAsync(user.Id)).FirstOrDefault() ?? "",
                WarehouseId = user.WarehouseId
            };

            return View(await BuildEditViewModel(model));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(await BuildEditViewModel(model));

            var user = await _userService.GetByIdAsync(model.Id);
            if (user == null)
            {
                ShowAlert("Error", "User not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            user.Firstname = model.Firstname;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.WarehouseId = model.WarehouseId;

            var (success, error) = await _userService.UpdateAsync(user, model.WarehouseId);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(await BuildEditViewModel(model));
            }

            ShowAlert("Updated", "User updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, error) = await _userService.DeleteAsync(id);
            ShowAlert(success ? "Deleted" : "Error", success ? "User deleted." : error!, success ? "warning" : "danger");
            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private async Task<UserCreateViewModel> BuildCreateViewModel(UserCreateViewModel? model = null)
        {
            var viewModel = model ?? new UserCreateViewModel();
            viewModel.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            viewModel.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
            return viewModel;
        }

        private async Task<UserEditViewModel> BuildEditViewModel(UserEditViewModel model)
        {
            model.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
            return model;
        }
    }
}
