using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class HouseholdFullException : DomainException
    {
        public HouseholdFullException() : base("Impossible de joindre le foyer car il est complet (10 membres)") { }
    }
}
