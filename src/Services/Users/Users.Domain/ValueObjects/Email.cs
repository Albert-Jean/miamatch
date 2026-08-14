using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Exceptions;

namespace Users.Domain.ValueObjects
{
    public sealed record Email
    {
        public string EmailAddress { get; }
        private Email(string email)
        {
            EmailAddress = email;
        }
        public static Email Create(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }
            if(MailAddress.TryCreate(email, out var _))
            {
                return new Email(email.Trim().ToLowerInvariant());
            }
            else
            {
                throw new InvalidEmailException(email);
            }
        }
    }
}
