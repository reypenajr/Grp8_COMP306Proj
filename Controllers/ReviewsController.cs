using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group8_BrarPena.Data;
using Group8_BrarPena.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Group8_BrarPena.Controllers
{
    public class ReviewsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _reviewsTable;

        public ReviewsController(ApplicationDbContext context, IAmazonDynamoDB dynamoDbClient)
        {
            _context = context;
            _dynamoDbClient = dynamoDbClient;
            _reviewsTable = Table.LoadTable(_dynamoDbClient, "Reviews");
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var search = _reviewsTable.Scan(new ScanFilter());
            var documents = await search.GetNextSetAsync();
            var reviews = documents.Select(doc => new Review
            {
                ReviewId = int.TryParse(doc["ReviewId"]?.ToString(), out var reviewId) ? reviewId : 0,
                CreatedDate = DateTime.TryParse(doc["CreatedDate"]?.AsString(), out var createdDate) ? createdDate : (DateTime?)null,
                UpdatedDate = DateTime.TryParse(doc["UpdatedDate"]?.AsString(), out var updatedDate) ? updatedDate : (DateTime?)null,
                ReviewerName = doc["ReviewerName"]?.AsString(),
                Rating = int.TryParse(doc["Rating"]?.ToString(), out var rating) ? rating : 0,
                Body = doc["Body"]?.AsString(),
                CourseId = doc["CourseId"]?.AsString()
            }).ToList();

            return View(reviews);
        }


        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int id)
        {

            try
            {
                var document = await _reviewsTable.GetItemAsync(id);

                if (document == null)
                {
                    return NotFound();
                }

                var review = new Review
                {
                    ReviewId = int.TryParse(document["ReviewId"]?.ToString(), out var reviewId) ? reviewId : 0,
                    CreatedDate = DateTime.TryParse(document["CreatedDate"]?.AsString(), out var createdDate) ? createdDate : (DateTime?)null,
                    UpdatedDate = DateTime.TryParse(document["UpdatedDate"]?.AsString(), out var updatedDate) ? updatedDate : (DateTime?)null,
                    ReviewerName = document["ReviewerName"]?.AsString(),
                    Rating = int.TryParse(document["Rating"]?.ToString(), out var rating) ? rating : 0,
                    Body = document["Body"]?.AsString(),
                    CourseId = document["CourseId"]?.AsString()
                };

                return View(review);
            }
            catch (AmazonDynamoDBException ex)
            {
                Console.WriteLine($"DynamoDB Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpGet]
        public IActionResult Create(string courseId)
        {
            // Get the logged-in user's email from the authentication context
            var userEmail = User.Identity.Name; // This assumes the email is stored as the username in your authentication system

            var review = new Review
            {
                ReviewerName = userEmail, // Autofill ReviewerName with the logged-in user's email
                CourseId = courseId
            };

            return View(review);
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewId,CreatedDate,ReviewerName,Rating,Body,CourseId")] Review review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedDate = DateTime.UtcNow;
                review.UpdatedDate = DateTime.UtcNow;
                review.ReviewId = new Random().Next(1, int.MaxValue);

                var reviewDocument = new Document
                {
                    ["ReviewId"] = review.ReviewId,
                    ["CreatedDate"] = review.CreatedDate?.ToString("o") ?? string.Empty,
                    ["UpdatedDate"] = review.UpdatedDate?.ToString("o") ?? string.Empty,
                    ["ReviewerName"] = review.ReviewerName,
                    ["Rating"] = review.Rating,
                    ["Body"] = review.Body,
                    ["CourseId"] = review.CourseId
                };

                await _reviewsTable.PutItemAsync(reviewDocument);

                return RedirectToAction("Details", "Courses", new { id = review.CourseId });
            }

            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var document = await _reviewsTable.GetItemAsync(new Primitive(id.ToString(), true));

                if (document == null)
                {
                    return NotFound();
                }

                var review = new Review
                {
                    ReviewId = int.TryParse(document["ReviewId"]?.ToString(), out var reviewId) ? reviewId : 0,
                    CreatedDate = DateTime.TryParse(document["CreatedDate"]?.AsString(), out var createdDate) ? createdDate : (DateTime?)null,
                    UpdatedDate = DateTime.TryParse(document["UpdatedDate"]?.AsString(), out var updatedDate) ? updatedDate : (DateTime?)null,
                    ReviewerName = document["ReviewerName"]?.AsString(),
                    Rating = int.TryParse(document["Rating"]?.ToString(), out var rating) ? rating : 0,
                    Body = document["Body"]?.AsString(),
                    CourseId = document["CourseId"]?.AsString() // Make sure to include CourseId here
                };

                return View(review);
            }
            catch (AmazonDynamoDBException ex)
            {
                Console.WriteLine($"DynamoDB Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,ReviewerName,Rating,Body,CourseId")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            review.UpdatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                try
                {
                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = "Reviews",
                        Key = new Dictionary<string, AttributeValue>
                {
                    { "ReviewId", new AttributeValue { N = review.ReviewId.ToString() } }
                },
                        ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#ReviewerName", "ReviewerName" },
                    { "#Rating", "Rating" },
                    { "#Body", "Body" },
                    { "#CourseId", "CourseId" },  // Add CourseId to the update expression
                    { "#UpdatedDate", "UpdatedDate" }
                },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":reviewerName", new AttributeValue { S = review.ReviewerName } },
                    { ":rating", new AttributeValue { N = review.Rating.ToString() } },
                    { ":body", new AttributeValue { S = review.Body } },
                    { ":courseId", new AttributeValue { S = review.CourseId } },  // Ensure CourseId is updated
                    { ":updatedDate", new AttributeValue { S = review.UpdatedDate.Value.ToString("o") } }
                },
                        UpdateExpression = "SET #ReviewerName = :reviewerName, #Rating = :rating, #Body = :body, #CourseId = :courseId, #UpdatedDate = :updatedDate"
                    };

                    await _dynamoDbClient.UpdateItemAsync(updateRequest);

                    Console.WriteLine($"Updated review with ID: {review.ReviewId}");
                    return RedirectToAction("Details", "Courses", new { id = review.CourseId });
                }
                catch (AmazonDynamoDBException ex)
                {
                    Console.WriteLine($"DynamoDB Error: {ex.Message}");
                    return StatusCode(500, "Internal server error.");
                }
            }

            return View(review);
        }



        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the review from DynamoDB using ReviewId as a number
                var document = await _reviewsTable.GetItemAsync(id);

                if (document == null)
                {
                    return NotFound();
                }

                var review = new Review
                {
                    ReviewId = int.TryParse(document["ReviewId"]?.ToString(), out var reviewId) ? reviewId : 0,
                    CreatedDate = DateTime.TryParse(document["CreatedDate"]?.AsString(), out var createdDate) ? createdDate : (DateTime?)null,
                    UpdatedDate = DateTime.TryParse(document["UpdatedDate"]?.AsString(), out var updatedDate) ? updatedDate : (DateTime?)null,
                    ReviewerName = document["ReviewerName"]?.AsString(),
                    Rating = int.TryParse(document["Rating"]?.ToString(), out var rating) ? rating : 0,
                    Body = document["Body"]?.AsString(),
                    CourseId = document["CourseId"]?.AsString()
                };

                return View(review);
            }
            catch (AmazonDynamoDBException ex)
            {
                Console.WriteLine($"DynamoDB Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            try
            {
                var hashKeyValue = id;

                var document = await _reviewsTable.GetItemAsync(id);

                var courseId = document["CourseId"]?.AsString();

                await _reviewsTable.DeleteItemAsync(hashKeyValue);

                return RedirectToAction("Details", "Courses", new { id = courseId });
            }
            catch (AmazonDynamoDBException ex)
            {
                Console.WriteLine($"DynamoDB Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }


    }
}
