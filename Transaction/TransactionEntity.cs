﻿using Utilities_aspnet.User;

namespace Utilities_aspnet.Transaction {
    [Table("Transactions")]
    public class TransactionEntity : BaseEntity {
        public decimal? Amount { get; set; }
        public string? Descriptions { get; set; }
        public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;
        public string? PaymentId { get; set; }
        public UserEntity? User { get; set; }
        public string? UserId { get; set; }
        public ProductEntity? Product { get; set; }
        public Guid? ProductId { get; set; }
    }

    public class TransactionReadDto {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public decimal? Amount { get; set; }
        public string? Descriptions { get; set; }
        public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;
        public string? PaymentId { get; set; }
        public UserReadDto? User { get; set; }
        public ProductReadDto? Ad { get; set; }
        public ProductReadDto? Product { get; set; }
        public ProductReadDto? Tutorial { get; set; }
        public ProductReadDto? Tender { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class TransactionCreateDto {
        public decimal? Amount { get; set; }
        public string? UserId { get; set; }
        public string? Descriptions { get; set; }
        public TransactionStatus? StatusId { get; set; } = TransactionStatus.Pending;
        public string? PaymentId { get; set; }
        public Guid? ProductId { get; set; }
    }


    public enum TransactionStatus {
        Fail = -1,
        Pending = 0,
        Success = 100,
    }
}