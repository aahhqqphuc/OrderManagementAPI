-- ===========================================================================
-- GetOrderWithDetails Procedure
-- Retrieves an order with all its details by order ID
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[GetOrderWithDetails]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get order information
    SELECT o.Id, o.CustomerName, o.TotalAmount, o.Status, o.CreatedAt, o.UpdatedAt
    FROM Orders o
    WHERE o.Id = @OrderId;

    -- Get order details
    SELECT od.Id, od.OrderId, od.ProductName, od.Quantity, od.Price
    FROM OrderDetails od
    WHERE od.OrderId = @OrderId;
END;
GO

-- ===========================================================================
-- GetOrdersPaginated Procedure
-- Retrieves paginated orders
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[GetOrdersPaginated]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate the number of records to skip
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Count total records for pagination
    SELECT @TotalCount = COUNT(*)
    FROM Orders o

    -- Get the paginated results
    SELECT o.Id, o.CustomerName, o.TotalAmount, o.Status, o.CreatedAt, o.UpdatedAt
    FROM Orders o
    ORDER BY o.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ===========================================================================
-- Create a user-defined table type for order details
-- ===========================================================================
IF EXISTS (SELECT * FROM sys.types WHERE name = 'OrderDetailsTableType')
    DROP TYPE OrderDetailsTableType;
GO

CREATE TYPE OrderDetailsTableType AS TABLE
(
    ProductName NVARCHAR(255),
    Quantity INT,
    Price DECIMAL(18,2)
);
GO

-- ===========================================================================
-- CreateOrder Procedure
-- Creates a new order with details and calculates total amount
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[CreateOrder]
    @CustomerName NVARCHAR(255),
    @Status INT,
    @OrderDetails OrderDetailsTableType READONLY,
    @NewOrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Calculate total amount from order details
        DECLARE @TotalAmount DECIMAL(18,2);
        SELECT @TotalAmount = SUM(Quantity * Price) FROM @OrderDetails;
        
        -- Insert order
        INSERT INTO Orders (CustomerName, TotalAmount, Status, CreatedAt, UpdatedAt)
        VALUES (@CustomerName, @TotalAmount, @Status, GETDATE(), GETDATE());
        
        -- Get the new order ID
        SET @NewOrderId = SCOPE_IDENTITY();
        
        -- Insert order details
        INSERT INTO OrderDetails (OrderId, ProductName, Quantity, Price)
        SELECT @NewOrderId, ProductName, Quantity, Price
        FROM @OrderDetails;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ===========================================================================
-- UpdateOrder Procedure
-- Updates an order and recalculates the total amount
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateOrder]
    @OrderId INT,
    @CustomerName NVARCHAR(255) = NULL,
    @Status INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update order
        UPDATE Orders
        SET 
            CustomerName = ISNULL(@CustomerName, CustomerName),
            Status = ISNULL(@Status, Status),
            UpdatedAt = GETDATE()
        WHERE Id = @OrderId;
        
        -- Recalculate and update total amount
        UPDATE Orders
        SET TotalAmount = (
            SELECT SUM(Quantity * Price)
            FROM OrderDetails
            WHERE OrderId = @OrderId
        )
        WHERE Id = @OrderId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ===========================================================================
-- DeleteOrder Procedure
-- Deletes an order and all its details with transaction support
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[DeleteOrder]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete order details first (foreign key constraint)
        DELETE FROM OrderDetails WHERE OrderId = @OrderId;
        
        -- Delete the order
        DELETE FROM Orders WHERE Id = @OrderId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ===========================================================================
-- AddOrderDetail Procedure
-- Adds a new detail to an order and updates the total amount
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[AddOrderDetail]
    @OrderId INT,
    @ProductName NVARCHAR(255),
    @Quantity INT,
    @Price DECIMAL(18,2),
    @NewDetailId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Insert the new order detail
        INSERT INTO OrderDetails (OrderId, ProductName, Quantity, Price)
        VALUES (@OrderId, @ProductName, @Quantity, @Price);
        
        -- Get the new detail ID
        SET @NewDetailId = SCOPE_IDENTITY();
        
        -- Update the order's total amount
        UPDATE Orders
        SET 
            TotalAmount = (
                SELECT SUM(Quantity * Price)
                FROM OrderDetails
                WHERE OrderId = @OrderId
            ),
            UpdatedAt = GETDATE()
        WHERE Id = @OrderId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ===========================================================================
-- DeleteOrderDetail Procedure
-- Removes a detail from an order and updates the total amount
-- ===========================================================================
CREATE OR ALTER PROCEDURE [dbo].[DeleteOrderDetail]
    @OrderDetailId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Get the order ID before deleting
        DECLARE @OrderId INT;
        SELECT @OrderId = OrderId FROM OrderDetails WHERE Id = @OrderDetailId;
        
        -- Delete the order detail
        DELETE FROM OrderDetails WHERE Id = @OrderDetailId;
        
        -- Update the order's total amount
        IF @OrderId IS NOT NULL
        BEGIN
            UPDATE Orders
            SET 
                TotalAmount = (
                    SELECT SUM(Quantity * Price)
                    FROM OrderDetails
                    WHERE OrderId = @OrderId
                ),
                UpdatedAt = GETDATE()
            WHERE Id = @OrderId;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO