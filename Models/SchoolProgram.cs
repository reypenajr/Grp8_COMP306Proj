using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Group8_BrarPena.Data;
namespace Group8_BrarPena.Models
{
    public class SchoolProgram
    {
        public int ProgramCode { get; set; }
        public string ProgramName { get; set; }
    }
}
