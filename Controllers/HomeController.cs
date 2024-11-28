using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Group8_BrarPena.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        // GET: Home/Index
        public async Task<IActionResult> Index(string? course, string sortColumn, string sortOrder)
        {
            var scanFilter = new ScanFilter();
            if (!string.IsNullOrEmpty(course))
            {
                scanFilter.AddCondition("CourseCode", ScanOperator.Contains, course);
            }

            var search = _coursesTable.Scan(scanFilter);
            var documents = await search.GetNextSetAsync();

            var courses = documents.Select(doc => new Course
            {
                CourseId = doc["CourseId"],
                CourseCode = doc["CourseCode"],
                CourseYearSem = doc["CourseYearSem"],
                ProgramCode = doc["ProgramCode"],
                Term = doc["Term"]
            }).ToList();

            switch (sortColumn)
            {
                case "CourseCode":
                    courses = sortOrder == "asc" ? courses.OrderBy(c => c.CourseCode).ToList() : courses.OrderByDescending(c => c.CourseCode).ToList();
                    break;
                case "CourseYearSem":
                    courses = sortOrder == "asc" ? courses.OrderBy(c => c.CourseYearSem).ToList() : courses.OrderByDescending(c => c.CourseYearSem).ToList();
                    break;
                case "Term":
                    courses = sortOrder == "asc" ? courses.OrderBy(c => c.Term).ToList() : courses.OrderByDescending(c => c.Term).ToList();
                    break;
                case "ProgramCode":
                    courses = sortOrder == "asc" ? courses.OrderBy(c => c.ProgramCode).ToList() : courses.OrderByDescending(c => c.ProgramCode).ToList();
                    break;
                default:
                    courses = sortOrder == "asc" ? courses.OrderBy(c => c.CourseId).ToList() : courses.OrderByDescending(c => c.CourseId).ToList();
                    break;
            }

            ViewData["CurrentSortColumn"] = sortColumn;
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchQuery"] = course;

            return View(courses);
        }

        // GET: Home/AdvancedSearch
        public async Task<IActionResult> AdvancedSearch(string? courseCode, string? programCode, string? term, string? courseYearSem, string? sortColumn, string? sortOrder)
        {
            var results = new List<Document>();

            // Dynamically add filters based on user input
            if (!string.IsNullOrEmpty(courseCode))
            {
                var tempFilter = new ScanFilter();
                tempFilter.AddCondition("CourseCode", ScanOperator.Contains, courseCode);
                var tempSearch = _coursesTable.Scan(tempFilter);
                results.AddRange(await tempSearch.GetNextSetAsync());
            }

            if (!string.IsNullOrEmpty(programCode))
            {
                var tempFilter = new ScanFilter();
                tempFilter.AddCondition("ProgramCode", ScanOperator.Contains, programCode);
                var tempSearch = _coursesTable.Scan(tempFilter);
                results.AddRange(await tempSearch.GetNextSetAsync());
            }

            if (!string.IsNullOrEmpty(term))
            {
                var tempFilter = new ScanFilter();
                tempFilter.AddCondition("Term", ScanOperator.Contains, term);
                var tempSearch = _coursesTable.Scan(tempFilter);
                results.AddRange(await tempSearch.GetNextSetAsync());
            }

            if (!string.IsNullOrEmpty(courseYearSem))
            {
                var tempFilter = new ScanFilter();
                tempFilter.AddCondition("CourseYearSem", ScanOperator.Contains, courseYearSem);
                var tempSearch = _coursesTable.Scan(tempFilter);
                results.AddRange(await tempSearch.GetNextSetAsync());
            }

            // Remove duplicates
            var distinctDocuments = results.Distinct().ToList();

            // Map results to the Course model
            var courses = distinctDocuments.Select(doc => new Course
            {
                CourseId = doc["CourseId"],
                CourseCode = doc["CourseCode"],
                CourseYearSem = doc["CourseYearSem"],
                ProgramCode = doc["ProgramCode"],
                Term = doc["Term"]
            }).ToList();

            // Apply sorting if specified
            courses = sortColumn switch
            {
                "CourseCode" => sortOrder == "asc" ? courses.OrderBy(c => c.CourseCode).ToList() : courses.OrderByDescending(c => c.CourseCode).ToList(),
                "CourseYearSem" => sortOrder == "asc" ? courses.OrderBy(c => c.CourseYearSem).ToList() : courses.OrderByDescending(c => c.CourseYearSem).ToList(),
                "Term" => sortOrder == "asc" ? courses.OrderBy(c => c.Term).ToList() : courses.OrderByDescending(c => c.Term).ToList(),
                "ProgramCode" => sortOrder == "asc" ? courses.OrderBy(c => c.ProgramCode).ToList() : courses.OrderByDescending(c => c.ProgramCode).ToList(),
                _ => sortOrder == "asc" ? courses.OrderBy(c => c.CourseId).ToList() : courses.OrderByDescending(c => c.CourseId).ToList(),
            };

            // Pass sorting and search data to the view
            ViewData["CurrentSortColumn"] = sortColumn;
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearchQuery"] = $"{courseCode} {programCode} {term} {courseYearSem}";

            // Return the same Index view
            return View("Index", courses);
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
