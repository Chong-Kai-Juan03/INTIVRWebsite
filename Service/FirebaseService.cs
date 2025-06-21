using Firebase.Database;
using Firebase.Database.Query;

namespace firstconnectfirebase.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebase;

        public FirebaseService(FirebaseClient firebase)
        {
            _firebase = firebase;
        }

        public async Task SaveSceneAsync(string imageUrl, string title)
        {
            var scenes = await _firebase
                .Child("Scenes")
                .OnceAsync<Scene>();

            var existingScene = scenes.FirstOrDefault(s =>
                s.Object.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (existingScene != null)
            {
                // Update the existing scene
                await _firebase
                    .Child("Scenes")
                    .Child(existingScene.Key)
                    .PatchAsync(new
                    {
                        ImageUrl = imageUrl,  // Consistent capitalization
                        CreatedAt = DateTime.UtcNow.ToString("o")  // Consistent capitalization
                    });
            }
            else
            {
                // Create a new scene
                await _firebase.Child("Scenes").PostAsync(new
                {
                    Title = title,  // Consistent capitalization
                    ImageUrl = imageUrl,  // Consistent capitalization
                    CreatedAt = DateTime.UtcNow.ToString("o")  // Consistent capitalization
                });
            }
        }

        public async Task TestConnectionAsync()
        {
            await _firebase.Child("test").PostAsync(new { test = DateTime.UtcNow });
        }

        public async Task<List<Scene>> GetScenesAsync()
        {
            try
            {
                var scenes = await _firebase
                    .Child("Scenes")
                    .OnceAsync<Scene>();

                return scenes
                    .Select(x => new Scene
                    {
                        Title = x.Object.Title,
                        ImageUrl = x.Object.ImageUrl,
                        CreatedAt = x.Object.CreatedAt,
                        Key = x.Key  // Important for updates
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching scenes: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateSceneAsync(string title, string newImageUrl)
        {
            var scenes = await _firebase
                .Child("Scenes")
                .OnceAsync<Scene>();

            var sceneToUpdate = scenes.FirstOrDefault(s =>
                s.Object.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (sceneToUpdate != null)
            {
                await _firebase
                    .Child("Scenes")
                    .Child(sceneToUpdate.Key)
                    .PatchAsync(new
                    {
                        ImageUrl = newImageUrl,
                        CreatedAt = DateTime.UtcNow.ToString("o")
                    });
            }
        }
    }

    public class Scene
    {
        public string Key { get; set; }  // Add this to track Firebase keys
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string CreatedAt { get; set; }
    }
}