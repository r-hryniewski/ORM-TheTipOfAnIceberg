﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Data;
using System.Data.SqlClient;

namespace RowLevelSecurityPolicyDemo.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EmployeesDbContext : DbContext
    {
        private readonly long companyId;

        //Setting sql session state in constructor
        public EmployeesDbContext(ICompany company)
            : base("name=EmployeesDbContext")
        {
            //Setting Company Id in SQL session won't work without it because EF will open and close SqlConnections for every command unless:
            //a. you'll pass explicitly opened SqlConnection into DbContext constructor 
            //b. you'll call Database.Connection.Open() by hand.
            this.Database.Connection.Open();


            companyId = company.Id;
            SetCompanyIdInSqlSession();
        }

        //Setting sql session state in DbConnection StateChange event
        public EmployeesDbContext(ICompany company, bool parameterToChangeMethodSignature)
            : base("name=EmployeesDbContext")
        {
            companyId = company.Id;
            this.Database.Connection.StateChange += OnConnectionOpened;

        }

        private void OnConnectionOpened(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open)
                SetCompanyIdInSqlSession();
        }

        public EmployeesDbContext()
            : base("name=EmployeesDbContext")
        {
            //Parameterless ctor which will work only if you uncomment EmployeesDbContextConfiguration class which will add interceptor
        }

        private void SetCompanyIdInSqlSession()
        {

            var sqlParameter = new SqlParameter("@companyId", companyId);
            this.Database.ExecuteSqlCommand(
                sql: "EXEC SP_SET_SESSION_CONTEXT @key=N'CompanyId' ,@value=@companyId",
                parameters: sqlParameter);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Employee> Employees { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

}
