using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShoppingList.Infrastructure.Persistence
{
    public class ShoppingListDbContextFactory : IDesignTimeDbContextFactory<ShoppingListDbContext>
    {
        public ShoppingListDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShoppingListDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=miamatch;Username=miamatch;Password=miamatch");
            return new ShoppingListDbContext(optionsBuilder.Options);
        }    
    }
}
