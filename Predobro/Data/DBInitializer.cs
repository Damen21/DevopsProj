using Predobro.Models;

namespace Predobro.Data
{
    public static class DBInitializer
    {
        public static void Initialize(FoodContext context)
        {
            context.Database.EnsureCreated();

            // Seed stores (users)
            if (!context.Users.Any(u => u.UserName == "store1@example.com"))
            {
                var store1 = new ApplicationUser
                {
                    UserName = "store1@example.com",
                    Email = "store1@example.com",
                    FullName = "Store One",
                    Address = "123 Store St",
                    PhoneNumber = "111-111-1111"
                };
                context.Users.Add(store1);

                var store2 = new ApplicationUser
                {
                    UserName = "store2@example.com",
                    Email = "store2@example.com",
                    FullName = "Store Two",
                    Address = "456 Store Ave",
                    PhoneNumber = "222-222-2222"
                };
                context.Users.Add(store2);

                context.SaveChanges();

                // Now seed items using the actual Ids
                var items = new Item[]
                {
                    new Item { 
                        Name = "Fresh Bread", 
                        Description = "Baked this morning.", 
                        Price = 2.99M, 
                        StoreId = store1.Id,
                        Quantity = 10 // Adding quantity
                    },
                    new Item { 
                        Name = "Organic Apples", 
                        Description = "From local farm.", 
                        Price = 1.49M, 
                        StoreId = store2.Id,
                        Quantity = 25 // Adding quantity
                    },
                    new Item { 
                        Name = "Cheese", 
                        Description = "Aged cheddar.", 
                        Price = 4.50M, 
                        StoreId = store1.Id,
                        Quantity = 8 // Adding quantity
                    }
                };

                context.Items.AddRange(items);
                context.SaveChanges();
            }
        }
    }
}