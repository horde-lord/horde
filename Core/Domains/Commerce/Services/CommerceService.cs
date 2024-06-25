using Autofac;
using Horde.Core.Domains.Economy.Entities;
using Horde.Core.Domains.Economy.Services;
using Horde.Core.Domains.World.Entities;
using Horde.Core.Domains.World.Services;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Horde.Core.Utilities;

namespace Horde.Core.Domains.Commerce.Services
{
    public class CommerceService : BaseService
    {
        public CommerceService(ILifetimeScope scope) : base(scope, ContextNames.Commerce)
        {

        }



        public async Task<DepositOrder> CreateDepositDraftOrder(int userId, int accountId, decimal amount)
        {
            if (amount <= 0)
                throw new Exception("Amount cannot be 0");
            if (accountId <= 0)
                throw new Exception("Account Id cannot be 0");
            var account = __<Account>("Currency", "AccountSponsor").
                FirstOrDefault(a => a.Id == accountId && a.Type == AccountType.Global && !a.Deleted);
            if (account == null)
                throw new Exception($"Invalid account Id {accountId}");
            //if ((account?.Id ?? 0) != (account?.AccountSponsor?.PayInAccountId ?? -1))
            //    throw new Exception($"Invalid account Id {accountId}");
            if (account?.Currency.Type != CurrencyNatureType.Fiat)
                throw new Exception($"Only Fiat currency is allowed to add money");
            var service = GetTenanted<CommerceService>(account.PartnerId);
            var user = service._<User>().FirstOrDefault(u => u.Id == userId && u.PartnerId == account.PartnerId);
            if (user == null)
                throw new Exception($"Invalid user Id {userId}");

            var order = new DepositOrder()
            {
                Id = 0,
                UserId = userId,
                AccountId = accountId,
                Amount = amount,
                Status = OrderStatusType.Draft,
                Deleted = false,
                PartnerId = account.PartnerId
            };

            await service.GetNew<CommerceService>().Save(order);
            return order;
        }

        public async Task ProcessOrder(int orderId)
        {
            var order = __<Order>(orderId);
            if (order == null)
                throw new Exception($"Order {orderId} not found");
            if (order.Status != OrderStatusType.Confirmed)
                throw new Exception($"Order {orderId} is not confirmed");

            var service = GetTenanted<CommerceService>(order.PartnerId);

            if (order is DepositOrder)
            {
                var depositOrder = order as DepositOrder;
                if (depositOrder.TransactionId > 0)
                    return;
                var gateway = service._<GatewayPayin>().Where(g => g.OrderId == orderId).FirstOrDefault();
                if (gateway == null)
                    throw new Exception($"Gateway Payin not found for order {orderId}");
                if (gateway.Status != PayinStatusType.Succeeded)
                    return;
                var gatewayAccount = service._<Account>(gateway.GatewayInputAccountId, "Currency");
                if (gatewayAccount == null)
                    throw new Exception($"Account {gateway.GatewayInputAccountId} not found");
                var destinationAccount = service._<Account>(depositOrder.AccountId);
                try
                {
                    var key = $"DepositOrder_{orderId}";
                    var transaction = await service.GetNew<PaymentService>().Transfer(gateway.Amount, key, nameof(depositOrder),
                        depositOrder.Id, "DepositMoneyToAccount", $"Money successfully deposited to account for order: {orderId}",
                        gatewayAccount.Key, gatewayAccount.Key, destinationAccount, gatewayAccount);
                    transaction = __<Transaction>().FirstOrDefault(t => t.Key == key);
                    if (transaction != null)
                    {
                        depositOrder = __<DepositOrder>(orderId);
                        depositOrder.TransactionId = transaction.Id;
                        await service.Save(depositOrder);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fatal Payment Failure. " +
                        $"Please contact Tribal Arena. Order Id {orderId}. {ex.Message}");
                }
            }

            //
            //if (order.Shipment == null)
            //    throw new Exception($"Order Shipment not found for order {order.Id}");
            //if (order.Status != OrderStatusType.Confirmed)
            //    throw new Exception($"Order is not confirmed {order.Id}");
            //var shipment = order.Shipment;
            //if (shipment.Status == ShipmentStatusType.Delivered || shipment.Status == ShipmentStatusType.ReturnToSender)
            //    throw new Exception($"Order {orderId} cannot be processed as its shipment status {shipment.Status}");

        }

       

