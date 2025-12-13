/**
 * Full Game Simulation - Complete game playthrough
 *
 * Purpose: Simulate complete games from start to finish
 * VUs: 10 concurrent complete games
 * Duration: 10 minutes
 */

import { sleep, check } from "k6";
import { config, GameState, CellState } from "../config.js";
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
  vus: 10,
  duration: "10m",
  thresholds: config.defaultThresholds
};

export default function () {
  // 1. Create player
  const username = generateUsername(__VU);
  const playerId = createPlayer(username);
  if (!playerId) {
    return;
  }

  // 2. Create game
  const gameId = createGame(playerId);
  if (!gameId) {
    return;
  }

  // Verify initial game state
  let game = getGame(gameId);
  if (game) {
    check(game, {
      "game is in Started state": (g) => g.state === GameState.Started
    });
  }

  // 3. Place all ships for both sides
  if (!placeAllShips(gameId)) {
    console.error("Failed to place ships");
    return;
  }

  sleep(1);

  // Verify boards are ready
  game = getGame(gameId);
  if (game) {
    check(game, {
      "game is in BoardsAreReady state": (g) =>
        g.state === GameState.BoardsAreReady
    });
  }

  // 4. Attack all opponent ship positions to win the game
  const positions = generateShipPositions();
  let hitCount = 0;

  for (const cellCode of positions) {
    const cellState = attack(gameId, 2, cellCode); // Attack opponent side

    if (cellState === CellState.Hit) {
      hitCount++;
    }

    sleep(0.5); // Realistic delay between attacks
  }

  console.log(`Game ${gameId}: Hit ${hitCount}/${positions.length} cells`);

  // 5. Verify game is over
  game = getGame(gameId);
  if (game) {
    check(game, {
      "game is in GameOver state": (g) => g.state === GameState.GameOver
    });
  }

  sleep(2);
}
