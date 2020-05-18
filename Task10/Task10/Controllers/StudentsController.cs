using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task10.Entities;
using Task10.Models;
using Task10.Services;

namespace Task10.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private IStudentsDbService _service;
        public readonly StudentContext _context;
        public StudentsController(IStudentsDbService service, StudentContext context) 
        {
            _service = service;
            _context = context;
        }
        [HttpGet]
        public IActionResult GetStudents() 
        {
            var students = _service.GetStudents();
            if (students == null) 
            {
                return BadRequest("400 Bad Request Error!");
            }
            return Ok(students);
        }

        [HttpPost]
        public IActionResult InsertStudents(StudentRequest request) 
        {
            var res = _service.InsertStudents(request);
            if (res != null)
            {
                return BadRequest("400 Bad Request Error!");
            }
            return Ok("A student has been successfully added");
        }
        [HttpPut]
        public IActionResult UpdateStudents(StudentRequest request) 
        {
            var res = _service.UpdateStudents(request);
            if (res != null)
            {
                return BadRequest("400 Bad Request Error!");
            }
            return Ok("A student has been updated");
        }

        [HttpDelete]
        public IActionResult DeleteStudents(StudentRequest request)
        {
            var res = _service.DeleteStudents(request);
            if (res != null)
            {
                return BadRequest("400 Bad Request Error!");
            }
            return Ok("A student has been deleted");
        }

        [HttpPost("enrollments")]
        public IActionResult EnrollStudent([FromBody] StudentRequest request)
        {
            var enrollment = _service.EnrollStudent(request);
            if (enrollment == null)
            {
                return BadRequest("400 Bad Request Error!");
            }
            return CreatedAtAction(nameof(EnrollStudent), enrollment);
        }
        [HttpPost("promotions")]
        public IActionResult PromoteStudents(PromotionRequest request)
        {
            var enrollment = _service.PromoteStudents(request);
            if (enrollment == null)
            {
                return BadRequest("400 Bad Request Error!");
            }
            return CreatedAtAction(nameof(EnrollStudent), enrollment);
        }

    }
}