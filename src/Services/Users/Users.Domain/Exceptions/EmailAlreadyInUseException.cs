using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class EmailAlreadyInUseException : DomainException
    {
        public EmailAlreadyInUseException(string email) : base($"L'adresse email '{email}' est déjà utilisée.") { }
    }
}
