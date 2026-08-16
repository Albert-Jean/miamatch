using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Exceptions;

namespace Users.Domain.ValueObjects
{
    public sealed record InviteCode
    {
        public string Value { get; }
        private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public static InviteCode Generate()
        {
            string code=string.Empty;
            for(int  i = 0; i <6;i++)
            {
                code+= AllowedChars[Random.Shared.Next(AllowedChars.Length)];
            }
            return new InviteCode(code);
        }
        public static InviteCode From(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new InvalidInviteCodeException(code);
            }
            code = code.ToUpperInvariant();
            if (code.Length != 6)
            {
                throw new InvalidInviteCodeException(code);
            }
            if (!code.All(c => AllowedChars.Contains(c)))
            {
                throw new InvalidInviteCodeException(code);
            }
            else
            {
                return new InviteCode(code);
            }
        }
        private InviteCode(string code) { Value = code; }
    }
}
