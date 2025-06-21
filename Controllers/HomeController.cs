using System.Diagnostics;
using firstconnectfirebase.Models;
using firstconnectfirebase.Services;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Firebase.Database;
using System.Linq;

namespace firstconnectfirebase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Cloudinary _cloudinary;
        private readonly FirebaseService _firebaseService;
        private static List<Card> _cards = new List<Card>();

        public HomeController(
            ILogger<HomeController> logger,
            Cloudinary cloudinary,
            FirebaseService firebaseService)
        {
            _logger = logger;
            _cloudinary = cloudinary;
            _firebaseService = firebaseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Cards()
        {
            try
            {
                var scenes = await _firebaseService.GetScenesAsync();
                var cards = scenes.Select(s => new Card
                {
                    Id = s.Key,
                    Title = s.Title,
                    ImagePath = s.ImageUrl,
                    CreatedDate = DateTime.Parse(s.CreatedAt) // Parse the string to DateTime
                }).ToList();
                
                return View(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scenes from Firebase");
                return View(new List<Card>());
            }
        }

        public IActionResult Upload360()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload360(IFormFile panoramaImage, string title)
        {
            try
            {
                // 1. Upload image to Cloudinary (keep existing functionality)
                var uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams
                {
                    File = new FileDescription(panoramaImage.FileName, panoramaImage.OpenReadStream()),
                    PublicId = $"vr360/{Guid.NewGuid()}"
                });

                // 2. Check if scene exists in Firebase
                var scenes = await _firebaseService.GetScenesAsync();
                var existingScene = scenes.FirstOrDefault(s => s.Title == title);

                if (existingScene != null)
                {
                    // Update existing scene
                    await _firebaseService.SaveSceneAsync(
                        imageUrl: uploadResult.SecureUrl.ToString(),
                        title: title
                    );
                }
                else
                {
                    // Create new scene (keep existing functionality)
                    await _firebaseService.SaveSceneAsync(
                        imageUrl: uploadResult.SecureUrl.ToString(),
                        title: title
                    );
                }

                // 3. Return result
                return Json(new
                {
                    success = true,
                    url = uploadResult.SecureUrl.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading 360 image");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> TestFirebase()
        {
            try
            {
                await _firebaseService.TestConnectionAsync();
                return Content("Firebase Connected successfully！");
            }
            catch (Exception ex)
            {
                return Content($"Firebase connecting fail: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetScenes()
        {
            try
            {
                var scenes = await _firebaseService.GetScenesAsync();
                return Json(scenes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get scene list: {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}