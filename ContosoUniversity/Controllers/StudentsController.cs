using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber /* Indexes the pages*/)
        {
            
            // keeps track of our sorting order
            ViewData["CurrentSort"] = sortOrder;
            // if our sort order is empty (what's passed through as an arg), our view using sort data is changed to either ascending (default) or descending.
            // Clicking on the header/links within the page will change how they're displayed.
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            // Similar to the name sort, but using the date. Checks if the param is a date, if it is, we sort basedd on the date.
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            // if the search string isn't null, we change the pageNumber to 1 as it's a new page else we'll set the searchString to our current filter.
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            // Assign the param we're searching for into the view data.
            ViewData["CurrentFilter"] = searchString;

            // uses LINQ to grab each student from the DB for listing
            var students = from s in _context.Students
                           select s;

            /*
             * If our search string is empty, we don't want to make a query. Instead, we just want to display everything.
             * In the case that our search string isn't empty/null, we want to search for the given parameter.
             * For this case, we query the students we gathered from our selection of students where the search parameters
             * mathc a student's last or middle name.
             */
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));
            }

            // changess our collection's sorting based on the param. 
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            // # of students we wish to display on a page
            int pageSize = 3;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1/* returns the value of pageNumber if there's a value, otherwise 1 if null*/, pageSize));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*
             * This is taken from the dykstra2 portion of the tutorial (Implement CRUD Function).
             * This line simply grabs a student entity from the established context via
             * course, enrollments, and a specific ID.
             */
            var student = await _context.Students
                            .Include(s => s.Enrollments)
                                .ThenInclude(e => e.Course)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Since we don't want the user to input an ID (we want to generate it server-side), get rid of it from the bind.
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            /*
             * If any update to the DB fails, we want to display an error and tell the user to try again.
             * Without a try/catch, a DB error can go uncaught and potentially crash the application
             * or cause unintended behavior. It also gives a specific behavior to a DB update failing.
             */
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

 

        /*
         * Added the seperate edit action.  
         */
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Student>(
                studentToUpdate,
                "",
                s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        // Added a flag to handle errors. Defaulted to false, otherwise can be changed
        // to log errors.
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }
            
            // Based on the delete flag, will log the errors.
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            // This is the read-first approach of httppost delete
            // doesn't delete anything if the student is null (i.e. DNE)
            if (student == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // if there is any sort of dbupdate exception that is thrown, we stop.
            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // log the error if one is thrown
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
