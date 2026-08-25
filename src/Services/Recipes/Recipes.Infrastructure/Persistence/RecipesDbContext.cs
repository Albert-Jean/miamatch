using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Recipe.Domain.Entities;
using Recipes.Domain.Entities;

namespace Recipes.Infrastructure.Persistence
{
    public class RecipesDbContext : DbContext
    {
        public RecipesDbContext(DbContextOptions<RecipesDbContext> options) : base(options) { }
        public DbSet<Recipes.Domain.Entities.Recipe> Recipes { get; set; }
        public DbSet<Deck> Decks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("recipes");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipesDbContext).Assembly);
        }
    }
}
