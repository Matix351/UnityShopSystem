# Inventory System Module

This folder contains the item and slot data classes used by the shop. They come from a separate inventory system; the full inventory implementation is not included in this repository.

## Included classes

| Class | Purpose |
| --- | --- |
| `SOItemData` | Abstract `ScriptableObject` containing an item's name, icon, and maximum inventory stack size. |
| `inventorySlotData` | Holds an item reference and quantity. Provides methods for adding, removing, replacing, and clearing slot data. |

`SOShopItemExample` extends `SOItemData` elsewhere in the shop module and adds a price and category through `IItemTradeable` and `IItemType`.

## How the shop uses these classes

`ShopManager` accepts `SOItemData` assets and sends an `inventorySlotData[]` to its inventory-capacity and purchase callbacks. In these callbacks, each entry represents the total quantity of one item in the cart, not necessarily a single inventory stack.

The integrating inventory system is responsible for checking available space, distributing purchased quantities across stacks, and adding the items after a successful purchase. `PlayerDebug` only simulates purchase checks and logs the result; it does not implement an inventory or wallet.

Implement `IShopPurchaseHandler.TryPurchase(items, totalPrice)` and register it with `ShopManager.RegisterPurchaseHandler(handler)`. Only one handler can own purchases; registering a different handler before unregistering the current one throws an exception. Call `UnregisterPurchaseHandler(handler)` when its owner is disabled or destroyed.

The handler must recheck current funds and capacity and commit money and inventory changes together. Returning `false` must leave both unchanged. The shop keeps the cart on failure and clears it on success. With no handler, checkout is refused.

Subscribe to `OnPurchaseCompleted` for UI, sound, quests, or logging. This notification runs after a successful transaction and cart reset; listeners do not approve or perform the purchase. The former `OnPurchaseEvent` API has been removed. `PlayerDebug` demonstrates registration and cleanup in `OnEnable` and `OnDisable`.

## Stack sizes and cart quantities

- `SOItemData.maxStackAmmount` limits how many items fit in one inventory stack. The sample assets use 64 for Other and Food, and 1 for Weapons and Tools.
- `ShopManager.MaxQuantityPerItem` is a separate limit on the quantity of each item in the cart. Its serialized setting defaults to 999.

For example, buying 70 apples with a stack size of 64 requires two stacks when no existing stack has available space. Buying three swords with a stack size of 1 requires three slots.

## Slot method behavior

- `addItems(amount)` fills an initialized slot up to its stack limit and returns the quantity that did not fit. Assign an item before calling it with a positive amount.
- `removeItems(amount)` returns the quantity removed and clears the item reference when the slot becomes empty.
- `clearData()` clears both the item reference and quantity.
- Constructors, `changeData(...)`, and `changeAmmount(...)` assign quantities without enforcing stack limits. Callers must validate their input; `changeAmmount(0)` also clears the item reference.

These classes are data helpers, not a complete inventory manager. Existing API spellings such as `ammount` and `maxStackAmmount` are preserved here to match the code.

