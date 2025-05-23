using Predobro.Models;
using Microsoft.AspNetCore.Identity;

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
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                
                var store1 = new ApplicationUser
                {
                    UserName = "store1@example.com",
                    Email = "store1@example.com",
                    NormalizedEmail = "STORE1@EXAMPLE.COM",
                    NormalizedUserName = "STORE1@EXAMPLE.COM",
                    FullName = "Store One",
                    Address = "Cankarjeva cesta 16",
                    PhoneNumber = "111-111-1111",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                store1.PasswordHash = passwordHasher.HashPassword(store1, "Store123!");
                context.Users.Add(store1);

                var store2 = new ApplicationUser
                {
                    UserName = "store2@example.com",
                    Email = "store2@example.com",
                    NormalizedEmail = "STORE2@EXAMPLE.COM",
                    NormalizedUserName = "STORE2@EXAMPLE.COM",
                    FullName = "Store Two",
                    Address = "Bohoričeva ulica 3",
                    PhoneNumber = "222-222-2222",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                store2.PasswordHash = passwordHasher.HashPassword(store2, "Store456!");
                context.Users.Add(store2);

                // Add customer user - Big boi
                var customer1 = new ApplicationUser
                {
                    UserName = "bigboi@example.com",
                    Email = "bigboi@example.com",
                    NormalizedEmail = "BIGBOI@EXAMPLE.COM",
                    NormalizedUserName = "BIGBOI@EXAMPLE.COM",
                    FullName = "Big boi",
                    Address = "Slovenska cesta 100",
                    PhoneNumber = "555-555-5555",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                customer1.PasswordHash = passwordHasher.HashPassword(customer1, "Bigboi123!");
                context.Users.Add(customer1);

                context.SaveChanges();

                // Get the role IDs
                var storeRole = context.Roles.FirstOrDefault(r => r.Name == "Store");
                var customerRole = context.Roles.FirstOrDefault(r => r.Name == "Customer");

                // Assign roles to users
                if (storeRole != null)
                {
                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = store1.Id,
                        RoleId = storeRole.Id
                    });

                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = store2.Id,
                        RoleId = storeRole.Id
                    });
                }

                if (customerRole != null)
                {
                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = customer1.Id,
                        RoleId = customerRole.Id
                    });
                }

                // Now seed items using the actual Ids
                var items = new Item[]
                {
                    new Item { 
                        Name = "Fresh Bread", 
                        Description = "Baked this morning.", 
                        Price = 2.99M, 
                        StoreId = store1.Id,
                        Quantity = 10
                    },
                    new Item { 
                        Name = "Organic Apples", 
                        Description = "From local farm.", 
                        Price = 1.49M, 
                        StoreId = store2.Id,
                        Quantity = 25
                    },
                    new Item { 
                        Name = "Cheese", 
                        Description = "Aged cheddar.", 
                        Price = 4.50M, 
                        StoreId = store1.Id,
                        Quantity = 8
                    }
                };

                context.Items.AddRange(items);
                context.SaveChanges();
            }
        }
    }
}