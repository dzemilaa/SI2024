namespace backend.Models
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string ReviewText { get; set; }

        public List<int> ProductIds { get; set; }
    }
   
}
