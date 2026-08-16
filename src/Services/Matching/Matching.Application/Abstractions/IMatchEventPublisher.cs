using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matching.Domain.Entities;

namespace Matching.Application.Abstractions
{
    public interface IMatchEventPublisher
    {
        Task PublishMatchCreatedAsync(Match match);
    }
}
