using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Recipes.Infrastructure.Persistence
{
    public class RecipesDbContextFactory: IDesignTimeDbContextFactory<RecipesDbContext>
    {
        public RecipesDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RecipesDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=miamatch;Username=miamatch;Password=miamatch");

            return new RecipesDbContext(optionsBuilder.Options);
        }
    }
}
