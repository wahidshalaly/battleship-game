/**
 * Smoke Test - Basic functionality check
 *
 * Purpose: Verify that the API is working correctly with minimal load
 * VUs: 1
 * Duration: 1 minute
 */

import { sleep } from "k6";
import { config } from "../config.js";
import { registerAndGetToken } from "../lib/auth-helpers.js";
import {
  createGame,
  placeAllShips,
  generateShipPositions,
  attack,
  getGame,
  updateGameState,
  generateUsername
} from "../lib/game-helpers.js";

export const options = {
  vus: 1,
  duration: "1m",
  thresholds: config.defaultThresholds
};

export default function () {
  // Register a user (creates identity + game profile) and get a bearer token
  const username = generateUsername(__VU);
  const token = registerAndGetToken(username);
  if (!token) {
    return;
  }

  // Create game
  const gameId = createGame(token);
  if (!gameId) {
    return;
  }

  // Verify game was created
  const game = getGame(token, gameId);
  if (!game) {
    return;
  }

  // Place all ships
  if (!placeAllShips(token, gameId)) {
    console.error("Failed to place ships");
    return;
  }

  // Update game state to Started
  if (!updateGameState(token, gameId)) {
    console.error("Failed to update game state");
    return;
  }

  sleep(0.5);

  // Attack a few cells
  const positions = generateShipPositions();
  for (let i = 0; i < Math.min(5, positions.length); i++) {
    attack(token, gameId, positions[i]);
    sleep(0.5);
  }

  sleep(1);
}
