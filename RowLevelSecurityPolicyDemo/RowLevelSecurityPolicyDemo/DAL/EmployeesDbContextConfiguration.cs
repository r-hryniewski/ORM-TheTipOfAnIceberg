using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;

namespace RowLevelSecurityPolicyDemo.DAL
{
    internal class EmployeesDbContextConfiguration : DbConfiguration
    {
        public EmployeesDbContextConfiguration()
        {
            //Uncomment this line to try out interceptor. Be warned tat this will overwrite session context you've set both in constructor and StateChanged event as interceptor executes AFTER those
            //AddInterceptor(new SqlSessionStateInterceptor());
        }
    }

    internal class SqlSessionStateInterceptor : IDbCommandInterceptor
    {
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            SetCompanyIdInSqlSessionState(command);
        }

        private void SetCompanyIdInSqlSessionState(DbCommand command)
        {
            var companyId = GetCurrentCompanyIdSomehow();

            var setSessionStateSql = "EXEC sp_set_session_context @key=N'CompanyId', @value=@CompanyIdFromEFInterceptor ;";

            //Append constructed command BEFORE anything you're executing in actual command
            command.CommandText = setSessionStateSql + command.CommandText;
            command.Parameters.Insert(0, new SqlParameter("@CompanyIdFromEFInterceptor", companyId));
        }

        private long GetCurrentCompanyIdSomehow()
        {

            return 1;
        }
    }
}