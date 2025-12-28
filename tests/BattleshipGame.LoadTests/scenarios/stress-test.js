/**
 * Stress Test - High load to find breaking point
 *
 * Purpose: Push the API beyond normal load to find its limits
 * VUs: Ramp from 0 to 200 over 5 minutes
 * Duration: ~12 minutes total
 */

import { sleep } from "k6";
import { config } from "../config.js";
import {
  createPlayer,
  createGame,
  placeAllShips,
  generateShipPositions,
  attack,
  generateUsername
} from "../lib/game-helpers.js";

export const options = {
  stages: [
    { duration: "2m", target: 50 }, // Warm up
    { duration: "3m", target: 100 }, // Ramp to 100
    { duration: "2m", target: 150 }, // Ramp to 150
    { duration: "2m", target: 200 }, // Ramp to 200
    { duration: "3m", target: 0 } // Cool down
  ],
  thresholds: {
    http_req_failed: ["rate<0.05"], // Allow up to 5% errors under stress
    http_req_duration: ["p(95)<5000"], // 95% under 5s (relaxed for stress)
    "http_req_duration{api:create_player}": ["p(95)<1000"],
    "http_req_duration{api:create_game}": ["p(95)<1000"]
  }
};

export default function () {
  const username = generateUsername(__VU);
  const playerId = createPlayer(username);
  if (!playerId) {
    sleep(1);
    return;
  }

  const gameId = createGame(playerId);
  if (!gameId) {
    sleep(1);
    return;
  }

  if (!placeAllShips(gameId)) {
    sleep(1);
    return;
  }

  // Quick attacks
  const positions = generateShipPositions();
  for (let i = 0; i < Math.min(3, positions.length); i++) {
    attack(gameId, 2, positions[i]);
  }

  sleep(0.5);
}
