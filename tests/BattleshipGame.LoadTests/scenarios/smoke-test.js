/**
 * Smoke Test - Basic functionality check
 *
 * Purpose: Verify that the API is working correctly with minimal load
 * VUs: 1
 * Duration: 1 minute
 */

import { sleep } from "k6";
import { config } from "../config.js";
import {
  createPlayer,
  createGame,
  placeAllShips,
  generateShipPositions,
  attack,
  getGame,
  generateUsername
} from "../lib/game-helpers.js";

export const options = {
  vus: 1,
  duration: "1m",
  thresholds: config.defaultThresholds
};

export default function () {
  // Create player
  const username = generateUsername(__VU);
  const playerId = createPlayer(username);
  if (!playerId) {
    return;
  }

  // Create game
  const gameId = createGame(playerId);
  if (!gameId) {
    return;
  }

  // Verify game was created
  const game = getGame(gameId);
  if (!game) {
    return;
  }

  // Place all ships
  if (!placeAllShips(gameId)) {
    console.error("Failed to place ships");
    return;
  }

  // Attack a few cells
  const positions = generateShipPositions();
  for (let i = 0; i < Math.min(5, positions.length); i++) {
    attack(gameId, 2, positions[i]); // Attack opponent side
    sleep(0.5);
  }

  sleep(1);
}
