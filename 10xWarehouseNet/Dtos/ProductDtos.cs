namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record ProductTemplateDto(Guid Id, string Name, string Barcode, string? Description, int LowStockThreshold);

// Command Models

public record CreateProductTemplateCommand(string Name, string Barcode, string? Description, int LowStockThreshold);
