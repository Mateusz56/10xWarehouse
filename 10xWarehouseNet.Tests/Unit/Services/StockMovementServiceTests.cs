using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace _10xWarehouseNet.Tests.Unit.Services;

/// <summary>
/// Unit tests for StockMovementService
/// </summary>
public class StockMovementServiceTests : IDisposable
{
    private readonly WarehouseDbContext _context;
    private readonly Mock<ILogger<StockMovementService>> _loggerMock;
    private readonly StockMovementService _service;
    
    private readonly Guid _testOrgId;
    private readonly Guid _testUserId;
    private readonly Guid _testProductId;
    private readonly Guid _testWarehouseId;
    private readonly Guid _testLocation1Id;
    private readonly Guid _testLocation2Id;

    public StockMovementServiceTests()
    {
        // Set up in-memory database with warnings suppressed for transactions
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new WarehouseDbContext(options);
        _loggerMock = new Mock<ILogger<StockMovementService>>();
        _service = new StockMovementService(_context, _loggerMock.Object);
        
        // Initialize test data IDs
        _testOrgId = Guid.NewGuid();
        _testUserId = Guid.NewGuid();
        _testProductId = Guid.NewGuid();
        _testWarehouseId = Guid.NewGuid();
        _testLocation1Id = Guid.NewGuid();
        _testLocation2Id = Guid.NewGuid();
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var organization = new Organization
        {
            Id = _testOrgId,
            Name = "Test Organization"
        };
        _context.Organizations.Add(organization);

        var warehouse = new Warehouse
        {
            Id = _testWarehouseId,
            OrganizationId = _testOrgId,
            Name = "Test Warehouse"
        };
        _context.Warehouses.Add(warehouse);

        var location1 = new Location
        {
            Id = _testLocation1Id,
            OrganizationId = _testOrgId,
            WarehouseId = _testWarehouseId,
            Name = "Location 1"
        };
        _context.Locations.Add(location1);

        var location2 = new Location
        {
            Id = _testLocation2Id,
            OrganizationId = _testOrgId,
            WarehouseId = _testWarehouseId,
            Name = "Location 2"
        };
        _context.Locations.Add(location2);

        var product = new ProductTemplate
        {
            Id = _testProductId,
            OrganizationId = _testOrgId,
            Name = "Test Product",
            Barcode = "TEST123"
        };
        _context.ProductTemplates.Add(product);

        _context.SaveChanges();
    }

    #region Add Stock Movement Tests

