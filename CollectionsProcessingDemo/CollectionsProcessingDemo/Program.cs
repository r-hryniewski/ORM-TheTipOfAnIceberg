using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CollectionsProcessingDemo.DAL;

namespace CollectionsProcessingDemo
{
    class Program
    {
        private static readonly Random random = new Random();
        private static readonly Guid[] employeesIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        private static readonly DateTime now = DateTime.UtcNow;
        static void Main(string[] args)
        {
            for (int i = 1; i <= 5; i++) //Code in loop to warm up and compile and cache query plans
            {
                Console.Clear();
                Console.WriteLine($"Iteration {i}");

                //Console.WriteLine($"Executing method: {nameof(ProcessInMemoryGeneratedCollection)}");
                //ProcessInMemoryGeneratedCollection();

                //Console.WriteLine($"Executing method: {nameof(ProcessCollectionFetchedEntirellyFromDatabase)}");
                //ProcessCollectionFetchedEntirellyFromDatabase();

                //Console.WriteLine($"Executing method: {nameof(ProcessCollectionFetchedEntirellyFromDatabaseAsNoTracking)}");
                //ProcessCollectionFetchedEntirellyFromDatabaseAsNoTracking();

                //Console.WriteLine($"Executing method: {nameof(ProcessCollectionFetchedEntirellyFromDatabaseWithWhereInIQueryable)}");
                //ProcessCollectionFetchedEntirellyFromDatabaseWithWhereInIQueryable();

                //Console.WriteLine($"Executing method: {nameof(ProcessCollectionFetchedEntirellyFromDatabaseWithWhereAndSelectInIQueryable)}");
                //ProcessCollectionFetchedEntirellyFromDatabaseWithWhereAndSelectInIQueryable();

                Console.WriteLine($"Executing method: {nameof(ExecuteRawSqlQuery)}");
                ExecuteRawSqlQuery();

                Console.WriteLine($"Executing method: {nameof(ExecuteRawSqlQueryAgainstIndexedDatabase)}");
                ExecuteRawSqlQueryAgainstIndexedDatabase();

                Console.WriteLine($"Executing method: {nameof(ExecuteRawSqlQueryAgainstIndexedDatabaseWithComputedColumn)}");
                ExecuteRawSqlQueryAgainstIndexedDatabaseWithComputedColumn();

                Console.WriteLine($"Executing method: {nameof(ExecuteRawSqlQueryAgainstIndexedView)}");
                ExecuteRawSqlQueryAgainstIndexedView();
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void ProcessInMemoryGeneratedCollection()
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting generating 10 000 000 sales in memory---");
            var sales = Enumerable.Range(1, 10000000)
                .Select(n => new Sale
                {
                    Id = Guid.NewGuid(),
                    Amount = random.Next(10, 100),
                    DateTime = new DateTime(2018, now.Month, n % 28 + 1, n % 24, n % 60, 0),
                    EmployeeId = employeesIds[random.Next(0, employeesIds.Length)]
                })
                .ToArray();

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Generated {sales.Length} sales, proceeding to generating report---");
            var employeedId = employeesIds[0];

            var salesGroupedByDay = sales
                .Where(s => s.EmployeeId == employeedId)
                .GroupBy(s => s.DateTime.Date)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(s => s.Amount).Sum());

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping of 10 000 000 sales, proceeding to writing results---");
            foreach (var entry in salesGroupedByDay.OrderBy(s => s.Key))
            {
                Console.WriteLine($"Sales for day {entry.Key:yyyy MMMM dd} - Sum: {entry.Value}");
            }
            sw.Stop();
            Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
        }

        private static void ProcessCollectionFetchedEntirellyFromDatabase()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sales = ctx.RawSales
                    .ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Fetched {sales.Count} sales, proceeding to generating report---");

