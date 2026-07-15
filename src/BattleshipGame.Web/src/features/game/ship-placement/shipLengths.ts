import type { components } from '../../../api/schema'

type ShipKind = components['schemas']['ShipKind']

// Ship sizes mirror the domain's source of truth:
// BattleshipGame.Domain/DomainModel/GameAggregate/ShipKindExtensions.cs.
// The API only accepts a bow cell + orientation, so the client derives the occupied
// cells from these lengths. Keep in sync if the domain ever changes them.
export const SHIP_LENGTHS: Record<Exclude<ShipKind, 'None'>, number> = {
  Destroyer: 2,
  Cruiser: 3,
  Submarine: 3,
  Battleship: 4,
  Carrier: 5,
}

// The five ships that must be placed, in descending size order.
export const SHIP_KINDS = ['Carrier', 'Battleship', 'Cruiser', 'Submarine', 'Destroyer'] as const

export type PlaceableShipKind = (typeof SHIP_KINDS)[number]
