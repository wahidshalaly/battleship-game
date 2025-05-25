# Redsign of Game Engine

## Analysis


### Components

Cell
- Consists of two things, a Capital letter and a digit, e.g. `D4` or `J5`
- Is either clear, occupied (part of a ship), or hit (whether it was clear or occupied before)
- When cell is occupied, it'll be assigned a `ShipId`
- When a cell is occupied, it cannot be re-assigned again.
- When a cell is hit, it cannot be attacked again.

Ship
- Has a start and end, stationed as vertical or horizontal
- Has a size from 2 to 5 cells maximum, based on ship kind
- Takes a hit when a cell it occupies receives a hit
- Sinks when all cells it occupies are attacked

Board
- Has a size of minimum of 10 (default) or maximum of 26
- Has a number of cells equal the square number of the board size
- Has only valid cells within its size limit, e.g. a board of size 10, can't have `K1` or `J11`
- Has one ship of each kind, in total 5 ships.

Game
- Has two players, Player1 (Human) and Player2 (AI)
- Has two boards, a board for each player
- Has a current state

Game State
- Either: Setup, Ready, Active, or Complete
- Has initial state of Setup, when a Game needs to be initiated
- Has state of Ready, when a Game gets initiated
- Has state of Active, when player turns start
- Has state of Complete when a player wins, which is once a player hits all opponents ships first.

Game Controller/Engine
- Setup
	- Creates two players, Player1 (Human) and Player2 (AI)
	- Creates two boards, a board for each player
	- Gives turns for players to place ships on board
	- When setup is complete, the Game State becomes Ready
- Player Turns
	- When turns start, the Game State becomes Active
	- Gives turns for players to attack other player's board
- Game Assessment
	- Assesses situation after every turn/attack
	- Announces a winner once a player sunk all opponent's ships
	- When no winner, then continue give other player a turn
