﻿using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndustryX.WebUI.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserAddressService _addressService;

        public ProfileController(UserManager<ApplicationUser> userManager, IUserAddressService addressService)
        {
            _userManager = userManager;
            _addressService = addressService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
                ViewData["CurrentUser"] = user;

            await next();
        }

        // -------------------- Profile Info --------------------
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(new ProfileViewModel
            {
                Firstname = user.Firstname,
                Surname = user.Surname,
                Email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Firstname = model.Firstname;
            user.Surname = model.Surname;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ShowAlert("Profile Updated", "Your profile has been updated successfully.", "success");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ShowAlert("Error", error.Description, "danger");

            return View(model);
        }

        
        // -------------------- Change Password --------------------
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ShowAlert("Success", "Password changed successfully.", "success");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ShowAlert("Error", error.Description, "danger");

            return View(model);
        }

        
        // -------------------- Addresses --------------------
        public async Task<IActionResult> Addresses()
        {
            var userId = _userManager.GetUserId(User);
            var addresses = await _addressService.GetUserAddressesAsync(userId);
            return View(addresses);
        }

        public IActionResult CreateAddress() => View(new UserAddressViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateAddress(UserAddressViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            var address = new UserAddress
            {
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Title = model.Title,
                Country = model.Country,
                City = model.City,
                District = model.District,
                FullAddress = model.FullAddress
            };

            var result = await _addressService.CreateAsync(address);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                return View(model);
            }

            ShowAlert("Success", "Address added successfully.", "success");
            return RedirectToAction("Addresses");
        }

        public async Task<IActionResult> EditAddress(int id)
        {
            var userId = _userManager.GetUserId(User);
            var address = await _addressService.GetByIdAsync(id, userId);
            if (address == null) return NotFound();

            return View(new UserAddressViewModel
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Title = address.Title,
                Country = address.Country,
                City = address.City,
                District = address.District,
                FullAddress = address.FullAddress
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress(UserAddressViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            var entity = new UserAddress
            {
                Id = model.Id,
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Title = model.Title,
                Country = model.Country,
                City = model.City,
                District = model.District,
                FullAddress = model.FullAddress
            };

            var result = await _addressService.UpdateAsync(entity, userId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                return View(model);
            }

            ShowAlert("Success", "Address updated successfully.", "success");
            return RedirectToAction("Addresses");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _addressService.DeleteAsync(id, userId);

            if (result)
                ShowAlert("Deleted", "Address has been deleted.", "info");
            else
                ShowAlert("Error", "Address could not be deleted.", "danger");

            return RedirectToAction("Addresses");
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _addressService.SetAsDefaultAsync(id, userId);

            if (result)
                ShowAlert("Success", "Default address updated.", "success");
            else
                ShowAlert("Error", "Failed to set default address.", "danger");

            return RedirectToAction("Addresses");
        }
    }
}
