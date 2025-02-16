using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FriendSQLiteMVCVSCode.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FriendSQLiteMVCVSCode.Controllers {
    // [Route("[controller]")]
    [Route("[controller]/{action=Index}/{Id=0}")]
    public class FriendController : Controller {
        private readonly FriendDbContext _context;
        private readonly IWebHostEnvironment environment;
        public FriendController(FriendDbContext friendDbContext, IWebHostEnvironment environment) {
            this._context = friendDbContext;
            this.environment = environment;
        }
        // GET:FriendController
        [HttpGet]
        public async Task<IActionResult> Index() {
            var friendList = await _context.Friends.ToListAsync();
            friendList = friendList.OrderByDescending(x => x.Id).ToList();
            foreach (var friend in friendList) {
                if (friend.PhotoFilename == "" | friend.PhotoFilename == null) {
                    friend.PhotoFilename = "NoData.jpg";
                }
            }
            return View(friendList);
        }
        // GET:FriendController/Create
        [HttpGet]
        public async Task<IActionResult> create() {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(FriendViewModel addFriendViewModel, IFormFile PhotoFile) {
            try {
                string? strPhotoFile = "NoData.jpg";
                if (PhotoFile != null) {
                    string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    strPhotoFile = strDateTime + "_" + PhotoFile.FileName;
                    string? PhotoFullPath = environment.WebRootPath + "/images/" + strPhotoFile;
                    using (var fileStream = new FileStream(PhotoFullPath, FileMode.Create)) {
                        await PhotoFile.CopyToAsync(fileStream);
                    }
                    // string? PhotoFullPath = "images/" + strPhotoFile;
                    // using (var fileStream = new FileStream(PhotoFullPath, FileMode.Create)) {
                    //     await PhotoFile.CopyToAsync(fileStream);
                    // }
                }
                FriendViewModel friendViewModel = new FriendViewModel() {
                    Id = addFriendViewModel.Id,
                    Firstname = addFriendViewModel.Firstname,
                    Lastname = addFriendViewModel.Lastname,
                    Mobile = addFriendViewModel.Mobile,
                    Email = addFriendViewModel.Email,
                    DateOfBirth = addFriendViewModel.DateOfBirth,
                    RegionOfBirth = addFriendViewModel.RegionOfBirth,
                    PhotoFilename = strPhotoFile
                };
                await _context.AddAsync(friendViewModel);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = $"New Friend was Created ({friendViewModel.Firstname} {friendViewModel.Lastname})";
                return RedirectToAction(nameof(Index));
            } catch (Exception ex) {
                TempData["errorMessage"] = $"Message: {ex.Message}{Environment.NewLine}Stack Trace:{Environment.NewLine}{ex.StackTrace}";
                return View();
            }
        }
        // GET:FriendController/Edit/4  // (4 is value of primary key (Id) that we are interested)
        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            try {
                var friend = await _context.Friends.SingleOrDefaultAsync(f => f.Id == id);
                TempData["PhotoFilePath"] = "/images/" + friend.PhotoFilename;
                TempData["DateOfBirth"] = friend.DateOfBirth?.ToString();
                if (friend.RegionOfBirth != null) {
                    TempData["RegionOfBirth"] = friend.RegionOfBirth?.ToString();
                } else {
                    TempData["RegionOfBirth"] = "Unknown";
                }
                return View(friend);
            } catch (Exception ex) {
                TempData["errorMessage"] = ex.Message + "<br/>" + ex.StackTrace;
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(FriendViewModel editFriendViewModel, IFormFile PhotoFile) {
            try {
                var friend = await _context.Friends.SingleOrDefaultAsync(f => f.Id == editFriendViewModel.Id);
                if (friend == null) {
                    TempData["errorMessage"] = $"Friend Not Found with Id {editFriendViewModel.Id}";
                    return View();
                } else {
                    friend.Firstname = editFriendViewModel.Firstname;
                    friend.Lastname = editFriendViewModel.Lastname;
                    friend.Mobile = editFriendViewModel.Mobile;
                    friend.Email = editFriendViewModel.Email;
                    friend.DateOfBirth = editFriendViewModel.DateOfBirth;
                    friend.RegionOfBirth = editFriendViewModel.RegionOfBirth;
                    // for image of object, there are 2 cases 1) changed 2) unchanged
                    if (PhotoFile != null) {
                        string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        string strPhotoFile = strDateTime + "_" + PhotoFile.FileName;
                        string photoFullPath = environment.WebRootPath + "/images/" + strPhotoFile;
                        using (var fileStream = new FileStream(photoFullPath, FileMode.Create)) {
                            await PhotoFile.CopyToAsync(fileStream);
                        }
                        friend.PhotoFilename = strPhotoFile; // use new photo file data
                    } else {
                        friend.PhotoFilename = editFriendViewModel.PhotoFilename; // use existing image file data
                    }
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = $"Friend Record was Edited ({editFriendViewModel.Firstname} {editFriendViewModel.Lastname})";
                    return RedirectToAction(nameof(Index));
                }
            } catch (Exception ex) {
                TempData["errorMessage"] = ex.Message + "<br/>" + ex.StackTrace;
                return View();
            }
        }
        // GET:FriendController/Delete/4  // (4 is value of primary key (Id) that we are interested)
        [HttpGet]
        public async Task<IActionResult> Delete(int id) {
            try {
                var friend = await _context.Friends.SingleOrDefaultAsync(f => f.Id == id);
                TempData["PhotoFilePath"] = "/images/" + friend.PhotoFilename;
                TempData["DateOfBirth"] = friend.DateOfBirth?.ToString();
                if (friend.RegionOfBirth != null) {
                    TempData["RegionOfBirth"] = friend.RegionOfBirth?.ToString();
                } else {
                    TempData["RegionOfBirth"] = "Unknown";
                }
                return View(friend);
            } catch (Exception ex) {
                TempData["errorMessage"] = ex.Message + "<br/>" + ex.StackTrace;
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(FriendViewModel deleteFriendViewModel) {
            try {
                var friend = await _context.Friends.SingleOrDefaultAsync(f => f.Id == deleteFriendViewModel.Id);
                if (friend == null) {
                    TempData["errorMessage"] = $"Friend Not Found with Id {deleteFriendViewModel.Id}";
                    return View();
                } else {
                    _context.Friends.Remove(friend);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = $"Friend was Deleted ({deleteFriendViewModel.Firstname} {deleteFriendViewModel.Lastname})";
                    return RedirectToAction(nameof(Index));
                }
            } catch (Exception ex) {
                TempData["errorMessage"] = ex.Message + "<br/>" + ex.StackTrace;
                return View();
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View("Error!");
        }
    }
}