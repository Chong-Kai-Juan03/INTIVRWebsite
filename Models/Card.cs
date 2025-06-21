
namespace firstconnectfirebase.Models
{
    public class Card
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
