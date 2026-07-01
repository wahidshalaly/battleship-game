/**
 * Full Game Simulation - Complete game playthrough
 *
 * Purpose: Simulate one complete game from start to finish
 * VUs: 1
 * Iterations: 1 (plays exactly one complete game)
 */

import { sleep, check } from "k6";
import { config, GameState, CellState, OpponentStrategy } from "../config.js";
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
  iterations: 1,
  thresholds: config.defaultThresholds
};

export default function () {
  // 1. Register a user (creates identity + game profile) and get a bearer token
  const username = generateUsername(__VU);
  const token = registerAndGetToken(username);
  if (!token) {
    return;
  }

  // 2. Create game
  // Use environment variable to choose strategy: K6_OPPONENT_STRATEGY=SemanticKernel or Random
  // If not set, defaults to API's default (Random)
  const strategyEnv = __ENV.K6_OPPONENT_STRATEGY;
  const strategy = strategyEnv === "SemanticKernel" ? OpponentStrategy.SemanticKernel :
                   strategyEnv === "Random" ? OpponentStrategy.Random :
                   undefined; // Let API use default

  const gameId = createGame(token, config.boardSize, strategy);
  if (!gameId) {
    return;
  }

  console.log(`Creating game with strategy: ${strategy || 'default (Random)'}`);

  // Verify initial game state
  let game = getGame(token, gameId);
  if (game) {
    check(game, {
      "game is in New state": (g) => g.state === "New"
    });
  }

  // 3. Place all ships for both sides
  if (!placeAllShips(token, gameId)) {
    console.error("Failed to place ships");
    return;
  }

  sleep(1);

  // Verify boards are ready
  game = getGame(token, gameId);
  if (game) {
    check(game, {
      "game is in Ready state": (g) =>
        g.state === GameState.Ready
    });
  }

  // 4. Transition game to Started state
  if (!updateGameState(token, gameId)) {
    console.error("Failed to update game state");
    return;
  }

  sleep(0.5);

  // Verify game is started
  game = getGame(token, gameId);
  if (game) {
    check(game, {
      "game is in Started state": (g) => g.state === GameState.Started
    });
  }

  // 5. Attack all opponent ship positions to win the game
  const positions = generateShipPositions();
  let hitCount = 0;

  for (const cellCode of positions) {
    const result = attack(token, gameId, cellCode);

    if (result && result.playerAttackResult === "Hit") {
      hitCount++;
    }

    sleep(0.5); // Realistic delay between attacks
  }

  console.log(`Game ${gameId}: Hit ${hitCount}/${positions.length} cells`);

  // 6. Verify game is over
  game = getGame(token, gameId);
  if (game) {
    check(game, {
      "game is in GameOver state": (g) => g.state === GameState.GameOver
    });
  }

  sleep(2);
}
