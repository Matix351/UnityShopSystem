# Inventory System Module

This folder contains the item-data interface and slot data helper used by the shop. They come from a separate inventory system; the full inventory implementation is not included in this repository.

## Included classes

| Class | Purpose |
| --- | --- |
| `IItemData` | Interface exposing an item's `Name`, `Image`, and `MaxStackAmmount`. |
| `inventorySlotData` | Holds an item reference and quantity. Provides methods for adding, removing, replacing, and clearing slot data. |

`SOShopItemExample` extends `ScriptableObject` elsewhere in the shop module and implements `IItemData`, `IItemTradeable`, and `IItemType`. Custom items can implement these interfaces without inheriting from a shared item base class.

## How the shop uses these classes

`ShopManager.OpenShop` accepts an `IItemData[]` whose entries also implement `IItemTradeable`, and sends an `inventorySlotData[]` to its inventory-capacity and purchase callbacks. Each slot exposes an `IItemData` reference through `item` and a quantity through `ammount`. In these callbacks, each entry represents the total quantity of one item in the cart, not necessarily a single inventory stack.

The integrating inventory system is responsible for checking available space, distributing purchased quantities across stacks, and adding the items after a successful purchase. `PlayerDebug` only simulates purchase checks and logs the result; it does not implement an inventory or wallet.

Implement `IPurchaseHandler` with `CanPurchaseMoney(int price)`, `CanPurchaseInventorySlots(inventorySlotData[] cartItems)`, `TryPurchase(inventorySlotData[] items, int totalPrice)`, and an `Action StateChanged` event. Pass it to `ShopManager.OpenShop(items, handler, optionalCloseCallback)`. Registration is private to the manager. Start with the shop GameObject inactive and its component enabled; `OpenShop` initializes and activates it.

When the shop GameObject's `activeSelf` is already `true`, `OpenShop` returns immediately. Repeated calls preserve the current cart, item list, handler, and callbacks, even if different arguments are supplied. Close the current session before opening with a different item list or handler.

`CloseShop()` clears the cart, disconnects the handler, deactivates the shop GameObject, clears completion listeners, and invokes the pending close callback. `OnDisable` calls `CloseShop()`, so disabling the shop also performs that cleanup. To reconnect, call `OpenShop` while the shop GameObject remains inactive; manually activating it first causes the open call to be ignored. Re-enable the component first if you disabled it directly, and ensure its parent GameObjects are active when displaying the shop.

Raise `StateChanged` when funds or inventory capacity change. The manager subscribes while the shop is open and calls both condition checks to refresh its purchase messages. Checkout also calls both checks before attempting the transaction.

The handler must recheck current funds and capacity and commit money and inventory changes together. Returning `false` must leave both unchanged. The shop keeps the cart on failure and clears it on success. With no handler, checkout is refused.

Subscribe to `OnPurchaseCompleted` for UI, sound, quests, or logging. This notification runs after a successful transaction and cart reset; listeners do not approve or perform the purchase. Closing the shop clears these listeners, so subscribe again for each new session. Repeated open calls while the shop is active leave existing listeners registered; avoid adding another subscription when an open call is ignored. See the guarded example in the [main README](../../../README.md#open-and-close-a-shop). In the demo, `OpenShopButtonDebug` passes its assigned `PlayerDebug` through `ShopExample` to `ShopManager.OpenShop`; the demo handler logs a successful purchase inside `TryPurchase`.

## Stack sizes and cart quantities

- `IItemData.MaxStackAmmount` limits how many items fit in one inventory stack. The sample assets use 64 for Other and Food, and 1 for Weapons and Tools.
- `ShopManager.GetMaxQuantityPerItem` exposes the separate limit on the quantity of each item in the cart. Its serialized setting defaults to 999.

For example, buying 70 apples with a stack size of 64 requires two stacks when no existing stack has available space. Buying three swords with a stack size of 1 requires three slots.

## Slot method behavior

- `AddItems(amount)` fills an initialized slot up to its stack limit and returns the quantity that did not fit. Assign an item before calling it with a positive amount.
- `RemoveItems(amount)` returns the quantity removed and clears the item reference when the slot becomes empty.
- `ClearData()` clears both the item reference and quantity.
- Constructors, `ChangeData(...)`, and `ChangeAmmount(...)` assign quantities without enforcing stack limits. Callers must validate their input; `ChangeAmmount(0)` also clears the item reference.

These classes are data helpers, not a complete inventory manager. Existing API spellings such as `ammount` and `MaxStackAmmount` are preserved here to match the code.

