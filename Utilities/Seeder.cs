namespace Utilities_aspnet.Utilities;

public static class Seeder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<GroupChatEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(_ => _.KeyValues);
			b.OwnsMany(_ => _.ReservationTimes);
			b.OwnsMany(_ => _.VisitCounts);
			b.OwnsOne(_ => _.ReservationSaloon).OwnsMany(_ => _.ReservationChairSections).OwnsMany(p => p.ReservationColumns);
			b.OwnsOne(_ => _.ReservationSaloon).OwnsMany(_ => _.ReservationChairSections).OwnsMany(p => p.ReservationRows);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(_ => _.Reacts);
		});
		builder.Entity<OrderEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(_ => _.ReservationTimes);
			b.OwnsMany(_ => _.ReserveChairs);
			b.OwnsMany(_ => _.OrderDetailHistories);
		});
	}
}