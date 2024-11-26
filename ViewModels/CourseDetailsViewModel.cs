using Group8_BrarPena.Models;

namespace Group8_BrarPena.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
    }
}
