using System;
using System.Collections.Generic;
using System.Text;
using Matching.Domain.Entities;

namespace Matching.Domain.Services
{
    public static class MatchEvaluator
    {
        public static bool IsMatch(IEnumerable<Swipe> swipes)
        {

           return swipes.Where(s=>s.Liked).Select(s=>s.UserId).Distinct().Count() >= 2;

        }
    }
}
