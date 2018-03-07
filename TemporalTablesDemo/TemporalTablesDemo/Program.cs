using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using TemporalTablesDemo.Models;

namespace TemporalTablesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"Products as of {now}:");
            using (var ctx = new ProductsContext())
            {
                var products = ctx.GetProductsAsOf(now).ToList();
                foreach (var p in products)
                {
                    Console.WriteLine($"Id: {p.Id}. Name: {p.Name}. Quantity: {p.Quantity}. Price: {p.Price}");
                }
            }

            Console.ReadKey();
        }
    }
}
