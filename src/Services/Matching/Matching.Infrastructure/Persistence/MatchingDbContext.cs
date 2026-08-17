using System;
using System.Collections.Generic;
using System.Text;
using Matching.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Matching.Infrastructure.Persistence
{
    public class MatchingDbContext : DbContext
    {
        public MatchingDbContext(DbContextOptions<MatchingDbContext> options) : base(options) { }
        public DbSet<Swipe> Swipes { get; set; }
        public DbSet<Match> Matches { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("matching");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MatchingDbContext).Assembly);
        }
    }
}
