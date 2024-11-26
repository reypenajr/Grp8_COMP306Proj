﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group8_BrarPena.Data;
using Group8_BrarPena.Models;
using Microsoft.AspNetCore.Authorization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Group8_BrarPena.Controllers
{
//    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _coursesTable;

        public CoursesController(ApplicationDbContext context, IAmazonDynamoDB dynamoDbClient)
        {
            _context = context;
            _dynamoDbClient = dynamoDbClient;
            _coursesTable = Table.LoadTable(_dynamoDbClient, "Courses");
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var search = _coursesTable.Scan(new ScanFilter());
            var documents = await search.GetNextSetAsync();
            var courses = documents.Select(doc => new Course
            {
                CourseId = doc["CourseId"],
                CourseCode = doc["CourseCode"],
                CourseYearSem = doc["CourseYearSem"],
                ProgramCode = doc["ProgramCode"],
                Term = doc["Term"]
            }).ToList();

            return View(courses);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(string id)
        {
            Console.WriteLine($"Fetching details for CourseId: {id}");

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("CourseId is required.");
            }

            try
            {
                var document = await _coursesTable.GetItemAsync(id);

                if (document == null)
                {
                    Console.WriteLine("Course not found in DynamoDB.");
                    return NotFound("Course not found.");
                }

                var course = new Course
                {
                    CourseId = document["CourseId"],
                    CourseCode = document["CourseCode"],
                    CourseYearSem = document["CourseYearSem"],
                    ProgramCode = document["ProgramCode"],
                    Term = document["Term"]
                };

                return View(course);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching course details: {ex.Message}");
                return BadRequest("An error occurred while fetching course details. Please try again.");
            }
        }


        //GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        //POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course model)
        {
            Console.WriteLine("Entered Create POST method for Course");

            if (model == null)
            {
                ModelState.AddModelError("", "No data was received. Please try again.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var courseDocument = new Document
                    {
                        ["CourseId"] = model.CourseId,
                        ["CourseCode"] = model.CourseCode,
                        ["CourseYearSem"] = model.CourseYearSem,
                        ["ProgramCode"] = model.ProgramCode,
                        ["Term"] = model.Term
                    };

                    Console.WriteLine("Inserting course document into DynamoDB...");
                    await _coursesTable.PutItemAsync(courseDocument);
                    Console.WriteLine("Course document inserted successfully.");

                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during course creation: {ex.Message}");
                    ModelState.AddModelError("", $"Error creating course: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(model);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _coursesTable.GetItemAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var course = new Course
            {
                CourseId = document["CourseId"],
                CourseCode = document["CourseCode"],
                CourseYearSem = document["CourseYearSem"],
                ProgramCode = document["ProgramCode"],
                Term = document["Term"]
            };

            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var courseDocument = new Document
                    {
                        ["CourseId"] = course.CourseId,
                        ["CourseCode"] = course.CourseCode,
                        ["CourseYearSem"] = course.CourseYearSem,
                        ["ProgramCode"] = course.ProgramCode,
                        ["Term"] = course.Term
                    };

                    await _coursesTable.PutItemAsync(courseDocument);

                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating course: {ex.Message}");
                }
            }

            return View(course);
        }

        // GET: Courses/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var course = await _context.Course
        //        .FirstOrDefaultAsync(m => m.CourseId == id);
        //    if (course == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(course);
        //}

        // POST: Courses/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var course = await _context.Course.FindAsync(id);
        //    if (course != null)
        //    {
        //        _context.Course.Remove(course);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool CourseExists(int id)
        //{
        //    return _context.Course.Any(e => e.CourseId == id);
        //}
    }
}
