// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using SampleSupport;
using System;
using System.Globalization;
using System.Linq;
using Task.Data;

// Version Mad01
namespace SampleQueries
{
    [Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{
        private readonly DataSource _dataSource = new DataSource();

        [Category("Linq Operators")]
        [Title("Task 1")]
        [Description("This sample return a list of all customers whose total " +
                     "turnover (the sum of all orders) exceeds some value of X. ")]
        public void Linq1()
        {
            var border = 80000;

            var results = from cust in _dataSource.Customers
                          where cust.Orders.Sum(order => order.Total) > border
                          select new { Customer = cust.CustomerID, Total = cust.Orders.Sum(order => order.Total) };

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 2_1 without grouping")]
        [Description("This sample return a list of suppliers located in the same country " +
                     "and the same city for each client. 2 queries are required here: using grouping and without.")]
        public void Linq2_1()
        {
            var results = from cust in _dataSource.Customers
                          join supl in _dataSource.Suppliers
                              on new { cust.City, cust.Country }
                              equals new { supl.City, supl.Country }
                          select new
                          {
                              Customer = cust.CustomerID,
                              CustCity = cust.City,
                              SuplCity = supl.City,
                              CustCountry = cust.Country,
                              SuplCountry = supl.Country,
                              Supplier = supl.SupplierName
                          };

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

	    [Category("Linq Operators")]
	    [Title("Task 2_2 using grouping")]
	    [Description("This sample return a list of suppliers located in the same country " +
	                 "and the same city for each client. 2 queries are required here: using grouping and without.")]
	    public void Linq2_2()
	    {
	        var results = _dataSource.Customers
	            .GroupJoin(_dataSource.Suppliers,
	                customer => new {customer.Country, customer.City},
	                suplier => new {suplier.Country, suplier.City},
	                (customer, suplier) => new
	                {
	                    customer.CustomerID,
	                    customer.City,
	                    customer.Country,
	                    Supliers = suplier.Select(supliers => new
	                    {
	                        supliers.City,
	                        supliers.Country,
	                        supliers.SupplierName
	                    })
	                })
                .SelectMany(customer => customer.Supliers, (customer, suplier) => new
	            {
	                Customer = customer.CustomerID,
	                CustCity = customer.City,
	                SuplCity = suplier.City,
	                CustCountry = customer.Country,
	                SuplCountry = suplier.Country,
	                Supplier = suplier.SupplierName
                });
	    
	    foreach (var p in results)
	        {
	            ObjectDumper.Write(p);
	        }
	    }

        [Category("Linq Operators")]
        [Title("Task 3")]
        [Description("This sample return a all customers who have orders that exceed the sum of X.")]
        public void Linq3()
        {
            var border = 8000;

            var results = _dataSource.Customers
                .SelectMany(
                    customer => customer.Orders,
                    (customer, order) => new
                    {
                        Customer = customer.CompanyName,
                        SumTotal = order.Total
                    })
                .Where(total => total.SumTotal > border);

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 4")]
        [Description("This sample return a list of customers including the month of " +
                     "the year they became clients (consider client first order month as a required date).")]
        public void Linq4()
        {
            var results =
                from cust in _dataSource.Customers
                where cust.Orders.Length != 0
                select new
                {
                    Customer = cust.CompanyName,
                    StartDate = cust.Orders.Min(order => order.OrderDate)
                };

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 5")]
        [Description("This sample return a list sorted by year, month, customer turnover " +
                     "(from the maximum to the minimum), and the client's name.")]
        public void Linq5()
        {
            var results =
                from customer in (from customer in _dataSource.Customers
                              where customer.Orders.Length != 0
                              select new
                              {
                                  Customer = customer.CompanyName,
                                  StartDate = customer.Orders.Min(order => order.OrderDate)
                              })
                orderby customer.StartDate.Year, customer.StartDate.Month, customer.Customer descending
                select new { customer.Customer, customer.StartDate };

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 6")]
        [Description("This sample return a all customers who have a non-digital postal code, " +
                     "or the region is not filled, " +
                     "or the operator code is not specified in the phone " +
                     "(for simplicity, consider.")]
        public void Linq6()
        {
            var results =
                from cust in _dataSource.Customers
                where !int.TryParse(cust.PostalCode, out var postalCode)
                      || string.IsNullOrEmpty(cust.Region)
                      || !cust.Phone.StartsWith("(")
                select new
                {
                    cust.CustomerID,
                    cust.CompanyName,
                    cust.PostalCode,
                    cust.Region,
                    cust.Phone
                };

            foreach (var p in results)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 7")]
        [Description("This sample return group all products by category, " +
                     "inside - by stock, within the last group sort by cost.")]
        public void Linq7()
        {
            var results = _dataSource.Products.GroupBy(product => product.Category,
                    (category, group) => new
                    {
                        Category = category,
                        StockGroups = group.GroupBy(product => product.UnitsInStock,
                            (stok, units) => new
                            {
                                UnitsInStock = stok,
                                Products = units.OrderBy(cost => cost.UnitPrice)
                            })
                    });

            foreach (var info in results)
            {
                Console.WriteLine($"Category:{info.Category}");
                foreach (var stockGroups in info.StockGroups)
                {
                    Console.WriteLine($"\tUnits in stock:{stockGroups.UnitsInStock}");
                    foreach (var product in stockGroups.Products)
                    {
                        Console.WriteLine($"\t\t{product.ProductName} - \t{product.UnitPrice}");
                    }
                }
            }
        }

        [Category("Linq Operators")]
        [Title("Task 8")]
        [Description("This sample return group the goods into groups (cheap, average price, expensive). " +
                     "The boundaries of each group set yourself.")]
        public void Linq8()
        {
            var results = _dataSource.Products
                .Select(product => new
                {
                    Product = product,
                    CategoryCast = product.UnitPrice < 20
                        ? "Cheap"
                        : product.UnitPrice >= 20 && product.UnitPrice <= 30
                            ? "Average price"
                            : "Expensive"
                })
                .GroupBy(group => group.CategoryCast, (group, products) => new
                {
                    CategoryCast = group,
                    Products = products.Select(product => product.Product)
                });

            foreach (var info in results)
            {
                Console.WriteLine($"Category cast:{info.CategoryCast}");
                foreach (var productInfo in info.Products)
                {
                    Console.WriteLine($"\tProduct:{productInfo.ProductName}");
                }
            }
        }

        [Category("Linq Operators")]
        [Title("Task 9-1")]
        [Description("This sample return group the average profitability of each city " +
                     "(the average amount of the order for all customers from a given city)")]
        public void Linq9_1()
        {
            var results = _dataSource.Customers
                .Select(customer => new
                {
                    City = customer.City,
                    Total = customer.Orders.Sum(order => order.Total),
                    CountOrders = customer.Orders.Count()
                })
                .GroupBy(customer => customer.City, (city, customer) => new
                {
                    City = city,
                    Average = customer.Sum(order => order.Total) / customer.Sum(order => order.CountOrders)
                })
                .OrderBy(cust => cust.City);

            foreach (var info in results)
            {
                Console.WriteLine($"City:{info.City}\t\t Average profitability:{info.Average: #.#}");
            }
        }

        [Category("Linq Operators")]
        [Title("Task 9-2")]
        [Description("This sample return the average intensity " +
                     "(the average number of orders per customer from each city).")]
        public void Linq9_2()
        {
            var results = _dataSource.Customers
                .Select(customer => new
                {
                    customer.City,
                    OrderCount = customer.Orders.Count()
                })
                .GroupBy(customer => customer.City, (city, customer) => new
                {
                    City = city,
                    Average = customer.Sum(order => order.OrderCount) / _dataSource.Customers.Count(cust => cust.City == city)
                })
                .OrderBy(cust => cust.City);

            foreach (var info in results)
            {
                Console.WriteLine($"City:{info.City}\t\t Average intensity:{info.Average : #.#}");
            }
        }

        [Category("Linq Operators")]
        [Title("Task 10-1")]
        [Description("This sample return the the average annual activity " +
                     "statistics of clients by month (excluding the year).")]
        public void Linq10_1()
        {
            var results = _dataSource.Customers
                .SelectMany(customer => customer.Orders, (customer, order) => new
                {
                    order.OrderDate.Month,
                    OrdersCount = customer.Orders.Count()
                })
                .GroupBy(orders => orders.Month, (mounth, group) => new 
                {
                    Mounth = mounth,
                    TotalOrders = group.Sum(order => order.OrdersCount)
                })
                .OrderBy(order => order.Mounth);

            Console.WriteLine("Month \tTotal Orders");
            foreach (var info in results)
            {
                Console.WriteLine($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(info.Mounth)} \t{info.TotalOrders}");
            }
        }

        [Category("Linq Operators")]
        [Title("Task 10-2")]
        [Description("This sample return the average annual activity statistics by year")]
        public void Linq10_2()
        {
            var results = _dataSource.Customers
                .SelectMany(customer => customer.Orders, (customer, order) => new
                {
                    order.OrderDate.Year,
                    OrdersCount = customer.Orders.Count()
                })
                .GroupBy(customer => customer.Year, (year, customer) => new
                {
                    Year = year,
                    TotalOrders = customer.Sum(order => order.OrdersCount)
                })
                .OrderBy(customer => customer.Year);

            Console.WriteLine("Year \tTotal Orders");
            foreach (var info in results)
            {
                Console.WriteLine($"{info.Year} \t{info.TotalOrders}");
            }
        }

	    [Category("Linq Operators")]
	    [Title("Task 10-3")]
	    [Description("This sample return the average annual activity statistics by year and mounth")]
        public void Linq10_3()
	    {
	        var results = _dataSource.Customers
	            .SelectMany(customer => customer.Orders, (customer, order) => new
	            {
	                Year = order.OrderDate.Year,
	                Month = order.OrderDate.Month,
	                Order = order
	            })
	            .GroupBy(customer => customer.Year, (year, group) => new
	            {
	                Year = year,
	                Months = group.GroupBy(orders => orders.Month, (month, orders) => new
	                {
	                    Month = month,
	                    TotalOrders = orders.Count()
	                })
	                .OrderBy(order => order.Month)
	            })
                .OrderBy(x => x.Year);

            foreach (var yearInfo in results)
	        {
	            Console.WriteLine($"Year: {yearInfo.Year}");
	            foreach (var month in yearInfo.Months)
	            {
	                Console.WriteLine($"\tMonth: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Month)} \tTotal orders: {month.TotalOrders}");
	            }
	        }
        }
    }
}
