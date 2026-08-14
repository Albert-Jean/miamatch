using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class HouseHoldNotFoundException : DomainException
    {
        public HouseHoldNotFoundException() : base("Code d'invitation ne correspond à aucun foyer") { }
    }
}
