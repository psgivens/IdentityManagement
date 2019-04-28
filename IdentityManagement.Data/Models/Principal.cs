
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.Data.Models
{
    public class Principal
    {
        [Key]
        public System.Guid Id { get; set; }
        public IList<GroupPrincipalMap> EncapsulatingGroups { get; set; }
    }

    public class User : Principal
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class Group : Principal
    {
        public string Name { get; set; }
        public IList<GroupPrincipalMap> MemberRelations { get; set; }
    }

    public class GroupPrincipalMap
    {
        public System.Guid PrincipalId { get; set; }
        public Principal Principal { get; set; }

        public System.Guid GroupId { get; set; }
        public Group Group { get; set; }
    }


    public class Role
    {
        [Key]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public IList<Principal> Members { get; set; }
    }
}