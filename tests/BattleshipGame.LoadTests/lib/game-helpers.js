import http from "k6/http";
import { check, sleep } from "k6";
import { config, shipDefinitions, BoardSide } from "../config.js";
import { authHeaders } from "./auth-helpers.js";

/**
 * Creates a new game. The owner is derived from the authenticated caller (JWT subject),
 * so no playerId is sent in the body.
 * @param {string} token - The caller's JWT access token
 * @param {number} boardSize - The board size (default: 10)
 * @param {string} opponentStrategy - Opponent strategy: "Random" or "SemanticKernel" (default: undefined for API default)
 * @returns {string|null} The game ID or null if failed
 */
export function createGame(token, boardSize = config.boardSize, opponentStrategy = undefined) {
  const payload = opponentStrategy
    ? JSON.stringify({ boardSize, opponentStrategy })
    : JSON.stringify({ boardSize });
  const params = {
    headers: authHeaders(token),
    tags: { api: "create_game" }
  };

  const res = http.post(`${config.baseUrl}/api/games`, payload, params);

  const success = check(res, {
    "game created": (r) => r.status === 201,
    "has location header": (r) => r.headers["Location"] !== undefined
  });

  if (!success) {
    console.error(`Failed to create game: ${res.status} ${res.body}`);
    return null;
  }

  const location = res.headers["Location"];
  const gameId = location.split("/").pop();
  return gameId;
}

/**
 * Places a ship on the board
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @param {number} side - Board side (1 = Player, 2 = Opponent)
 * @param {number} shipKind - Ship kind (1-5)
 * @param {number} orientation - Orientation (1 = Vertical, 2 = Horizontal)
 * @param {string} bowCode - The bow position code (e.g., "A1")
 * @returns {boolean} Success status
 */
export function placeShip(token, gameId, side, shipKind, orientation, bowCode) {
  const payload = JSON.stringify({ side, shipKind, orientation, bowCode });
  const params = {
    headers: authHeaders(token),
    tags: { api: "add_ship" }
  };

  const res = http.post(
    `${config.baseUrl}/api/games/${gameId}/ships`,
    payload,
    params
  );

  return check(res, {
    "ship placed": (r) => r.status === 200
  });
}

/**
 * Attacks a cell on the board
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @param {string} cellCode - The cell code (e.g., "A1")
 * @returns {object|null} LastRoundResult or null if failed
 */
export function attack(token, gameId, cellCode) {
  const payload = JSON.stringify({ cell: cellCode });
  const params = {
    headers: authHeaders(token),
    tags: { api: "attack" }
  };

  const res = http.post(
    `${config.baseUrl}/api/games/${gameId}/attacks`,
    payload,
    params
  );

  const success = check(res, {
    "attack successful": (r) => r.status === 200
  });

  if (!success) {
    console.error(`Failed to attack: ${res.status} ${res.body}`);
    return null;
  }

  return res.json();
}

/**
 * Gets game information
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @returns {object|null} Game data or null if failed
 */
export function getGame(token, gameId) {
  const params = {
    headers: authHeaders(token),
    tags: { api: "get_game" }
  };

  const res = http.get(`${config.baseUrl}/api/games/${gameId}`, params);

  const success = check(res, {
    "game retrieved": (r) => r.status === 200
  });

  return success ? res.json() : null;
}

/**
 * Gets game state
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @returns {object|null} Game state or null if failed
 */
export function getGameState(token, gameId) {
  const params = {
    headers: authHeaders(token),
    tags: { api: "get_game_state" }
  };

  const res = http.get(`${config.baseUrl}/api/games/${gameId}/state`, params);

  const success = check(res, {
    "game state retrieved": (r) => r.status === 200
  });

  return success ? res.json() : null;
}

/**
 * Updates game state (transitions from Ready to Started)
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @returns {boolean} Success status
 */
export function updateGameState(token, gameId) {
  const payload = JSON.stringify({ state: "Started" });
  const params = {
    headers: authHeaders(token),
    tags: { api: "update_game_state" }
  };

  const res = http.put(
    `${config.baseUrl}/api/games/${gameId}/state`,
    payload,
    params
  );

  const success = check(res, {
    "game state updated": (r) => r.status === 200
  });

  if (!success) {
    console.error(`Failed to update game state: ${res.status} ${res.body}`);
  }

  return success;
}

/**
 * Places all ships for both players and opponent
 * @param {string} token - The caller's JWT access token
 * @param {string} gameId - The game ID
 * @returns {boolean} Success status
 */
export function placeAllShips(token, gameId) {
  const positions = [
    { bowCode: "A1", kind: 1, orientation: 2 }, // Destroyer
    { bowCode: "B2", kind: 3, orientation: 1 }, // Submarine
    { bowCode: "C3", kind: 2, orientation: 2 }, // Cruiser
    { bowCode: "D4", kind: 4, orientation: 1 }, // Battleship
    { bowCode: "E5", kind: 5, orientation: 2 } // Carrier
  ];

  for (const ship of positions) {
    // Place for player
    if (
      !placeShip(
        token,
        gameId,
        BoardSide.Player,
        ship.kind,
        ship.orientation,
        ship.bowCode
      )
    ) {
      return false;
    }

    // Place for opponent
    if (
      !placeShip(
        token,
        gameId,
        BoardSide.Opponent,
        ship.kind,
        ship.orientation,
        ship.bowCode
      )
    ) {
      return false;
    }
  }

  return true;
}

/**
 * Generates all cell positions for all ships
 * @returns {string[]} Array of cell codes
 */
export function generateShipPositions() {
  const positions = [
    { bowCode: "A1", size: 2, orientation: 2 }, // Destroyer horizontal
    { bowCode: "B2", size: 3, orientation: 1 }, // Submarine vertical
    { bowCode: "C3", size: 3, orientation: 2 }, // Cruiser horizontal
    { bowCode: "D4", size: 4, orientation: 1 }, // Battleship vertical
    { bowCode: "E5", size: 5, orientation: 2 } // Carrier horizontal
  ];

  const cells = [];

  for (const pos of positions) {
    const col = pos.bowCode.charCodeAt(0) - "A".charCodeAt(0);
    const row = parseInt(pos.bowCode.substring(1)) - 1;

    for (let i = 0; i < pos.size; i++) {
      if (pos.orientation === 2) {
        // Horizontal
        const cellCol = String.fromCharCode("A".charCodeAt(0) + col + i);
        cells.push(`${cellCol}${row + 1}`);
      } else {
        // Vertical
        const cellCol = String.fromCharCode("A".charCodeAt(0) + col);
        cells.push(`${cellCol}${row + 1 + i}`);
      }
    }
  }

  return cells;
}

/**
 * Generates a random username
 * @param {number} vuId - Virtual user ID
 * @returns {string} Username
 */
export function generateUsername(vuId) {
  const timestamp = Date.now();
  return `player_${vuId}_${timestamp}`;
}
