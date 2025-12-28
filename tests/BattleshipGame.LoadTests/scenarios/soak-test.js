/**
 * Soak Test - Long-running stability test
 *
 * Purpose: Test system stability over an extended period
 * VUs: 20 constant users
 * Duration: 30 minutes
 *
 * Note: This test is designed to find memory leaks and degradation over time
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
  vus: 20,
  duration: "30m",
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<3000"],
    "http_req_duration{api:create_player}": ["p(95)<500"],
    "http_req_duration{api:create_game}": ["p(95)<500"],
    "http_req_duration{api:add_ship}": ["p(95)<500"],
    "http_req_duration{api:attack}": ["p(95)<1000"]
  }
};

export default function () {
  const username = generateUsername(__VU);
  const playerId = createPlayer(username);
  if (!playerId) {
    sleep(2);
    return;
  }

  const gameId = createGame(playerId);
  if (!gameId) {
    sleep(2);
    return;
  }

  // Verify game creation
  const game = getGame(gameId);
  if (!game) {
    sleep(2);
    return;
  }

  if (!placeAllShips(gameId)) {
    sleep(2);
    return;
  }

  // Play partial game
  const positions = generateShipPositions();
  const attackCount = Math.floor(Math.random() * positions.length);

  for (let i = 0; i < attackCount; i++) {
    attack(gameId, 2, positions[i]);
    sleep(1); // Simulate realistic user behavior
  }

  sleep(2);
}
