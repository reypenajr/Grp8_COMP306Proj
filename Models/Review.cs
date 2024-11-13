namespace Group8_BrarPena.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReviewerName { get; set; }
        public int Rating {  get; set; }
        public string Body { get; set; }
    }
}
