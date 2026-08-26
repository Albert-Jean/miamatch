using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShoppingList.Domain.Entities;

namespace ShoppingList.Infrastructure.Persistence
{
    public class ShoppingListDbContext : DbContext
    {
        public ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : base(options) { }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("shoppinglist");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShoppingListDbContext).Assembly);
        }  

    }
}
