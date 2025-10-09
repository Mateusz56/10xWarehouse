namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record WarehouseDto(Guid Id, string Name);

public record LocationDto(Guid Id, string Name, string? Description);

// Command Models

public record CreateWarehouseCommand(string Name);

public record UpdateWarehouseCommand(string Name);

public record CreateLocationCommand(string Name, string? Description);
