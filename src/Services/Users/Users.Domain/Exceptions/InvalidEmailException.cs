using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class InvalidEmailException: DomainException
    {
        public InvalidEmailException(string email) : base($"L'adresse email '{email}' n'est pas valide.") { }
    }
}
