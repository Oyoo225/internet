using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FriendSQLiteMVCVSCode.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FriendSQLiteMVCVSCode.Controllers {
    [Route("[controller]/{action=Index}/{Id=0}")]
    public class FriendController : Controller {
        private readonly FriendDbContext _context;
        private readonly IWebHostEnvironment environment;

        public FriendController(FriendDbContext friendDbContext, IWebHostEnvironment environment) {
            this._context = friendDbContext;
            this.environment = environment;
        }

        private (string Sign, string ImageFilename) GetZodiacSign(DateTime birthDate) {
            int day = birthDate.Day;
            int month = birthDate.Month;

            return (month, day) switch
            {
                (1, >= 20) or (2, <= 18) => ("Aquarius", "aquarius.jpg"),
                (2, >= 19) or (3, <= 20) => ("Pisces", "pisces.jpg"),
                (3, >= 21) or (4, <= 19) => ("Aries", "aries.jpg"),
                (4, >= 20) or (5, <= 20) => ("Taurus", "taurus.jpg"),
                (5, >= 21) or (6, <= 20) => ("Gemini", "gemini.jpg"),
                (6, >= 21) or (7, <= 22) => ("Cancer", "cancer.jpg"),
                (7, >= 23) or (8, <= 22) => ("Leo", "leo.jpg"),
                (8, >= 23) or (9, <= 22) => ("Virgo", "virgo.jpg"),
                (9, >= 23) or (10, <= 22) => ("Libra", "libra.jpg"),
                (10, >= 23) or (11, <= 21) => ("Scorpio", "scorpio.jpg"),
                (11, >= 22) or (12, <= 21) => ("Sagittarius", "sagittarius.jpg"),
                (12, >= 22) or (1, <= 19) => ("Capricorn", "capricorn.jpg"),
                _ => ("Unknown", "NoData.jpg")
            };
        }

        // GET: FriendController
        [HttpGet]
        public async Task<IActionResult> Index() {
            var friendList = await _context.Friends.ToListAsync();
            friendList = friendList.OrderByDescending(x => x.Id).ToList();

            foreach (var friend in friendList) {
                if (string.IsNullOrEmpty(friend.PhotoFilename)) {
                    friend.PhotoFilename = "NoData.jpg";
                }
            }

            return View(friendList);
        }

        // GET: FriendController/Create
        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FriendViewModel addFriendViewModel, IFormFile PhotoFile) {
            try {
                string strPhotoFile = "NoData.jpg";

                if (PhotoFile != null) {
                    string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    strPhotoFile = strDateTime + "_" + PhotoFile.FileName;
                    string photoFullPath = Path.Combine(environment.WebRootPath, "images", strPhotoFile);

                    using (var fileStream = new FileStream(photoFullPath, FileMode.Create)) {
                        await PhotoFile.CopyToAsync(fileStream);
                    }
                }

                DateTime? parsedDate = null;
                string zodiacSign = "Unknown";

                if (!string.IsNullOrEmpty(addFriendViewModel.DateOfBirth) &&
                    DateTime.TryParse(addFriendViewModel.DateOfBirth, out DateTime tempDate)) {
                    parsedDate = tempDate;
                    (zodiacSign, _) = GetZodiacSign(tempDate); // Correct tuple deconstruction
                }

                var friend = new FriendViewModel() {
                    Firstname = addFriendViewModel.Firstname,
                    Lastname = addFriendViewModel.Lastname,
                    Note = addFriendViewModel.Note,
                    DateOfBirth = parsedDate?.ToString("yyyy-MM-dd") ?? "Unknown",
                    RegionOfBirth = addFriendViewModel.RegionOfBirth,
                    PhotoFilename = strPhotoFile,
                    Sign = zodiacSign
                };

                await _context.Friends.AddAsync(friend);
                await _context.SaveChangesAsync();

                TempData["successMessage"] = $"New Friend Created: {friend.Firstname} {friend.Lastname}";
                return RedirectToAction(nameof(Index));
            } catch (Exception ex) {
                TempData["errorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }

        // GET: FriendController/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            var friend = await _context.Friends.FindAsync(id);
            if (friend == null) {
                TempData["errorMessage"] = "Friend not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["PhotoFilePath"] = "/images/" + (friend.PhotoFilename ?? "NoData.jpg");
            TempData["DateOfBirth"] = friend.DateOfBirth ?? "Unknown";
            TempData["RegionOfBirth"] = friend.RegionOfBirth ?? "Unknown";

            return View(friend);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FriendViewModel editFriendViewModel, IFormFile PhotoFile) {
            var friend = await _context.Friends.FindAsync(editFriendViewModel.Id);
            if (friend == null) {
                TempData["errorMessage"] = "Friend not found.";
                return View();
            }

            friend.Firstname = editFriendViewModel.Firstname;
            friend.Lastname = editFriendViewModel.Lastname;
            friend.Note = editFriendViewModel.Note;
            friend.RegionOfBirth = editFriendViewModel.RegionOfBirth;

            if (!string.IsNullOrEmpty(editFriendViewModel.DateOfBirth) &&
                DateTime.TryParse(editFriendViewModel.DateOfBirth, out DateTime parsedDate)) {
                friend.DateOfBirth = parsedDate.ToString("yyyy-MM-dd");
                (friend.Sign, _) = GetZodiacSign(parsedDate);
            }

            if (PhotoFile != null) {
                string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                string strPhotoFile = strDateTime + "_" + PhotoFile.FileName;
                string photoFullPath = Path.Combine(environment.WebRootPath, "images", strPhotoFile);

                using (var fileStream = new FileStream(photoFullPath, FileMode.Create)) {
                    await PhotoFile.CopyToAsync(fileStream);
                }

                friend.PhotoFilename = strPhotoFile;
            }

            await _context.SaveChangesAsync();
            TempData["successMessage"] = $"Friend Updated: {editFriendViewModel.Firstname} {editFriendViewModel.Lastname}";
            return RedirectToAction(nameof(Index));
        }

        // GET: FriendController/Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id) {
            var friend = await _context.Friends.FindAsync(id);
            if (friend == null) {
                TempData["errorMessage"] = "Friend not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(friend);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(FriendViewModel deleteFriendViewModel) {
            var friend = await _context.Friends.FindAsync(deleteFriendViewModel.Id);
            if (friend == null) {
                TempData["errorMessage"] = "Friend not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();

            TempData["successMessage"] = $"Friend Deleted: {deleteFriendViewModel.Firstname} {deleteFriendViewModel.Lastname}";
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View("Error!");
        }
    }
}
