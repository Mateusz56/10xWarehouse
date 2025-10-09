namespace _10xWarehouseNet.Dtos;

public record DashboardDto(
    IEnumerable<StockMovementDto> RecentMovements,
    IEnumerable<InventorySummaryDto> LowStockAlerts
);
