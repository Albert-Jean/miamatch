using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Exceptions
{
    public class InvalidInviteCodeException :DomainException
    {
        public InvalidInviteCodeException(string code) : base($"Le code saisi '{code}' n'est pas valide.") { }
    }
}
