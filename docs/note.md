Game
- Does not have an identifier
- `CreateBoard` and `PlayerHasLost` don't feel natural - not good pattern
- `ShipKind` should be used for size

Board
- Ships list is unlimited
- Constructors needs work
- Size is not set properly
- Ships and Attacks collections are exposed

Battleship
- `AttackAt()` can return `HitResult` instead of `void`

Cell Locator
- Does not feel natural
