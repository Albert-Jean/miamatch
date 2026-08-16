using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class Household
    {
        public Guid Id { get; }
        public string Name { get; }
        public InviteCode InviteCode { get; }
        public DateTime CreatedAt { get; }

        private readonly List<HouseholdMember> _members;

        public IReadOnlyCollection<HouseholdMember> Members => _members.AsReadOnly();
        private Household(Guid id, string name, InviteCode inviteCode, DateTime createdAt)
        {
            Id = id;
            Name = name;
            InviteCode = inviteCode;
            CreatedAt = createdAt;
            _members = new List<HouseholdMember>();
        }
        private Household(Guid id, string name, InviteCode inviteCode, DateTime createdAt, List<HouseholdMember> members)
        {
            Id = id;
            Name = name;
            InviteCode = inviteCode;
            CreatedAt = createdAt;
            _members = members;
        }

        public static Household Create(string name, Guid creatorUserID)
        {
            Guid houseHoldID = Guid.NewGuid();
            InviteCode inviteCode = InviteCode.Generate();
            DateTime createdAt = DateTime.UtcNow;
            List<HouseholdMember> householdMembers = new List<HouseholdMember>();
            householdMembers.Add(new HouseholdMember(creatorUserID, createdAt));
            return new Household(houseHoldID, name, inviteCode, createdAt, householdMembers);
        }

        public void AddMember(Guid userId)
        {
            if (Members.Count >= 10)
            {
                throw new HouseholdFullException();
            }
            if (_members.Any(m=>m.UserId == userId)){
                throw new AlreadyMemberException(userId);
            }
            else
            {
                _members.Add(new HouseholdMember(userId, DateTime.UtcNow));
            }
        }
    }
}
