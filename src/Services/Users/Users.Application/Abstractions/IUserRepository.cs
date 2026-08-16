using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(Email email);
        Task<User?> GetByIdAsync(Guid id);
        Task AddAsync(User user);
    }
}
