/**
 * Load Test - Normal load simulation
 *
 * Purpose: Simulate normal expected load on the API
 * VUs: Ramp from 0 to 50 over 2 minutes, hold for 5 minutes, ramp down
 * Duration: ~10 minutes total
 */

import { sleep } from "k6";
import { config } from "../config.js";
import { registerAndGetToken } from "../lib/auth-helpers.js";
import {
  createGame,
  placeAllShips,
  generateShipPositions,
  attack,
  getGameState,
  updateGameState,
  generateUsername
} from "../lib/game-helpers.js";

export const options = {
  stages: [
    { duration: "2m", target: 50 }, // Ramp up to 50 VUs
    { duration: "5m", target: 50 }, // Stay at 50 VUs
    { duration: "2m", target: 0 } // Ramp down to 0
  ],
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

  // Place all ships
  if (!placeAllShips(token, gameId)) {
    return;
  }

  // Update game state to Started
  if (!updateGameState(token, gameId)) {
    console.error("Failed to update game state");
    return;
  }

  // Check game state
  getGameState(token, gameId);

  // Attack some cells (simulate partial game)
  const positions = generateShipPositions();
  const attackCount = Math.floor(Math.random() * 10) + 5; // 5-15 attacks

  for (let i = 0; i < Math.min(attackCount, positions.length); i++) {
    attack(token, gameId, positions[i]);
    sleep(0.5);
  }

  sleep(1);
}
