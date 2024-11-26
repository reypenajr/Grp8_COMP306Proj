using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Group8_BrarPena.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Group8_BrarPena.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _coursesTable;

        public HomeController(ILogger<HomeController> logger, IAmazonDynamoDB dynamoDbClient)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
            _coursesTable = Table.LoadTable(_dynamoDbClient, "Courses");
        }

        //// GET: Home/Index
        //public async Task<IActionResult> Index(string? course)
        //{
        //    var scanFilter = new ScanFilter();
        //    if (!string.IsNullOrEmpty(course))
        //    {
        //        scanFilter.AddCondition("CourseCode", ScanOperator.Contains, course);
        //    }

        //    var search = _coursesTable.Scan(scanFilter);
        //    var documents = await search.GetNextSetAsync();

        //    var courses = documents.Select(doc => new Course
        //    {
        //        CourseId = doc["CourseId"],
        //        CourseCode = doc["CourseCode"],
        //        CourseYearSem = doc["CourseYearSem"],
        //        ProgramCode = doc["ProgramCode"],
        //        Term = doc["Term"]
        //    }).ToList();

        //    return View(courses);
        //}

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("~/Identity/Account/Login");
            }
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
