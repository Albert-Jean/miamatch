using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Users.Application.Abstractions
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, IEnumerable<Guid> householdIds);
    }
}