        public int ProductVariantStock(int variantId)
        {
            var variant = _<ProductVariant>().Where(v => v.Id == variantId && !v.Deleted).FirstOrDefault();
            if (variant == null)
                return 0;
            if (!variant.IsQtyAvailable)
                return 0;
            var orderItemCount = VariantOrderCount(variantId);
            var stock = variant.TotalQty - orderItemCount;
            if (stock < 0)
                return 0;
            return stock;
        }

        public int VariantOrderCount(int variantId)
        {
            var orderItemCount = _<OrderItem>().Where(o => o.VariantId == variantId && !o.Deleted &&
            o.Order.Deleted == false && o.Order.Status == OrderStatusType.Confirmed).Sum(o => o.OrderedQty);
            return orderItemCount;
        }

        public Address UserLatestAddress(int userId)
        {
            var order = _<CommerceOrder>("Shipment.Address").Where(o => o.UserId == userId && o.Shipment.AddressId != 0 && !o.Shipment.Address.Deleted).OrderByDescending(o => o.CreatedAt).FirstOrDefault();
            if (order?.Shipment?.Address != null)
                return order?.Shipment.Address;
            var address = _<Address>().Where(a => !a.Deleted && a.UserId == userId).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            return address;
        }

        public void CheckProductVariantValidity(ProductVariant variant)
        {
            if (variant == null)
                throw new Exception("Product variant is null");
            if(variant.ProductId<=0)
                throw new Exception("Product Id cannot be null or empty");
            if (string.IsNullOrEmpty(variant.Title))
                throw new Exception("Product Title cannot be null or empty");
            if (variant.CurrencyId == 0)
                throw new Exception("Currency cannot be empty");
            if (variant.Cost <= 0)
                throw new Exception("Product cost cannot be negative or zero");
            if (variant.TotalQty < 0)
                throw new Exception($"Product quantity cannot be negative i.e {variant.TotalQty}");
        }

        public async Task<List<Order>> GetOrdersByIds(List<int> orderIds,bool needOrderRefernceImages=true)
        {
            if (orderIds.IsNullOrEmpty())
                return new List<Order>();

            var orders = __<Order>().Where(o => !o.Deleted && orderIds.Contains(o.Id))
                .OrderByDescending(o => o.CreatedAt).ToList() ?? new List<Order>();

            var commerceOrders = orders.Where(o => o is CommerceOrder).Select(o => o as CommerceOrder).ToList() ?? new List<CommerceOrder>();
            var shipmentIds = commerceOrders.Select(o => o.ShipmentId).ToList() ?? new List<int?>();
            var shipments = __<Shipment>().Where(s => shipmentIds.Contains(s.Id)).ToList() ?? new List<Shipment>();
            var orderItems = await GetOrderItemsForOrders(commerceOrders.Select(o => o.Id).ToList(),needOrderRefernceImages);
            foreach (var order in commerceOrders)
            {
                order.Shipment = shipments.FirstOrDefault(s => s.Id == order.ShipmentId);
                var items = orderItems.Where(o => o.OrderId == order.Id).ToList() ?? new List<OrderItem>();
                order.OrderItems = items;
            }
            orders.RemoveAll(o => o is CommerceOrder);
            orders.AddRange(commerceOrders);
            orders = orders.OrderByDescending(o => o.CreatedAt).ToList();
            return orders.Select(o => o as Order).ToList() ?? new List<Order>();
        }

