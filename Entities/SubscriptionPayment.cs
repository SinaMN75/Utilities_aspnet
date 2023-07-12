using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Entities
{
    [Table("SubscriptionPayment")]
    public class SubscriptionPaymentEntity : BaseEntity
    {
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.None;
        public OrderStatuses Status { get; set; } = OrderStatuses.Pending;
        public double? Amount { get; set; }
        public DateTime? PayDateTime { get; set; }
        public string? Description { get; set; }
        public UserEntity? User { get; set; }
        public string? UserId { get; set; }
        public PromotionEntity? Promotion { get; set; }
        public Guid? PromotionId { get; set; }
    }

    public class SubscriptionPaymentCreateUpdateDto
    {
        public Guid? Id { get; set; }
        public SubscriptionType? SubscriptionType { get; set; }
        public OrderStatuses? Status { get; set; }
        public Guid? PromotionId { get; set; }
        public string? UserId { get; set; }
        public double? Amount { get; set; }
        public string? Description { get; set; }
    }
    public class SubscriptionPaymentFilter
    {
        public bool? OrderBySubscriptionType { get; set; }
        public bool? OrderByStatus { get; set; }
        public bool? OrderByAmount { get; set; }
        public bool? ShowUser { get; set; }
        public bool? ShowPromotion { get; set; }

    }
}
