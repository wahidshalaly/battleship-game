// Configuration for K6 load tests
export const config = {
  // Base URL for the API - override via environment variable
  baseUrl: __ENV.API_BASE_URL || "https://localhost:7127",

  // Default board size
  boardSize: 10,

  // Thresholds for all tests
  defaultThresholds: {
    http_req_failed: ["rate<0.01"], // Less than 1% errors
    http_req_duration: ["p(95)<2000"], // 95% under 2s
    "http_req_duration{api:create_player}": ["p(95)<500"],
    "http_req_duration{api:create_game}": ["p(95)<500"],
    "http_req_duration{api:add_ship}": ["p(95)<500"],
    "http_req_duration{api:attack}": ["p(95)<1000"],
    "http_req_duration{api:get_game}": ["p(95)<300"]
  }
};

// Ship definitions for placing on the board
export const shipDefinitions = [
  { kind: 1, name: "Destroyer", size: 2, orientation: 2 }, // Horizontal
  { kind: 3, name: "Submarine", size: 3, orientation: 1 }, // Vertical
  { kind: 2, name: "Cruiser", size: 3, orientation: 2 }, // Horizontal
  { kind: 4, name: "Battleship", size: 4, orientation: 1 }, // Vertical
  { kind: 5, name: "Carrier", size: 5, orientation: 2 } // Horizontal
];

// Board sides
export const BoardSide = {
  None: 0,
  Player: 1,
  Opponent: 2
};

// Ship orientations
export const ShipOrientation = {
  None: 0,
  Vertical: 1,
  Horizontal: 2
};

// Game states
export const GameState = {
  Started: "Started",
  BoardsAreReady: "BoardsAreReady",
  GameOver: "GameOver"
};

// Cell states
export const CellState = {
  Unknown: 0,
  Empty: 1,
  Occupied: 2,
  Hit: 3,
  Miss: 4
};
