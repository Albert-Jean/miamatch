using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Application.Abstractions
{
    public interface IHouseholdMembersClient
    {
        Task<IReadOnlyCollection<Guid>> GetMembersAsync(Guid householdId);
    }
}
