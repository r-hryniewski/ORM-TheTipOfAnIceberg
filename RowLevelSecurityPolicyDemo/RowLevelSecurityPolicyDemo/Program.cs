using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RowLevelSecurityPolicyDemo.DAL;

namespace RowLevelSecurityPolicyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var company1 = new Company(1);
            var company2 = new Company(2);

            Console.WriteLine("Querying for employees with setting session state in DbContext ctor");
            PrintCompanyEmployeesWithSetSessionContextInCtor(company1);
            PrintCompanyEmployeesWithSetSessionContextInCtor(company2);

            Console.WriteLine("Querying for employees with setting session state in DbConnection StatusChange event");
            PrintCompanyEmployeesWithSetSessionContextOnDbConnectionOpened(company1);
            PrintCompanyEmployeesWithSetSessionContextOnDbConnectionOpened(company2);

            Console.WriteLine("Querying for employees with setting session state in interceptor (this will work only when you uncomment contents of uncomment EmployeesDbContextConfiguration class");
            PrintCompanyEmployeesWithSetSessionContextInInterceptor();

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static void PrintCompanyEmployeesWithSetSessionContextInInterceptor()
        {
            Console.WriteLine($"Employees of company with unknown id (it's being set in interceptor)");

            using (var ctx = new EmployeesDbContext())
            {
                var employees = ctx.Employees.AsNoTracking().ToList();
                foreach (var employee in employees)
                {
                    Console.WriteLine(employee.ToString());
                }
            }

            Console.WriteLine("---");
        }

        private static void PrintCompanyEmployeesWithSetSessionContextInCtor(ICompany company)
        {
            Console.WriteLine($"Employees of company with id {company.Id}:");

            using (var ctx = new EmployeesDbContext(company))
            {
                var employees = ctx.Employees.AsNoTracking().ToList();
                foreach (var employee in employees)
                {
                    Console.WriteLine(employee.ToString());
                }
            }

            Console.WriteLine("---");
        }

        private static void PrintCompanyEmployeesWithSetSessionContextOnDbConnectionOpened(ICompany company)
        {
            Console.WriteLine($"Employees of company with id {company.Id}:");

            using (var ctx = new EmployeesDbContext(company: company, parameterToChangeMethodSignature: true))
            {
                var employees = ctx.Employees.AsNoTracking().ToList();
                foreach (var employee in employees)
                {
                    Console.WriteLine(employee.ToString());
                }
            }

            Console.WriteLine("---");
        }
    }
}
