namespace backend.Models
{
    public class Order
    {
        public int Id { get; set; }             
        public int UserId { get; set; }           
        public string Address { get; set; }        
        public decimal TotalAmount { get; set; }  
        public DateTime OrderDate { get; set; }   
        public string Status { get; set; }        
        public IFormFile Image { get; set; }      
        public string FirstName { get; set; }      
        public string LastName { get; set; }       
        public string Email { get; set; }         
        public string Phone { get; set; }          
    }
}