        public async Task AddOrUpdateSalePrices(int variantId, List<Price> salePrices)
        {
            if (salePrices.IsNullOrEmpty())
                salePrices = new List<Price>();

            var existingPrices = _<Price>().Where(p => p.VariantId == variantId).ToList() ?? new List<Price>();

            var currencyIds = new HashSet<int>();

            foreach (var price in salePrices)
            {
                if (currencyIds.Contains(price.CurrencyId))
                    throw new Exception($"Sale Prices cannot contain {price.CurrencyId} more than once");
                currencyIds.Add(price.CurrencyId);

                var existingPrice = existingPrices.FirstOrDefault(e => e.CurrencyId == price.CurrencyId);
                price.VariantId = variantId;
                price.Deleted = false;
                price.Id = existingPrice?.Id ?? 0;
            }
            if (!salePrices.IsNullOrEmpty())
                await Save(salePrices);

            var leftPrices = existingPrices.Where(e => !salePrices.Any(p => p.CurrencyId == e.CurrencyId) && !e.Deleted).ToList();

            if (!leftPrices.IsNullOrEmpty())
            {
                foreach (var price in leftPrices)
                    price.Deleted = true;
                await Save(leftPrices);
            }
        }

        

        public async Task<List<OrderItem>> GetOrderItemsForOrders(List<int> orderIds = null,bool needReferenceImage=false)
        {
            if (orderIds.IsNullOrEmpty())
                return new List<OrderItem>();

            var orderItems = _<OrderItem>("Order").Where(o => orderIds.Contains(o.OrderId) &&
            !o.Deleted && !o.Order.Deleted).ToList() ?? new List<OrderItem>();

            var orderItemIds = orderItems.Select(o => o.Id).ToList() ?? new List<int>();

            var variantIds = orderItems.Select(o => o.VariantId).ToList() ?? new List<int>();
            var variants = _<ProductVariant>().Where(v => variantIds.Contains(v.Id)).ToList() ?? new List<ProductVariant>();
            var prices = _<Price>().Where(p => variantIds.Contains(p.VariantId) && !p.Deleted).ToList() ?? new List<Price>();
            var images = _<ProductVariantImage>().Where(p => variantIds.Contains(p.VariantId) && !p.Deleted).ToList() ?? new List<ProductVariantImage>();
            var currencyIds = prices.Select(p => p.CurrencyId).Distinct().ToList() ?? new List<int>();
            var currencies = __<Currency>(currencyIds);

            foreach (var price in prices)
            {
                price.Currency = currencies.FirstOrDefault(c => c.Id == price.CurrencyId);
            }

            foreach (var variant in variants)
            {
                var varaintPrices = prices.Where(p => p.VariantId == variant.Id).ToList() ?? new List<Price>();
                var variantImages = images.Where(p => p.VariantId == variant.Id).OrderBy(p => p.Order).ToList() ?? new List<ProductVariantImage>();
                variant.Prices = varaintPrices;
                variant.Images = variantImages;
            }

            var orderItemTransactions = _<OrderItemTransaction>().Where(t => orderItemIds.Contains(t.OrderItemId) && !t.Deleted).ToList() ?? new List<OrderItemTransaction>();
            var transactionIds = orderItemTransactions.Select(t => t.TransactionId).ToList() ?? new List<int>();
            var transactions = _<Transaction>("Source", "Destination").Where(t => transactionIds.Contains(t.Id) && !t.Deleted).ToList() ?? new List<Transaction>();

            foreach (var orderTransaction in orderItemTransactions)
            {
                var transaction = transactions.FirstOrDefault(t => t.Id == orderTransaction.TransactionId);
                orderTransaction.Transaction = transaction;
            }

            
            return orderItems;
        }


        public List<ProductVariant> GetProductVariantsById(List<int> variantIds)
        {
            if (variantIds.IsNullOrEmpty())
                return new List<ProductVariant>();

            var variants = _<ProductVariant>("Images", "Prices").Where(v => variantIds.Contains(v.Id)).ToList() ?? new List<ProductVariant>();
            var currencyIds = variants.SelectMany(v => v.Prices).Select(p => p.CurrencyId).Distinct().ToList() ?? new List<int>();
            var currencies = _<Currency>(currencyIds);
            foreach (var variant in variants)
            {
                variant.Prices = variant.Prices.Where(p => !p.Deleted).ToList();
                variant.Images = variant?.Images?.Where(i => !i.Deleted).OrderBy(i => i.Order)?.ToList() ?? new List<ProductVariantImage>();
                foreach (var price in variant.Prices)
                    price.Currency = currencies.FirstOrDefault(c => c.Id == price.CurrencyId);
            }
            return variants;
        }




