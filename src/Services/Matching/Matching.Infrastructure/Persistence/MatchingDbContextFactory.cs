using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Matching.Infrastructure.Persistence
{
    public class MatchingDbContextFactory: IDesignTimeDbContextFactory<MatchingDbContext>
    {
        public MatchingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MatchingDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=miamatch;Username=miamatch;Password=miamatch");

            return new MatchingDbContext(optionsBuilder.Options);
        }
    }
}
