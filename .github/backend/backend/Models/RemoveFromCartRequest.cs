namespace backend.Models
{
    public class RemoveFromCartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }

}
