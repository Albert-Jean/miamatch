using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder) 
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(u => u.Name).HasColumnName("name").IsRequired();
            builder.Property(u => u.Email)
    .HasConversion(
        email => email.EmailAddress,
        value => Email.Create(value))
    .HasColumnName("email")
    .IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
