
namespace backend.Models
{
    public class Reviews
    {
        public int ReviewId { get; set; } 
        public int OrderId { get; set; } 

        public int UserId { get; set; }
        public string ReviewText { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public virtual Order Order { get; set; }
        public virtual Registration Registration { get; set; }
    }
}