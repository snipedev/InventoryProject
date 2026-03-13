using Inventory.Core.Abstraction;
using Inventory.Core.Entities;
using Inventory.Core.ValueObjects;

namespace Inventory.Core.Services
{
    public interface IInventoryReader
    {
        /// <summary>
        /// Reads all the Sku present
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<InventorySku?> GetSkuAsync(Sku sku, CancellationToken ct = default);
    }

    public interface IInventoryWriter
    {
        /// <summary>
        /// Creates a new inventory SKU with the specified initial available quantity.
        /// </summary>
        /// <remarks>This method may throw exceptions if the SKU is invalid or if the operation is
        /// canceled.</remarks>
        /// <param name="sku">The SKU object that contains the details of the inventory item to be created. Cannot be null.</param>
        /// <param name="initialAvailable">The initial quantity available for the SKU. Must be a non-negative value.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the created
        /// InventorySku.</returns>
        Task<Result<InventorySku>> CreateSkuAsync(Sku sku, long initialAvailable, CancellationToken ct = default);

        /// <summary>
        /// Adjusts the inventory data
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="delta"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Result<InventorySku>> AdjustAvailableAsync(Sku sku, long delta, CancellationToken ct = default);
    }
}
