using DatabridgeServer.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace DatabridgeServer.Validators
{
    // 1. Validates POST and PUT (Body)
    public class EmployeeRequestValidator : AbstractValidator<Employee>
    {
        public EmployeeRequestValidator()
        {
            RuleFor(x => x.EmpName)
                .NotEmpty().WithMessage("Employee Name is required")
                .MinimumLength(3).WithMessage("Employee Name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Employee Name cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Employee Name must not contain numbers or special characters.");

            RuleFor(x => x.DeptName)
                .NotEmpty().WithMessage("Department Name is required")
                .MinimumLength(2).WithMessage("Department Name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Department Name cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Department Name must not contain numbers or special characters.");
        }
    }

    // 2. Validates GET by ID and DELETE (Route Parameter)
    public class EmployeeIdRequestValidator : AbstractValidator<EmployeeIdRequest>
    {
        public EmployeeIdRequestValidator()
        {
            RuleFor(x => x.EmpId)
                .GreaterThan(0).WithMessage("Employee ID is required and must be greater than 0.");
        }
    }

    // 3. Validates Delete Multiple (Body)
    public class DeleteMultipleEmployeesRequestValidator : AbstractValidator<DeleteMultipleEmployeesRequest>
    {
        public DeleteMultipleEmployeesRequestValidator()
        {
            RuleFor(x => x.EmpIds)
                .NotEmpty().WithMessage("Please provide at least one Employee ID to delete.")
                .Must(ids => ids != null && ids.All(id => id > 0))
                .WithMessage("All Employee IDs must be greater than 0.");
        }
    }

    // 4. Validates Bulk Import (Form File)
    public class FileImportRequestValidator : AbstractValidator<FileImportRequest>
    {
        public FileImportRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage("No file uploaded.")
                .Must(BeAValidFile).WithMessage("Invalid file format. Please upload an Excel file (.xlsx, .xls) or CSV (.csv).");
        }

        private bool BeAValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            
            var extension = Path.GetExtension(file.FileName).ToLower();
            return extension == ".xlsx" || extension == ".xls" || extension == ".csv";
        }
    }
}