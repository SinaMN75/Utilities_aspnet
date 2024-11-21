namespace Utilities_aspnet.Utilities;

public static class Seeder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.UserSubscriptions).OwnsMany(i => i.KeyValues);
		});
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.KeyValues);
		});
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.KeyValues);
			b.OwnsMany(i => i.ReservationTimes);
			b.OwnsMany(i => i.VisitCounts);
			b.OwnsMany(i => i.Seats);
			b.OwnsMany(i => i.UsersReactions);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<QuestionEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.Answers);
		});
		builder.Entity<UserQuestionAnswerEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.UserQuestionAnswer);
		});
		builder.Entity<OrderEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(i => i.ReservationTimes);
			b.OwnsMany(i => i.Seats);
			b.OwnsMany(i => i.OrderDetailHistories);
		});
	}
}