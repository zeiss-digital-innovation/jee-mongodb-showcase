using DotNetMongoDbBackend.Models.Entities;
using DotNetMongoDbBackend.Models.DTOs;

namespace DotNetMongoDbBackend.Mappers;

public static class PointOfInterestMapper
{
    // Entity -> DTO (for GET requests)
    public static PointOfInterestDto? ToDto(PointOfInterestEntity entity)
    {
        if (entity == null) return null;
        return new PointOfInterestDto
        {
            Id = entity.Id,
            Category = entity.Category,
            Details = entity.Details,
            Name = entity.Name,
            Tags = entity.Tags,
            Location = entity.Location != null ? new LocationDto
            {
                Type = entity.Location.Type,
                Coordinates = entity.Location.Coordinates
            } : null
        };
    }


    // DTO -> (for POST/PUT requests)
    public static PointOfInterestEntity? ToEntity(PointOfInterestDto dto)
    {
        if (dto == null) return null;

        return new PointOfInterestEntity
        {
            Id = dto.Id,
            Category = dto.Category,
            Details = dto.Details,
            Name = dto.Name,
            Tags = dto.Tags,
            Location = dto.Location != null ? new LocationEntity
            {
                Type = dto.Location.Type,
                Coordinates = dto.Location.Coordinates
            } : null
        };
    }
    
    // Batch-converting
    public static List<PointOfInterestDto> ToDtoList(List<PointOfInterestEntity> entities)
    {
        return entities?.Select(ToDto).Where(dto => dto != null).Cast<PointOfInterestDto>().ToList() ?? [];
    }
}