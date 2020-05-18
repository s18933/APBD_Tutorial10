using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Task10.Entities;
using Task10.Models;

namespace Task10.Services
{
    public class SqlServerStudentsDbService : IStudentsDbService
    {
        public readonly StudentContext _context;
        public SqlServerStudentsDbService(StudentContext context)
        {
            _context = context;
        }
        public List<GetStudentsResponce> GetStudents()
        {
            var students = _context.Student
                                  .Include(x => x.IdEnrollmentNavigation).ThenInclude(x => x.IdStudyNavigation)
                                  .Select(s => new GetStudentsResponce
                                  {
                                      IndexNumber = s.IndexNumber,
                                      FirstName = s.FirstName,
                                      LastName = s.LastName,
                                      BirthDate = s.BirthDate.ToShortDateString(),
                                      Semester = s.IdEnrollmentNavigation.Semester,
                                      Studies = s.IdEnrollmentNavigation.IdStudyNavigation.Name,
                                      CurrentDate = s.IdEnrollmentNavigation.StartDate.ToShortDateString()
                                  })
                                  .ToList();
            return students;
        }

        public string InsertStudents(StudentRequest request)
        {
            _context.Student.Add(new Student
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdEnrollment = request.IdEnrollment
            });
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return "Exception caused!";
            }
            return null;
        }

        public string UpdateStudents(StudentRequest request)
        {
            var update_student = new Student
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdEnrollment = request.IdEnrollment
            };

            _context.Update(update_student);
            _context.Entry(update_student).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return "Exception caused!";
            }
            return null;
        }

        public string DeleteStudents(StudentRequest request)
        {
            var delete_student = new Student
            {
                IndexNumber = request.IndexNumber
            };

            _context.Remove(delete_student);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return "Exception caused!";
            }
            return null;
        }

        public GetStudentsResponce EnrollStudent(StudentRequest request)
        {
            if (request.FirstName == null || request.BirthDate == null || request.StudiesName == null)
            {
                return null;
            }

                        int idStudy = 0;
                        int idEnrollment = 0;

            var existsStudy = _context.Studies.Any(e => e.Name == request.StudiesName);

            if (!existsStudy)
            {
                return null;
            }

            var req1 = _context.Enrollment.Where(e => e.Semester == 1 && e.IdStudyNavigation.Name == request.StudiesName)
                                          .OrderByDescending(e => e.IdEnrollment)
                                          .Select(e => e.IdEnrollment)
                                          .FirstOrDefault();

            if (req1 == 0)
            {
                idStudy = _context.Studies.Single(e => e.Name == request.StudiesName).IdStudy;
                int req2 = _context.Enrollment.Max(e => e.IdEnrollment);
                DateTime currentDate = DateTime.Now;
                idEnrollment = req2 + 1; //Convert.ToInt32(req2) + 1;

                _context.Enrollment.Add(new Enrollment { IdEnrollment = idEnrollment, Semester = 1, IdStudy = idStudy, StartDate = currentDate });
            }
            else
            {
                idEnrollment = req1;
            }

            string indexNumber = $"s{new Random().Next(1, 2000)}";

            var checking = _context.Student.Any(e => e.IndexNumber == indexNumber);
            if (checking)
            { 
                return null;
            }
            var req4 = _context.Student.Add(new Student { IndexNumber = indexNumber, FirstName = request.FirstName, LastName = request.LastName, BirthDate = request.BirthDate, IdEnrollment = idEnrollment});
            _context.SaveChanges();
            var enrollment = _context.Student
                                   .Include(x => x.IdEnrollmentNavigation).ThenInclude(x => x.IdStudyNavigation)
                                   .Where(s => s.IndexNumber == indexNumber)
                                   .Select(s => new GetStudentsResponce
                                   {
                                       IndexNumber = s.IndexNumber,
                                       FirstName = s.FirstName,
                                       LastName = s.LastName,
                                       BirthDate = s.BirthDate.ToShortDateString(),
                                       Studies = s.IdEnrollmentNavigation.IdStudyNavigation.Name,
                                       Semester = s.IdEnrollmentNavigation.Semester,
                                       CurrentDate = s.IdEnrollmentNavigation.StartDate.ToShortDateString()
                                   }).Single();
           
            return enrollment;
        }
        public GetStudentsResponce PromoteStudents(PromotionRequest request)
        {
            int idStudy = 0;
            int newIdEnrollment = 0;

            var existsStudy = _context.Studies.Any(e => e.Name == request.StudiesName);
            if (!existsStudy)
            {
                return null;
            }

            var existsEnrollment = _context.Enrollment.Any(e => e.IdStudyNavigation.Name == request.StudiesName && e.Semester == request.Semester);
            if (!existsEnrollment)
            {
                return null;
            }
            idStudy = _context.Studies.Single(e => e.Name == request.StudiesName).IdStudy;
            var oldEnrollment = _context.Enrollment.Where(e => e.Semester == request.Semester && e.IdStudy == idStudy)
                                              .Select(e => e.IdEnrollment)
                                              .FirstOrDefault();

            newIdEnrollment = _context.Enrollment.Where(e => e.Semester == request.Semester+1 && e.IdStudy == idStudy+1)
                                              .Select(e => e.IdEnrollment)
                                              .FirstOrDefault();
            if (newIdEnrollment == 0)
            {
                int req1 = _context.Enrollment.Max(e => e.IdEnrollment);
                DateTime currentDate = DateTime.Now;
                newIdEnrollment = req1 + 1;

                _context.Enrollment.Add(new Enrollment { IdEnrollment = newIdEnrollment, Semester = request.Semester+1, IdStudy = idStudy+1, StartDate = currentDate });
            }

            var list = _context.Student.Where(e => e.IdEnrollment == oldEnrollment)
                .Select(s => new StudentRequest
            {
                IndexNumber = s.IndexNumber,
                FirstName = s.FirstName,
                LastName = s.LastName,
                BirthDate = s.BirthDate,
                IdEnrollment = s.IdEnrollment
            })
                .ToList();
            
            for (var i = 0; i < list.Count; i++)
            {
                list[i].IdEnrollment = newIdEnrollment;
                UpdateStudents(list[i]);
            }

            _context.SaveChanges();
            var enrollment = _context.Student
                                   .Include(x => x.IdEnrollmentNavigation).ThenInclude(x => x.IdStudyNavigation)
                                   .Where(s => s.IdEnrollment == newIdEnrollment)
                                   .Select(s => new GetStudentsResponce
                                   {
                                       IndexNumber = s.IndexNumber,
                                       FirstName = s.FirstName,
                                       LastName = s.LastName,
                                       BirthDate = s.BirthDate.ToShortDateString(),
                                       Studies = s.IdEnrollmentNavigation.IdStudyNavigation.Name,
                                       Semester = s.IdEnrollmentNavigation.Semester,
                                       CurrentDate = s.IdEnrollmentNavigation.StartDate.ToShortDateString()
                                   }).FirstOrDefault();
            
            return enrollment;
        }
    }
}
