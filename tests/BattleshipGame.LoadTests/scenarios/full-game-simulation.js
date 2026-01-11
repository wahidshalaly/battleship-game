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
  updateGameState,
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
      "game is in New state": (g) => g.state === "New"
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
      "game is in Ready state": (g) =>
        g.state === GameState.Ready
    });
  }

  // 4. Transition game to Started state
  if (!updateGameState(gameId)) {
    console.error("Failed to update game state");
    return;
  }

  sleep(0.5);

  // Verify game is started
  game = getGame(gameId);
  if (game) {
    check(game, {
      "game is in Started state": (g) => g.state === GameState.Started
    });
  }

  // 5. Attack all opponent ship positions to win the game
  const positions = generateShipPositions();
  let hitCount = 0;

  for (const cellCode of positions) {
    const result = attack(gameId, cellCode);

    if (result && result.playerAttackResult && result.playerAttackResult.hit) {
      hitCount++;
    }

    sleep(0.5); // Realistic delay between attacks
  }

  console.log(`Game ${gameId}: Hit ${hitCount}/${positions.length} cells`);

  // 6. Verify game is over
  game = getGame(gameId);
  if (game) {
    check(game, {
      "game is in GameOver state": (g) => g.state === GameState.GameOver
    });
  }

  sleep(2);
}
