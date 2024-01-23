using System.Data;
using ClosedXML.Excel;

namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto);
	Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct);
	Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class TransactionRepository(DbContext dbContext, IWebHostEnvironment env) : ITransactionRepository {
	public async Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct) {
		TransactionEntity e = new() {
			Amount = dto.Amount,
			Descriptions = dto.Descriptions,
			RefId = dto.RefId,
			CardNumber = dto.CardNumber,
			Tags = dto.Tags,
			BuyerId = dto.BuyerId,
			SellerId = dto.SellerId,
			OrderId = dto.OrderId,
			SubscriptionId = dto.SubscriptionId
		};
		await dbContext.Set<TransactionEntity>().AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct) {
		TransactionEntity e = (await dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		if (dto.Amount.HasValue) e.Amount = dto.Amount;
		if (dto.RefId.IsNotNullOrEmpty()) e.RefId = dto.RefId;
		if (dto.Tags.IsNotNullOrEmpty()) e.Tags = dto.Tags!;
		if (dto.CardNumber.IsNotNullOrEmpty()) e.CardNumber = dto.CardNumber;
		if (dto.Descriptions.IsNotNullOrEmpty()) e.Descriptions = dto.Descriptions;
		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		await dbContext.Set<TransactionEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}


	public GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto) {
		IQueryable<TransactionEntity> q = dbContext.Set<TransactionEntity>().AsNoTracking().OrderBy(x => x.CreatedAt)
			.Select(x => new TransactionEntity {
				Id = x.Id,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt,
				Amount = x.Amount,
				Descriptions = x.Descriptions,
				RefId = x.RefId,
				CardNumber = x.CardNumber,
				Tags = x.Tags,
				BuyerId = x.BuyerId,
				SellerId = x.SellerId,
				OrderId = x.OrderId,
				SubscriptionId = x.SubscriptionId,
				Code = x.Code,
				Buyer = new UserEntity {
					Id = x.Buyer!.Id,
					FirstName = x.Buyer.FirstName,
					LastName = x.Buyer.LastName,
					FullName = x.Buyer.FullName,
					Tags = x.Buyer.Tags,
					PhoneNumber = x.Buyer.PhoneNumber,
					JsonDetail = x.Buyer.JsonDetail,
					Email = x.Buyer.Email,
					AppEmail = x.Buyer.AppEmail,
					AppPhoneNumber = x.Buyer.AppPhoneNumber,
					AppUserName = x.Buyer.AppUserName
				},
				Seller = new UserEntity {
					Id = x.Buyer!.Id,
					FirstName = x.Buyer.FirstName,
					LastName = x.Buyer.LastName,
					FullName = x.Buyer.FullName,
					Tags = x.Buyer.Tags,
					PhoneNumber = x.Buyer.PhoneNumber,
					JsonDetail = x.Buyer.JsonDetail,
					Email = x.Buyer.Email,
					AppEmail = x.Buyer.AppEmail,
					AppPhoneNumber = x.Buyer.AppPhoneNumber,
					AppUserName = x.Buyer.AppUserName
				},
				Order = x.Order != null
					? new OrderEntity {
						Id = x.Order.Id,
						CreatedAt = x.Order.CreatedAt,
						UpdatedAt = x.Order.UpdatedAt,
						JsonDetail = x.Order.JsonDetail,
						UserId = x.Order.UserId,
						TotalPrice = x.Order.TotalPrice,
						ProductOwnerId = x.Order.ProductOwnerId,
						OrderNumber = x.Order.OrderNumber
					}
					: x.Order
			});
		if (dto.RefId.IsNotNullOrEmpty()) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.BuyerId.IsNotNullOrEmpty()) q = q.Where(x => x.BuyerId == dto.BuyerId);
		if (dto.SellerId.IsNotNullOrEmpty()) q = q.Where(x => x.BuyerId == dto.SellerId);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));


		GenerateExcel(q.ToList());

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsSingleQuery());
	}

	private void GenerateExcel(IEnumerable<TransactionEntity> list) {
		DataTable datatableSell = new();
		datatableSell.TableName = "فروش";
		datatableSell.Columns.Add("کد", typeof(string));
		datatableSell.Columns.Add("تاریخ", typeof(string));
		datatableSell.Columns.Add("عنوان ", typeof(string));
		datatableSell.Columns.Add("شماره سفارش", typeof(string));
		datatableSell.Columns.Add("بستانکار", typeof(long));
		datatableSell.Columns.Add("بدهکار", typeof(long));
		datatableSell.Columns.Add("تخفیف بستانکار", typeof(long));
		datatableSell.Columns.Add("تخفیف بدهکار", typeof(long));
		datatableSell.Columns.Add("مبلغ نهایی بستانکار", typeof(int));
		datatableSell.Columns.Add("مبلغ نهایی بدهکار", typeof(long));
		datatableSell.Columns.Add("توضیحات", typeof(string));
		
		DataTable datatableReturn = new();
		datatableReturn.TableName = "فروش";
		datatableReturn.Columns.Add("کد", typeof(string));
		datatableReturn.Columns.Add("تاریخ", typeof(string));
		datatableReturn.Columns.Add("عنوان ", typeof(string));
		datatableReturn.Columns.Add("شماره سفارش", typeof(string));
		datatableReturn.Columns.Add("بستانکار", typeof(long));
		datatableReturn.Columns.Add("بدهکار", typeof(long));
		datatableReturn.Columns.Add("تخفیف بستانکار", typeof(long));
		datatableReturn.Columns.Add("تخفیف بدهکار", typeof(long));
		datatableReturn.Columns.Add("مبلغ نهایی بستانکار", typeof(int));
		datatableReturn.Columns.Add("مبلغ نهایی بدهکار", typeof(long));
		datatableReturn.Columns.Add("توضیحات", typeof(string));
		
		foreach (TransactionEntity e in list) {
			if (e.Tags.Contains(TagTransaction.Sell)) {
				string code = e.Code ?? "";
				string date = e.CreatedAt.ToString(CultureInfo.InvariantCulture);
				string title = e.Order?.OrderDetails?.Select(x => x.Product.Title).ToString();
				string orderCode = (e.Order?.OrderNumber ?? 0).ToString();
				long bestankar = e.Amount ?? 0;
				long bedehkar = 0;
				long takhfifBestankar = 0;
				long takhfifBedehkar = 0;
				long finalPriceBestankar =  e.Amount ?? 0;
				long finalPriceBedehkar = 0;
				string description = "فروش به مبلغ " + (e.Amount ?? 0);
			
				datatableSell.Rows.Add(
					code,
					date,
					title,
					orderCode,
					bestankar,
					bedehkar,
					takhfifBestankar,
					takhfifBedehkar,
					finalPriceBestankar,
					finalPriceBedehkar,
					description
				);
			}
			else if (e.Tags.Contains(TagTransaction.Return)) {
				string code = e.Code ?? "";
				string date = e.CreatedAt.ToString(CultureInfo.InvariantCulture);
				string title = e.Order?.OrderDetails?.Select(x => x.Product.Title).ToString();
				string orderCode = (e.Order?.OrderNumber ?? 0).ToString();
				long bestankar = 0;
				long bedehkar = e.Amount ?? 0;
				long takhfifBestankar = 0;
				long takhfifBedehkar = 0;
				long finalPriceBestankar =  e.Amount ?? 0;
				long finalPriceBedehkar = e.Amount ?? 0;
				string description = "برگشت از فروش به مبلغ " + (e.Amount ?? 0);
			
				datatableReturn.Rows.Add(
					code,
					date,
					title,
					orderCode,
					bestankar,
					bedehkar,
					takhfifBestankar,
					takhfifBedehkar,
					finalPriceBestankar,
					finalPriceBedehkar,
					description
				);
			}
		}
		
		XLWorkbook wb = new();
		wb.AddWorksheet(datatableSell, "فروش");
		wb.AddWorksheet(datatableReturn, "برگشت از فروش");
		wb.SaveAs(Path.Combine(env.WebRootPath, "Report", Guid.NewGuid() + ".xlsx"));
	}
}