                var salesGroupedByDay = sales
                    .Where(s => s.EmployeeId == someEmployeeId)
                    .GroupBy(s => s.DateTime.Date)
                    .ToDictionary(g => g.Key, g => g.Select(s => s.Amount).Sum());

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay.OrderBy(s => s.Key))
                {
                    //Console.WriteLine($"Sales for day {entry.Key:yyyy MMMM dd} - Sum: {entry.Value}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static void ProcessCollectionFetchedEntirellyFromDatabaseAsNoTracking()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sales = ctx.RawSales
                    .AsNoTracking()
                    .ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Fetched {sales.Count} sales, proceeding to generating report---");

                var salesGroupedByDay = sales
                    .Where(s => s.EmployeeId == someEmployeeId)
                    .GroupBy(s => s.DateTime.Date)
                    .ToDictionary(g => g.Key, g => g.Select(s => s.Amount).Sum());

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay.OrderBy(s => s.Key))
                {
                    //Console.WriteLine($"Sales for day {entry.Key:yyyy MMMM dd} - Sum: {entry.Value}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static void ProcessCollectionFetchedEntirellyFromDatabaseWithWhereInIQueryable()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sales = ctx.RawSales
                    .AsNoTracking()
                    .Where(s => s.EmployeeId == someEmployeeId)
                    .ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Fetched {sales.Count} sales, proceeding to generating report---");

                var salesGroupedByDay = sales
                    .GroupBy(s => s.DateTime.Date)
                    .ToDictionary(g => g.Key, g => g.Select(s => s.Amount).Sum());

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay.OrderBy(s => s.Key))
                {
                    //Console.WriteLine($"Sales for day {entry.Key:yyyy MMMM dd} - Sum: {entry.Value}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static void ProcessCollectionFetchedEntirellyFromDatabaseWithWhereAndSelectInIQueryable()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sales = ctx.RawSales
                    .AsNoTracking()
                    .Where(s => s.EmployeeId == someEmployeeId)
                    .Select(s => new
                    {
                        s.DateTime,
                        s.Amount
                    })
                    .ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Fetched {sales.Count} sales, proceeding to generating report---");

                var salesGroupedByDay = sales
                    .GroupBy(s => s.DateTime.Date)
                    .ToDictionary(g => g.Key, 
                        g => g.Select(s => s.Amount).Sum());

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay.OrderBy(s => s.Key))
                {
                    //Console.WriteLine($"Sales for day {entry.Key:yyyy MMMM dd} - Sum: {entry.Value}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }
        private static void ExecuteRawSqlQuery()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sql =
                    @"SELECT 
	                    CONVERT(date, [DateTime]) AS SalesDate, 
	                    SUM([Amount]) AS AmountSum
                    FROM [dbo].[RawSales]
                    WHERE [EmployeeId] = @employeeId
                    GROUP BY CONVERT(date, [DateTime])
                    ORDER BY SalesDate";

                var salesGroupedByDay =
                    ctx.Database.SqlQuery<ReportEntry>(sql, new SqlParameter("employeeId", someEmployeeId)).ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay)
                {
                    //Console.WriteLine($"Sales for day {entry.SalesDate:yyyy MMMM dd} - Sum: {entry.AmountSum}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static void ExecuteRawSqlQueryAgainstIndexedDatabase()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sql =
                    @"SELECT 
	                    CONVERT(date, [DateTime]) AS SalesDate, 
	                    SUM([Amount]) AS AmountSum
                    FROM [dbo].[IndexedSales]
                    WHERE [EmployeeId] = @employeeId
                    GROUP BY CONVERT(date, [DateTime])
                    ORDER BY SalesDate";

                var salesGroupedByDay =
                    ctx.Database.SqlQuery<ReportEntry>(sql, new SqlParameter("employeeId", someEmployeeId)).ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay)
                {
                    //Console.WriteLine($"Sales for day {entry.SalesDate:yyyy MMMM dd} - Sum: {entry.AmountSum}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }
        private static void ExecuteRawSqlQueryAgainstIndexedDatabaseWithComputedColumn()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sql =
                    @"SELECT 
	                    [Date] AS SalesDate, 
	                    SUM([Amount]) AS AmountSum
                    FROM [dbo].[IndexedSalesWithComputedColumn]
                    WHERE [EmployeeId] = @employeeId
                    GROUP BY [Date]
                    ORDER BY SalesDate";

                var salesGroupedByDay =
                    ctx.Database.SqlQuery<ReportEntry>(sql, new SqlParameter("employeeId", someEmployeeId)).ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay)
                {
                    //Console.WriteLine($"Sales for day {entry.SalesDate:yyyy MMMM dd} - Sum: {entry.AmountSum}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static void ExecuteRawSqlQueryAgainstIndexedView()
        {
            using (var ctx = new SalesContext())
            {
                var someEmployeeId = ctx.RawSales.AsNoTracking().Select(s => s.EmployeeId).FirstOrDefault();
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Starting fetching sales from db---");

                var sql = @"SELECT 
                        [SalesDate]
                        ,[AmountSum]
                    FROM [dbo].[Vw_SalesReport] 
	                    WITH (NOEXPAND)
                    WHERE [EmployeeId] = @employeeId
                    ORDER BY [SalesDate]";

                var salesGroupedByDay =
                    ctx.Database.SqlQuery<ReportEntry>(sql, new SqlParameter("employeeId", someEmployeeId)).ToList();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms:---Finished grouping, proceeding to writing results---");
                foreach (var entry in salesGroupedByDay)
                {
                    //Console.WriteLine($"Sales for day {entry.SalesDate:yyyy MMMM dd} - Sum: {entry.AmountSum}");
                }
                sw.Stop();
                Console.WriteLine($"Entire operation took {sw.ElapsedMilliseconds}ms");
            }
        }
    }

    public class ReportEntry
    {
        public DateTime SalesDate { get; set; }
        public int AmountSum { get; set; }
    }

    public interface ISale
    {
        Guid Id { get; }
        int Amount { get; }
        DateTime DateTime { get; }
        Guid EmployeeId { get; }
    }
    public class Sale : ISale
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public DateTime DateTime { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
