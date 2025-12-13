/**
 * Spike Test - Sudden traffic burst
 *
 * Purpose: Test how the system handles sudden spikes in traffic
 * VUs: Sudden jump from 10 to 200, then back down
 * Duration: ~7 minutes total
 */

import { sleep } from "k6";
import { config } from "../config.js";
import {
  createPlayer,
  createGame,
  placeAllShips,
  attack,
  generateUsername
} from "../lib/game-helpers.js";

export const options = {
  stages: [
    { duration: "1m", target: 10 }, // Normal load
    { duration: "30s", target: 200 }, // Spike!
    { duration: "2m", target: 200 }, // Hold spike
    { duration: "30s", target: 10 }, // Drop back
    { duration: "2m", target: 10 }, // Recover
    { duration: "1m", target: 0 } // Cool down
  ],
  thresholds: {
    http_req_failed: ["rate<0.05"],
    http_req_duration: ["p(95)<5000"]
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

  // Quick ship placement
  placeAllShips(gameId);

  // Single attack
  attack(gameId, 2, "A1");

  sleep(0.5);
}