    [Fact]
    public async Task CreateStockMovementAsync_Add_CreatesInventoryIfMissing()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: 25,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MovementType.Add, result.MovementType);
        Assert.Equal(25, result.Delta);
        Assert.Equal(_testLocation1Id, result.ToLocationId);
        Assert.Equal(25, result.Total);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == _testOrgId 
                && i.ProductTemplateId == _testProductId 
                && i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(25, inventory.Quantity);

        var stockMovement = await _context.StockMovements
            .FirstOrDefaultAsync(sm => sm.Id == result.Id);
        
        Assert.NotNull(stockMovement);
        Assert.Equal(25, stockMovement.Delta);
        Assert.Equal(25, stockMovement.Total);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Add_UpdatesExistingInventory()
    {
        // Arrange - Create existing inventory
        var existingInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 50
        };
        _context.Inventories.Add(existingInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: 25,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.Equal(75, result.Total);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == _testOrgId 
                && i.ProductTemplateId == _testProductId 
                && i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(75, inventory.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Add_WithZeroDelta_Works()
    {
        // Arrange - Zero delta is technically valid (non-negative check passes)
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: 0,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert - Should succeed with zero delta
        Assert.NotNull(result);
        Assert.Equal(0, result.Delta);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Add_WithNegativeDelta_ThrowsException()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: -10,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    #endregion

    #region Withdraw Stock Movement Tests

    [Fact]
    public async Task CreateStockMovementAsync_Withdraw_SufficientStock_UpdatesInventoryCorrectly()
    {
        // Arrange - Create inventory with 100 units
        var existingInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 100
        };
        _context.Inventories.Add(existingInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Withdraw,
            Delta: 30,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MovementType.Withdraw, result.MovementType);
        Assert.Equal(-30, result.Delta); // Delta should be negative
        Assert.Equal(_testLocation1Id, result.FromLocationId);
        Assert.Equal(70, result.Total);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == _testOrgId 
                && i.ProductTemplateId == _testProductId 
                && i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(70, inventory.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Withdraw_InsufficientStock_ThrowsException()
    {
        // Arrange - Create inventory with only 20 units
        var existingInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 20
        };
        _context.Inventories.Add(existingInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Withdraw,
            Delta: 50,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
        
        Assert.Contains("Insufficient inventory", exception.Message);
        
        // Verify inventory was not changed
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == _testOrgId 
                && i.ProductTemplateId == _testProductId 
                && i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(20, inventory.Quantity); // Should remain unchanged
        
        // Verify no stock movement was created
        var movements = await _context.StockMovements
            .Where(sm => sm.OrganizationId == _testOrgId && sm.ProductTemplateId == _testProductId)
            .ToListAsync();
        
        Assert.Empty(movements);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Withdraw_FromEmptyInventory_ThrowsException()
    {
        // Arrange - No inventory exists
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Withdraw,
            Delta: 10,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    [Fact]
    public async Task CreateStockMovementAsync_Withdraw_WithZeroDelta_ThrowsException()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Withdraw,
            Delta: 0,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    #endregion

    #region Move Stock Movement Tests

    [Fact]
    public async Task CreateStockMovementAsync_Move_CreatesMoveSubtractAndMoveAddRecords()
    {
        // Arrange - Create inventory at source location
        var sourceInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 100
        };
        _context.Inventories.Add(sourceInventory);

        // Create inventory at destination location
        var destInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation2Id,
            Quantity = 50
        };
        _context.Inventories.Add(destInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Move,
            Delta: 25,
            LocationId: null,
            FromLocationId: _testLocation1Id,
            ToLocationId: _testLocation2Id
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert - Should return MoveAdd record
        Assert.NotNull(result);
        Assert.Equal(MovementType.MoveAdd, result.MovementType);
        Assert.Equal(25, result.Delta);
        Assert.Equal(_testLocation1Id, result.FromLocationId);
        Assert.Equal(_testLocation2Id, result.ToLocationId);
        Assert.Equal(75, result.Total); // 50 + 25

        // Verify both MoveSubtract and MoveAdd records exist
        var movements = await _context.StockMovements
            .Where(sm => sm.OrganizationId == _testOrgId 
                && sm.ProductTemplateId == _testProductId)
            .OrderBy(sm => sm.CreatedAt)
            .ToListAsync();
        
        Assert.Equal(2, movements.Count);
        
        var moveSubtract = movements.First(m => m.MovementType == MovementType.MoveSubtract);
        Assert.Equal(-25, moveSubtract.Delta);
        Assert.Equal(75, moveSubtract.Total); // 100 - 25
        
        var moveAdd = movements.First(m => m.MovementType == MovementType.MoveAdd);
        Assert.Equal(25, moveAdd.Delta);
        Assert.Equal(75, moveAdd.Total); // 50 + 25

        // Verify source inventory was decremented
        var sourceInventoryAfter = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation1Id);
        Assert.NotNull(sourceInventoryAfter);
        Assert.Equal(75, sourceInventoryAfter.Quantity);

        // Verify destination inventory was incremented
        var destInventoryAfter = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation2Id);
        Assert.NotNull(destInventoryAfter);
        Assert.Equal(75, destInventoryAfter.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Move_CreatesDestinationInventoryIfMissing()
    {
        // Arrange - Only source inventory exists
        var sourceInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 100
        };
        _context.Inventories.Add(sourceInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Move,
            Delta: 25,
            LocationId: null,
            FromLocationId: _testLocation1Id,
            ToLocationId: _testLocation2Id
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        var destInventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation2Id);
        
        Assert.NotNull(destInventory);
        Assert.Equal(25, destInventory.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Move_InsufficientStock_ThrowsException()
    {
        // Arrange - Source has only 20 units
        var sourceInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 20
        };
        _context.Inventories.Add(sourceInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Move,
            Delta: 50,
            LocationId: null,
            FromLocationId: _testLocation1Id,
            ToLocationId: _testLocation2Id
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
        
        Assert.Contains("Insufficient inventory at source location", exception.Message);
        
        // Verify no movements were created
        var movements = await _context.StockMovements
            .Where(sm => sm.OrganizationId == _testOrgId)
            .ToListAsync();
        
        Assert.Empty(movements);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Move_SameSourceAndDestination_ThrowsException()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Move,
            Delta: 25,
            LocationId: null,
            FromLocationId: _testLocation1Id,
            ToLocationId: _testLocation1Id
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    #endregion

    #region Reconcile Stock Movement Tests

    [Fact]
    public async Task CreateStockMovementAsync_Reconcile_SetsAbsoluteQuantity()
    {
        // Arrange - Create inventory with 45 units
        var existingInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 45
        };
        _context.Inventories.Add(existingInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Reconcile,
            Delta: 60, // This represents the new total quantity
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MovementType.Reconcile, result.MovementType);
        Assert.Equal(15, result.Delta); // Delta should be calculated as 60 - 45 = 15
        Assert.Equal(60, result.Total); // Total should be the new quantity
        Assert.Equal(_testLocation1Id, result.ToLocationId);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(60, inventory.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Reconcile_CreatesInventoryIfMissing()
    {
        // Arrange - No inventory exists
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Reconcile,
            Delta: 100, // New total quantity
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.Equal(100, result.Delta); // Delta equals the quantity since no inventory existed (100 - 0)
        Assert.Equal(100, result.Total);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(100, inventory.Quantity);
    }

    [Fact]
    public async Task CreateStockMovementAsync_Reconcile_WithZeroQuantity_Works()
    {
        // Arrange - Create inventory with 50 units
        var existingInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            LocationId = _testLocation1Id,
            Quantity = 50
        };
        _context.Inventories.Add(existingInventory);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Reconcile,
            Delta: 0, // Reconcile to zero
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act
        var result = await _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command);

        // Assert
        Assert.Equal(-50, result.Delta); // Delta should be 0 - 50 = -50
        Assert.Equal(0, result.Total);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == _testLocation1Id);
        
        Assert.NotNull(inventory);
        Assert.Equal(0, inventory.Quantity);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task CreateStockMovementAsync_ProductDoesNotExist_ThrowsException()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var command = new CreateStockMovementCommand(
            ProductTemplateId: nonExistentProductId,
            MovementType: MovementType.Add,
            Delta: 10,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
        
        Assert.Contains("Product template", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateStockMovementAsync_LocationDoesNotExist_ThrowsException()
    {
        // Arrange
        var nonExistentLocationId = Guid.NewGuid();
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: 10,
            LocationId: nonExistentLocationId,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
        
        Assert.Contains("Location", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateStockMovementAsync_MissingLocationId_ThrowsException()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Add,
            Delta: 10,
            LocationId: null,
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    [Fact]
    public async Task CreateStockMovementAsync_Move_MissingFromLocationId_ThrowsException()
    {
        // Arrange
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Move,
            Delta: 10,
            LocationId: null,
            FromLocationId: null,
            ToLocationId: _testLocation2Id
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    [Fact]
    public async Task CreateStockMovementAsync_ProductInDifferentOrganization_ThrowsException()
    {
        // Arrange - Create another organization with its own product
        var otherOrgId = Guid.NewGuid();
        var otherOrg = new Organization { Id = otherOrgId, Name = "Other Org" };
        _context.Organizations.Add(otherOrg);

        var otherWarehouse = new Warehouse 
        { 
            Id = Guid.NewGuid(), 
            OrganizationId = otherOrgId, 
            Name = "Other Warehouse" 
        };
        _context.Warehouses.Add(otherWarehouse);

        var otherLocation = new Location 
        { 
            Id = Guid.NewGuid(), 
            OrganizationId = otherOrgId, 
            WarehouseId = otherWarehouse.Id, 
            Name = "Other Location" 
        };
        _context.Locations.Add(otherLocation);

        var otherProduct = new ProductTemplate 
        { 
            Id = Guid.NewGuid(), 
            OrganizationId = otherOrgId, 
            Name = "Other Product" 
        };
        _context.ProductTemplates.Add(otherProduct);
        await _context.SaveChangesAsync();

        var command = new CreateStockMovementCommand(
            ProductTemplateId: otherProduct.Id, // Product from different org
            MovementType: MovementType.Add,
            Delta: 10,
            LocationId: _testLocation1Id, // Location from test org
            FromLocationId: null,
            ToLocationId: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));
    }

    #endregion

    #region Transaction Rollback Tests

    [Fact]
    public async Task CreateStockMovementAsync_OnFailure_RollsBackTransaction()
    {
        // Arrange - Create invalid scenario that will fail validation
        var command = new CreateStockMovementCommand(
            ProductTemplateId: _testProductId,
            MovementType: MovementType.Withdraw,
            Delta: 100,
            LocationId: _testLocation1Id,
            FromLocationId: null,
            ToLocationId: null
        );
        // No inventory exists, so withdraw will fail

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateStockMovementAsync(_testOrgId, _testUserId.ToString(), command));

        // Verify no data was persisted
        var movements = await _context.StockMovements
            .Where(sm => sm.OrganizationId == _testOrgId)
            .ToListAsync();
        
        Assert.Empty(movements);

        var inventories = await _context.Inventories
            .Where(i => i.OrganizationId == _testOrgId)
            .ToListAsync();
        
        Assert.Empty(inventories);
    }

    #endregion

    #region GetStockMovementsAsync Tests

    [Fact]
    public async Task GetStockMovementsAsync_ReturnsPaginatedResults()
    {
        // Arrange - Create some stock movements
        var movement1 = new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            MovementType = MovementType.Add,
            Delta = 10,
            ToLocationId = _testLocation1Id,
            Total = 10,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        var movement2 = new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            MovementType = MovementType.Add,
            Delta = 20,
            ToLocationId = _testLocation1Id,
            Total = 30,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-3)
        };
        _context.StockMovements.AddRange(movement1, movement2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetStockMovementsAsync(_testOrgId, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Pagination.Total);
        Assert.Equal(2, result.Data.Count());
        Assert.True(result.Data.All(d => d.ProductTemplateId == _testProductId));
    }

    [Fact]
    public async Task GetStockMovementsAsync_FiltersByProductTemplateId()
    {
        // Arrange
        var otherProductId = Guid.NewGuid();
        var otherProduct = new ProductTemplate
        {
            Id = otherProductId,
            OrganizationId = _testOrgId,
            Name = "Other Product"
        };
        _context.ProductTemplates.Add(otherProduct);

        var movement1 = new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            MovementType = MovementType.Add,
            Delta = 10,
            ToLocationId = _testLocation1Id,
            Total = 10,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var movement2 = new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = otherProductId,
            MovementType = MovementType.Add,
            Delta = 20,
            ToLocationId = _testLocation1Id,
            Total = 20,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.AddRange(movement1, movement2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetStockMovementsAsync(_testOrgId, pagination, productTemplateId: _testProductId);

        // Assert
        Assert.Equal(1, result.Pagination.Total);
        Assert.Single(result.Data);
        Assert.All(result.Data, d => Assert.Equal(_testProductId, d.ProductTemplateId));
    }

    [Fact]
    public async Task GetStockMovementsAsync_FiltersByLocationId()
    {
        // Arrange
        var movement1 = new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            MovementType = MovementType.MoveAdd,
            Delta = 10,
            FromLocationId = _testLocation1Id,
            ToLocationId = _testLocation2Id,
            Total = 10,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.Add(movement1);
        await _context.SaveChangesAsync();

        var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };

        // Act - Filter by source location
        var result1 = await _service.GetStockMovementsAsync(_testOrgId, pagination, locationId: _testLocation1Id);
        
        // Act - Filter by destination location
        var result2 = await _service.GetStockMovementsAsync(_testOrgId, pagination, locationId: _testLocation2Id);

        // Assert
        Assert.Equal(1, result1.Pagination.Total);
        Assert.Equal(1, result2.Pagination.Total);
    }

    [Fact]
    public async Task GetStockMovementsAsync_RespectsPagination()
    {
        // Arrange - Create 5 stock movements
        var movements = Enumerable.Range(1, 5).Select(i => new StockMovement
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            ProductTemplateId = _testProductId,
            MovementType = MovementType.Add,
            Delta = i * 10,
            ToLocationId = _testLocation1Id,
            Total = i * 10,
            UserId = _testUserId,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
        }).ToList();
        
        _context.StockMovements.AddRange(movements);
        await _context.SaveChangesAsync();

        var pagination = new PaginationRequestDto { Page = 2, PageSize = 2 };

        // Act
        var result = await _service.GetStockMovementsAsync(_testOrgId, pagination);

        // Assert
        Assert.Equal(5, result.Pagination.Total);
        Assert.Equal(2, result.Data.Count());
        Assert.Equal(2, result.Pagination.Page);
        Assert.Equal(2, result.Pagination.PageSize);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}

