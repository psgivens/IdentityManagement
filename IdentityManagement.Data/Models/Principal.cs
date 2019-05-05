  
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.Data.Models
{
    public class Principal
    {
        [Key]
        public System.Guid Id { get; set; }
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
        public IList<GroupPrincipalMap> MemberRelations { get; set; } = new List<GroupPrincipalMap> ();
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
        public System.Guid ExternalId { get; set; }
        public string Name { get; set; }
        public IList<RolePrincipalMap> Members { get; set; } = new List<RolePrincipalMap>();
    }

    public class RolePrincipalMap
    {
        public System.Guid PrincipalId { get; set; }
        public Principal Principal { get; set; }

        public System.Guid RoleId { get; set; }
        public Role Role { get; set; }
    }

}