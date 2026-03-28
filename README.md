# Catan Tracker — Backend

A learning project. Backend for tracking a physical game of Catan — players report what happens (resources, dev cards, reputation changes) and the app keeps score, manages turn order, and saves game history.

---

## Stack

- .NET 10 / ASP.NET Core
- PostgreSQL + EF Core
- Fleck (WebSockets)
- JWT auth, PBKDF2 password hashing

---

## How it works

There are two servers running at once:

**REST API** — auth, teams, game history
**WebSocket server on port 8181** — live game sessions

When a game is created via REST, it gets a session ID. Players connect to the WebSocket with that session ID and a JWT. From there, all game events (next turn, resource update, end game) go over the socket.

The main chain is: `WsHandler → GameSession → GameService → GameState`. The repository layer handles all DB access.

Messages have two parts — `Broadcast` (goes to all players) and `Private` (goes only to the sender). Errors are always private.

---

## What's working

- Register, login
- Create and join teams
- Create a game session, join it over WebSocket
- Turn order with optional countdown timer
- Track victory points and dev cards per player
- Reputation — players rate each other, average is stored at end
- End game — picks winner by points, saves everything to DB
- Game history per team

## What's not done

- Any actual Catan board mechanics (dice, building, trading)
- No input validation — no required field checks, no length limits
- No tests
- Known race condition if the timer and a player advance the turn at the same time
