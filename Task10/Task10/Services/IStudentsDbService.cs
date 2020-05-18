using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task10.Models;

namespace Task10.Services
{
    public interface IStudentsDbService
    {
        List<GetStudentsResponce> GetStudents();
        string InsertStudents(StudentRequest request);
        string UpdateStudents(StudentRequest request);
        string DeleteStudents(StudentRequest request);
        GetStudentsResponce EnrollStudent(StudentRequest request);
        GetStudentsResponce PromoteStudents(PromotionRequest request);
    }
}
