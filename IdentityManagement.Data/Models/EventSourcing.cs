using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityManagement.Data.Models
{
    public class EventStream
    {
        [Column(Order = 0), Key]
        public virtual long Id { get; set; }
        public virtual System.DateTimeOffset Created { get; set; }
    }
    public abstract class EnvelopeEntityBase
    {
        public virtual System.Guid StreamId { get; set; }
        public virtual EventStream Stream { get; set; }
        public virtual System.Guid UserId { get; set; }
        public virtual System.Guid Id { get; set; }

        public virtual System.Guid TransactionId { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual short Version { get; set; }
        public virtual System.DateTimeOffset TimeStamp { get; set; }
        public virtual string Event { get; set; }
    }
    public class UserEventEnvelopeEntity : EnvelopeEntityBase { }
    public class GroupEventEnvelopeEntity : EnvelopeEntityBase { }
    public class RoleEventEnvelopeEntity : EnvelopeEntityBase { }
}
