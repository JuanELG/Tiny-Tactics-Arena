# 🚀 Tiny Tactics Arena — Unity DOTS Single-Player Experiment

Personal project built on top of the **Unity DOTS Samples** repository (Unity 6, Entities 1.3). The goal was to explore ECS/DOTS architecture by converting the original multiplayer Asteroids sample into a **single-player tactical arena game**.

> **Role:** Solo Developer — Gameplay Systems, Scene Design, AI Concept, Unit Placement  
> **Engine:** Unity 6 · **Architecture:** ECS / DOTS (Entities, Physics, Netcode for Entities) · **Language:** C# (95%), ShaderLab (4%)  
> **Base:** Fork of [Unity-Technologies/EntityComponentSystemSamples](https://github.com/Unity-Technologies/EntityComponentSystemSamples)

---

## 🎮 Game Concept

A single-player tactical arena where the player must destroy enemies and obstacles to survive:

- **Asteroids as obstacles** — randomly placed in the center of the arena, blocking the path to the enemy.
- **Destroy to advance** — clear the field of asteroids to reach and eliminate the AI-controlled enemy.
- **Enemy AI** *(in design)* — the enemy fires projectiles at the player; dodge or use shields.
- **Unit placement system** — the player can deploy asteroids on their side of the arena to act as **shields** against incoming enemy projectiles.

---

## 🔬 What I Explored with DOTS

This project was a hands-on exploration of Unity's ECS/DOTS stack in a gameplay context:

| Area | What I Learned |
|---|---|
| **Entities & Components** | Structuring game data as pure ECS components (position, health, faction) |
| **Systems** | Writing gameplay logic as Systems that operate on component queries |
| **Unity Physics** | Physics-based projectile movement and asteroid collision using DOTS Physics |
| **Netcode for Entities** | Understood server-authoritative model and client prediction (original multiplayer base) |
| **Baking** | Converting GameObjects to Entities via the baking workflow |
| **Random placement** | Spawning asteroids at randomized center-arena positions via ECS spawner systems |

---

## 🏗️ Architecture Overview

```
Original:  Multiplayer Asteroids (Netcode for Entities)
Modified:  Single-player arena with tactical unit placement
           └── Random obstacle spawning (asteroid field)
           └── Player ship + projectile system
           └── Shield placement mechanic (player-deployed asteroids)
           └── AI enemy concept (not yet implemented)
```

---

## 📌 Status

| Feature | Status |
|---|---|
| Random asteroid obstacle placement | ✅ Done |
| Player movement & shooting | ✅ Done |
| Shield unit placement | ✅ Done |
| Enemy AI | 🔲 Designed, not implemented |
| Win/Lose conditions | 🔲 Pending |

---

## 🤖 Why DOTS?

DOTS was chosen specifically because of its performance profile for entity-heavy simulations — a large number of asteroids, projectiles, and units all moving simultaneously benefits significantly from ECS data layout and the Job System. This project was a personal learning investment in preparation for working on larger-scale games.

---

*Base samples copyright Unity Technologies. Personal gameplay modifications and scene design by Juan Esteban Leal.*
