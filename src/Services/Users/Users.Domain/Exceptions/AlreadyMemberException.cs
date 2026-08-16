using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class AlreadyMemberException : DomainException
    {
        public AlreadyMemberException(Guid id) : base($"L'utilisateur {id} fait déjà parti du foyer") { }
    }
}
