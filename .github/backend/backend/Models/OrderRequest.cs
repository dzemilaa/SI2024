namespace backend.Models
{
    public class OrderRequest
    {
        public int UserId { get; set; }   

        public int ProductId { get; set; }
        public string Address { get; set; }         
        public decimal TotalAmount { get; set; }    
        public List<CartItem> CartItems { get; set; } 

        public IFormFile Image { get; set; }
    }
}
