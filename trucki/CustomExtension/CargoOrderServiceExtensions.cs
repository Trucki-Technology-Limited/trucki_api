using trucki.Interfaces.IServices;


public static class CargoOrderServiceExtensions
{
    public static async Task CreditDriverForDeliveryAsync(
        this IDriverWalletService driverWalletService,
        IDriverService driverService,
        string orderId,
        string truckId,
        decimal amount,
        string orderDescription)
    {
        try
        {
            // 1. Get the driver associated with the truck
            var driver = await driverService.GetDriverById(truckId);

            if (driver == null || string.IsNullOrEmpty(driver.Id))
            {
                // Log error - no driver found for truck
                return;
            }

            // 2. Credit the driver's wallet with the amount
            var description = $"Payment for delivery: {orderDescription}";
            await driverWalletService.CreditDeliveryAmountAsync(
                driver.Id,
                orderId,
                amount,
                description);
        }
        catch (Exception ex)
        {
            // Log error without disrupting the main delivery process
            // Consider adding to a queue for retry later
        }
    }
}
