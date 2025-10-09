namespace _10xWarehouseNet.Dtos;

public record PaginationDto(int Page, int PageSize, int Total);

public record PaginatedResponseDto<T>(IEnumerable<T> Data, PaginationDto Pagination);
