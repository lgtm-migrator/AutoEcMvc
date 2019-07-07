using AutoEcMvc.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace AutoEcMvc.Controllers
{
    public partial class CoursesController
    {
        protected override void _AfterUpdate(object objToUpdate = null)
        {
            PopulateDepartmentsDropDownList(objToUpdate);
        }

        private void PopulateDepartmentsDropDownList(object currentCourse = null)
        {
            int? departmentId = null;
            if (currentCourse is Course course) departmentId = course.DepartmentID;

            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;
            ViewBag.DepartmentID = new SelectList(departmentsQuery.AsNoTracking(), "DepartmentID", "Name", departmentId);
        }

        public IActionResult UpdateCourseCredits()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourseCredits(int? multiplier)
        {
            if (multiplier != null)
            {
                ViewData["RowsAffected"] =
                    await _context.Database.ExecuteSqlCommandAsync(
                        "UPDATE Course SET Credits = Credits * {0}",
                        parameters: multiplier);
            }
            return View();
        }
    }
}
