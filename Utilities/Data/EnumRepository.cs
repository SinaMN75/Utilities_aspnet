﻿namespace Utilities_aspnet.Utilities.Data;

public interface IAppSettingRepository {
    Task<GenericResponse<EnumDto?>> Read();
}

public class AppSettingRepository : IAppSettingRepository {
    private readonly DbContext _context;
    private readonly IMapper _mapper;

    public AppSettingRepository(DbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    public Task<GenericResponse<EnumDto?>> Read() {
        EnumDto model = new() {
            Favorites = _context.Set<FavoriteEntity>().Select(x => new IdTitleReadDto {
                Id = x.Id,
                Title = x.Title
            }).ToList(),
            Colors = _context.Set<ColorEntity>().Select(x => new IdTitleReadDto {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Color
            }).ToList(),
            Specialties = _context.Set<SpecialityEntity>().Select(x => new IdTitleReadDto {
                Id = x.Id,
                Title = x.Title,
                Subtitle = x.Color
            }).ToList()
        };

        List<IdTitleReadDto> formFieldType = EnumExtension.GetValues<FormFieldType>();
        List<IdTitleReadDto> idTitleUseCase = EnumExtension.GetValues<IdTitleUseCase>();
        List<IdTitleReadDto> mediaUseCase = EnumExtension.GetValues<MediaUseCase>();
        model.FormFieldType = formFieldType;
        model.CategoryUseCase = idTitleUseCase;
        model.MediaUseCase = mediaUseCase;

        model.Categories = _context.Set<CategoryEntity>()
            .Include(x => x.Media)
            .Include(x => x.Parent)
            .OrderBy(x => x.UseCase)
            .Select(w =>
                new IdTitleReadDto {
                    Id = w.Id,
                    Title = w.Title,
                    UseCase = w.UseCase,
                    ParentId = w.ParentId
                }).ToList();

        return Task.FromResult(new GenericResponse<EnumDto?>(model, UtilitiesStatusCodes.Success, "Success"));
    }
}