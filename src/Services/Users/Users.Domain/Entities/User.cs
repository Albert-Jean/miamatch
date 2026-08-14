using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class User
    {
        public Guid Id { get; }
        public string Name {  get; }
        public  DateTime CreatedAt {  get; }
        public Email Email {  get; }
        public string PasswordHash {  get; }
        private User(Guid id,string name,DateTime createdAt,Email email, string passwordHash)
        {
            Id = id;
            Name = name;
            CreatedAt = createdAt;
            Email = email;
            PasswordHash = passwordHash;
        }
        public static User Create(string name, string email, string passwordHash)
        {
            Guid id = Guid.NewGuid();
            DateTime dateTime = DateTime.UtcNow;
            Email validEmail = Email.Create(email);
            return new User(id, name,dateTime, validEmail, passwordHash);
        }
    }
}
