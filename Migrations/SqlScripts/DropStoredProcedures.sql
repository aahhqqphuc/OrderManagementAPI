-- ===================================================================
-- Drop all stored procedures if they exist
-- ===================================================================
DROP PROCEDURE IF EXISTS GetOrderWithDetails;
DROP PROCEDURE IF EXISTS GetOrdersPaginated;
DROP PROCEDURE IF EXISTS CreateOrder;
DROP PROCEDURE IF EXISTS UpdateOrder;
DROP PROCEDURE IF EXISTS DeleteOrder;
DROP PROCEDURE IF EXISTS AddOrderDetail;
DROP PROCEDURE IF EXISTS DeleteOrderDetail;

-- ===================================================================
-- Drop user-defined table type if it exists
-- ===================================================================
DROP TYPE IF EXISTS OrderDetailsTableType;