        public async Task<List<Product>> SearchForProductsAsync(string search)
        {
            var store = Get<ICommerceStore>();
            return await store.FindProducts(search);
        }

        private decimal TotalSalePriceForItems(List<OrderItem> orderItems, int currencyId)
        {
            if (orderItems.IsNullOrEmpty())
                return 0;
            var salePrice = 0m;
            foreach (var item in orderItems)
            {
                if (item.OrderedQty <= 0)
                    throw new Exception($"Order quantity for item is negative or zero {item.Id} and {item.OrderedQty}");
                var variant = item.Variant;
                var variantPrice = variant?.Prices?.FirstOrDefault(p => p.CurrencyId == currencyId);
                if (variantPrice == null)
                    throw new Exception($"{currencyId} currency price is not available for item {item.Id}");
                if (variantPrice.SalePrice < 0)
                    throw new Exception($"{currencyId} currency sale price for variant {variant.Id} is negative {variantPrice.SalePrice}");
                salePrice += variantPrice.SalePrice * item.OrderedQty;
            }
            return salePrice;
        }

        public async Task DoOrderPayment(int orderId)
        {
            var order = _<CommerceOrder>(orderId);
            var user = _<User>(order.UserId);
            var orderItems = _<OrderItem>("Variant.Prices").Where(o => o.OrderId == orderId && !o.Deleted).ToList() ?? new List<OrderItem>();
            var currencyId = orderItems.FirstOrDefault()?.Variant?.Prices?.FirstOrDefault()?.CurrencyId ?? 0;

            if (currencyId == 0)
                throw new Exception($"No currency found for order {orderId}");

            if (orderItems.IsNullOrEmpty())
                throw new Exception($"No items added into cart for order {orderId}");

            var paymentService = Get<PaymentService>();
            var virtualCurrencyService = Get<VirtualCurrencyService>();
            var virtualCurrencyAccount = await virtualCurrencyService.GetUserVirtualCurrencyWallet(user.Id, currencyId);
            var userAccountBalance = paymentService.GetAccountBalance(virtualCurrencyAccount.Id);

            var totalItemSalePrice = TotalSalePriceForItems(orderItems, currencyId);

            if (userAccountBalance < totalItemSalePrice)
                throw new Exception($"Payment Failed: Insufficient Balance. Total Item cost is currency:{currencyId} {totalItemSalePrice}. User currency:{currencyId} Account Balance is currency:{currencyId} {userAccountBalance}");

            var itemTransactions = new List<OrderItemTransaction>();
            foreach (var item in orderItems)
            {

                var itemPrice = item.Variant.Prices.FirstOrDefault(p => p.CurrencyId == currencyId);
                var totalItemAmount = itemPrice.SalePrice * item.OrderedQty;

                var key = $"store_payment_{item.Id}";

                await Get<PaymentService>().GratifyAccount(virtualCurrencyAccount,
                       totalItemAmount, item, key: key, paymentKey: "OrderItemPayment",
                       $"Store Payment for {item.Variant.Title} , Qty: {item.OrderedQty} , Order Id {orderId}");

                var transaction = _<Transaction>().FirstOrDefault(t => t.Key == key);
                var itemTransaction = new OrderItemTransaction() { OrderItemId = item.Id, TransactionId = transaction.Id };
                itemTransactions.Add(itemTransaction);
            }
            try
            {
                await Get<PaymentService>().Save(itemTransactions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fatal Payment Failure. Please contact Tribal Arena. Order Id {orderId}. {ex.Message}");
            }
        }
    }
}
