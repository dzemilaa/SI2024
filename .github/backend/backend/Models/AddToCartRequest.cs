namespace backend.Models
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
    }
}
