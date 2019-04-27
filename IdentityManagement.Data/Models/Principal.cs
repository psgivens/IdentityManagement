using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityManagement.Models
{
    public class Principal
    {
        [Key]
        public virtual long Id { get; set; }
        public virtual IList<GroupPrincipalMap> EncapsulatingGroups { get; set; }
    }

    public class User : Principal
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }

    public class Group : Principal
    {
        public virtual string Name { get; set; }
        public virtual IList<GroupPrincipalMap> MemberRelations { get; set; }
    }

    public class GroupPrincipalMap
    {
        public long PrincipalId { get; set; }
        public Principal Principal { get; set; }

        public long GroupId { get; set; }
        public Group Group { get; set; }
    }


    public class Role
    {
        [Key]
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<Principal> Members { get; set; }
    }
}