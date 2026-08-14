using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Entities
{
    public sealed record HouseholdMember(Guid UserId, DateTime JoinedAt);
}
