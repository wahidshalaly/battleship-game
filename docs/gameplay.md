# How to Play

A player's guide to the Battleship game's web UI. For running the app locally, see
[local-dev.md](./local-dev.md).

## The goal

Battleship is a turn-based naval game. You and a computer opponent each have a 10×10 grid
with a hidden fleet of five ships. Take turns firing at each other's grid; the first to sink
the other's entire fleet wins.

Each fleet has the same five ships:

| Ship       | Size (cells) |
| ---------- | ------------ |
| Carrier    | 5            |
| Battleship | 4            |
| Cruiser    | 3            |
| Submarine  | 3            |
| Destroyer  | 2            |

That's **17 cells** to sink in total.

## 1. Open the app

Start the backend and frontend (see [local-dev.md](./local-dev.md)), then open
**http://localhost:5173** in your browser.

## 2. Create an account or sign in

- **Register** with a username, email, and password to create a new account.
- Or **Sign in** if you already have one.

Your session is remembered, so refreshing the page keeps you signed in. Use **Sign out** in
the top bar to end the session.

## 3. Start a new game

On the lobby screen, choose your opponent and create the game:

- **Random (fast)** — the opponent fires at random cells. Quick games, good for trying things
  out.
- **AI (Semantic Kernel)** — the opponent uses an LLM to choose its shots. Requires the AI
  backend to be configured (see [local-dev.md](./local-dev.md#ai-opponent-optional)); otherwise
  use Random.

If you already have a game in progress, the lobby shows **Resume game** instead.

## 4. Place your ships

You place all five of your ships before the battle begins:

1. Pick the **orientation** — _Horizontal_ (the ship extends to the right) or _Vertical_ (the
   ship extends downward).
2. Select a ship from the **Ships** list on the right (the largest is selected first).
3. **Click a cell** on your board to drop the ship's bow (front) there. A highlighted preview
   shows where it will land; the placement is rejected if it would run off the board or overlap
   another ship.

Placed ships are ticked off the list. Once all five are down, a **Start game** button appears.

> You only place _your_ ships. The opponent's fleet is positioned automatically (and stays
> hidden from you) when you start the game.

## 5. Battle

The screen shows two grids:

- **Opponent** (left) — click any un-fired cell to attack it.
- **Your fleet** (right) — your ships, and where the opponent has fired at you.

Each time you fire, the round resolves immediately: your shot lands, then the opponent fires
back once. Cells are colored by result:

- 🔴 **Red** — a hit.
- ⚫ **Grey** — a miss.

When a ship is destroyed, a message tells you which one ("You sank the opponent's Cruiser!" or
"Your Destroyer was sunk!"). You can't fire at the same cell twice.

## 6. Winning

Sink all five opponent ships (17 cells) before the opponent sinks yours. When the game ends, a
banner announces the result and you can return to the lobby to play again.

## Resuming a game

If you leave mid-game, signing back in (or returning to the lobby) lets you **Resume** it — the
game state lives on the server. Note that your own ship layout is remembered in the browser tab
you placed them in; if you resume in a different browser or tab, your fleet outline may not be
drawn, but incoming hits and misses are still tracked and the game plays normally.
