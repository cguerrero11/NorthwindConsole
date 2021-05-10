using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");                    
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add Product");
                    Console.WriteLine("6) Display products");
                    Console.WriteLine("7) Edit products");
                    Console.WriteLine("8) Edit categories");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new NWConsole_96_CFGContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        var db = new NWConsole_96_CFGContext();
                        Categories category = InputCategories(db);
                        db.AddCategory(category);
                    }
                    else if (choice == "3")
                    {
                        var db = new NWConsole_96_CFGContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Categories category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Products p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        var db = new NWConsole_96_CFGContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Products p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "5"){
                        //add product
                        var db = new NWConsole_96_CFGContext();
                        Products product = InputProduct(db);
                        if (product != null)
                        {
                            db.AddProduct(product);
                            logger.Info("Product added - {name}", product.ProductName);
                        }
                    }
                    else if (choice == "6"){
                //filter only discontinued TODO
                //display all content for one product TODO
                        var db = new NWConsole_96_CFGContext();
                        Console.WriteLine("Choose an option: ");
                        Console.WriteLine("1) Display all active products");
                        Console.WriteLine("2) Display all products");
                        Console.WriteLine("3) Display only discontinued products");
                        Console.WriteLine("4) Display info on a specific product");
                        string input = Console.ReadLine();
                        GetProducts(db, input);
                    }
                    else if (choice == "7"){
                        // edit products
                        var db = new NWConsole_96_CFGContext();
                        Console.WriteLine("Choose a product to edit:");
                        var query = db.Products.OrderBy(p => p.ProductId);

                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductId}) {item.ProductName}");
                        }

                        if (int.TryParse(Console.ReadLine(), out int ProductId))
                        {
                            logger.Info($"ProductId {ProductId} selected");
                            Products product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);

                            if (product != null)
                            {
                                // input product
                                Products UpdatedProduct = InputProduct(db);
                                if (UpdatedProduct != null)
                                {
                                    UpdatedProduct.ProductId = product.ProductId;
                                    db.EditProduct(UpdatedProduct);
                                    logger.Info($"Product (id: {product.ProductId}) updated");
                                }
                            }
                        }
                        
                    }
                    else if (choice == "8"){
                        var db = new NWConsole_96_CFGContext();
                        // edit categories
                        Console.WriteLine("Choose a category to edit:");
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
     
                        if (int.TryParse(Console.ReadLine(), out int CategoryId))
                        {
                            logger.Info($"CategoryId {CategoryId} selected");
                            Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);

                            if (category != null)
                            {
                                // input category
                                Categories UpdatedCategory = InputCategories(db);
                                if (UpdatedCategory != null)
                                {
                                    UpdatedCategory.CategoryId = category.CategoryId;
                                    db.EditCategory(UpdatedCategory);
                                    logger.Info($"Category (id: {category.CategoryId}) updated");
                                }
                            }
                        // GetCategories(db);
                        }
                    }
                    Console.WriteLine();

                    } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Products InputProduct(NWConsole_96_CFGContext db){
            Products product = new Products();
            Console.Write("Enter Product name: ");
            product.ProductName = Console.ReadLine();
            Console.Write("Enter the quantity per unit: ");
            product.QuantityPerUnit = Console.ReadLine();
            Console.Write("Enter unit price: ");
            product.UnitPrice = Decimal.Parse(Console.ReadLine());

            Console.Write("Enter amount of units in stock: ");
            product.UnitsInStock = Int16.Parse(Console.ReadLine());
            Console.Write("Enter amount of units on order: ");
            product.UnitsOnOrder = Int16.Parse(Console.ReadLine());
            Console.Write("Enter reorder level: ");
            product.ReorderLevel = Int16.Parse(Console.ReadLine());

            Console.Write("Product discontinued?(y/n)");
            string disc = Console.ReadLine();
            bool discontinued = false;
            if (disc.ToLower() == "y")
            {
                discontinued = true;
            }
            product.Discontinued = discontinued;
            //select catagory
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category this product belongs to:");
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            int id = int.Parse(Console.ReadLine());
            logger.Info($"CategoryId {id} selected");

            Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"The category {category.CategoryName} was selected.");
            product.Category = category;
            product.CategoryId = category.CategoryId;
            //select supplier
            var query2 = db.Suppliers.OrderBy(p => p.SupplierId);
            //int pid = 0;
            Console.WriteLine("Select the supplier of this product:");
            foreach (var item in query2)
            {
                Console.WriteLine($"{item.SupplierId}) {item.CompanyName}");

            }
            id = int.Parse(Console.ReadLine());
            logger.Info($"SupplierId {id} selected");

            Suppliers supplier = db.Suppliers.FirstOrDefault(c => c.SupplierId == id);
            Console.WriteLine($"The supplier {supplier.SupplierId} was selected.");

            product.Supplier = supplier;
            product.SupplierId = supplier.SupplierId;
//

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();
            
            var isValid = Validator.TryValidateObject(product, context, results, true);
            
            if (isValid)
            {
                logger.Info("Validation passed");
                
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }

            return product;
        }

        public static Products GetProducts(NWConsole_96_CFGContext db, string input)
        {
            // display all products
            var products = db.Products.OrderBy(p => p.ProductId);
            if(input == "1"){
                Console.WriteLine("All active products: ");
                foreach (Products p in products){
                    if (db.Products.Any(p => p.Discontinued == false)){
                        
                        Console.WriteLine($"{p.ProductName}");
                    }
                }

            } else if(input == "2") { 
                //all products
                Console.WriteLine("All products (discontinued products in red):");
                foreach (Products p in products)
                {
                    if(p.Discontinued == false){
                        
                        Console.WriteLine($"{p.ProductName}");
                    } else {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{p.ProductName}");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                }
            } else if(input == "3"){
                Console.WriteLine("All discontinued products: ");
                foreach (Products p in products)
                {
                    if(p.Discontinued == true){
                        Console.WriteLine($"{p.ProductName}");
                    }
                }
                        
            } else if(input == "4"){
                // Display info on a specific product
                Console.WriteLine("Select a product to view: ");
                var query = db.Products.OrderBy(p => p.ProductId);
                foreach (var item in query)
                {
                    Console.WriteLine($"{item.ProductId}) {item.ProductName}");
                }
                int id = int.Parse(Console.ReadLine());
                Products product = db.Products.FirstOrDefault(p => p.ProductId == id);

                Console.WriteLine($"Product name: {product.ProductName}");
                Suppliers supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == product.SupplierId);
                Console.WriteLine($"Supplier: {supplier.SupplierId}");
                Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
                Console.WriteLine($"Category: {category.CategoryId}");
                Console.WriteLine($"Quanity per Unit: {product.QuantityPerUnit}");
                Console.WriteLine($"Unit Price: ${product.UnitPrice}");
                Console.WriteLine($"Units in Stock: {product.UnitsInStock}");
                Console.WriteLine($"Units on Order: {product.UnitsOnOrder}");
                Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
                Console.WriteLine($"Discontinued: {product.Discontinued}");

            } else {
                Console.WriteLine("Invalid input.");
            }
            return null;
        }

        public static Categories InputCategories(NWConsole_96_CFGContext db){
            Categories category = new Categories();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();

            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {

                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    // db.AddCategory(category);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
            return category;

        }
        public static Categories GetCategories(NWConsole_96_CFGContext db){
            var categories = db.Categories.OrderBy(c => c.CategoryId);
            foreach (Categories c in categories)
            {
                Console.WriteLine($"{c.CategoryName}: {c.Description}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Category Id");
            return null;
        }
    }
}