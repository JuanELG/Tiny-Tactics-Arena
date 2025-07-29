# DEVLOG.md

> Development log for the “Tiny Tactics Arena” challenge — Path A: Fork & Transform
> Duration: 72 hours
---

## 1. Project Overview

* **Challenge Name:** Tiny Tactics Arena
* **Chosen Path:** A. Fork & Transform (Asteroids sample)
* **Main Objective:** Transform the base game into a tactical arena with placement and combat phases.
* **Target Platform:** Android/iOS (≮45 FPS on mid-tier devices)

---

## 2. Change Log

| Change #  | Task                                                       | Commit / Artifact   	      	 |
| --------- | ---------------------------------------------------------- | ----------------------------- |
|    1      | START: Fork Asteroids repo              			 | 		              	 |
|    2      | Add initial DEVLOG.md	              			 | db350075		      	 |
|    3      | Add multiplayer playmode package        			 | b2fd2250		      	 |
|    4      | WIP: Implement player zones(AI usage)      		 | 928f41df		      	 |
|    5      | Remove Netcode for entities		      		 | 4715e29e		      	 |
|    6      | Remove netcode samples		      			 | b4527fb7		      	 |
|    7      | Remove multiplayer scripts(AI usage)      		 | 3e58c38a		      	 |
|    8      | Refactor useful scripts for single-player(AI usage)      	 | 2ab42bcb		      	 |
|    9      | Update level settings and input handling(AI usage)	 | 5e01542e		      	 |
|    10     | Refactor Asteroids systems and update scenes(AI usage)	 | 6b631512		      	 |
|    11     | Adjust asteroid and player zone spawn positions(AI usage)	 | c879ab34		      	 |
|    12     | Game phase system and ship deployment with touch(AI usage) | 8779738d		      	 |
|    13     | Collision zones and improve ship placement logic(AI usage) | 3e4e0a28		      	 |
|    14     | asteroid placement phase, refactor placement...(AI usage) 	 | a6db8499		      	 |
---

## 3. Key Design Decisions

* NetCode removed for rapid prototyping: To accelerate development, NetCode was removed and multiplayer-dependent systems were refactored to work in single player mode.
This simplified the architecture for the challenge while keeping it extensible for future multiplayer reintegration.
* Separated gameplay logic per mode (SinglePlayer vs Multiplayer) using Assembly Definitions and defineConstraints, ensuring exclusive system compilation and execution per scene or platform
* Applied ECS architectural patterns:
  * Composition of behavior via components
  * Modularization by feature-based folder structure
  * Extraction of pure Burst-safe logic to reusable static helpers
* State machines are used to manage the new phases of the game flow and to have greater control over the functionalities in each of them.
---

## 4. AI Usage

* **ChatGPT:**

  * In-depth investigation of Unity DOTS and questions and answers from the basics
  * Deep refactoring of the scripts used for multiplayer, allowing  their use in single player mode.
  * Use of the expert panel strategy through prompts to receive better answers and compare ideas regarding architecture, design, and DOTS concepts.
  * Bug fixing.
  * Implementation of new tactics arena features.

* **GitHub Copilot:**

  * Generation of commit messages and necessary documentation for the project
  * Quickly and efficiently autocomplete code

* **AI Leverage:**

  * ChatGPT helped resolve questions and deep research on DOTS in 3 hours (vs. ~8 hours of searching through documents and videos).
  * ChatGPT helped refactor and understand multiplayer scripts in 10 hours. (vs. ~48 hours of understanding and manual coding / LOC saved ~3500 lines of code)
  * ChatGPT helped find better design solutions to apply to the project code in 3 hours. (vs. ~12 hours researching the application of design and architecture patterns for DOTS in documentation)
  * ChatGPT helped with bug fixing in the scripts created for the new features in ~4 hours. (vs. ~12 hours manually fixing each of the bugs found / LOC saved ~400 lines of code)
  * ChatGPT helped with the implementation of new tactical arena features in code in 12 hours. (vs ~24 hours manual implementation of new features / LOC saved ~1500 lines of code)
  * Github Copilot helped with code autocompletion. (LOC saved ~600 lines of code)
  * Github Copilot helped generate commit messages for changes uploaded to the repository. (Time saved ~2 hours manual writing of each commit message)
---

## 5. Optimization and Performance Evidence

* **Tool:** Unity Profiler (Android Device)
* **Initial metrics:**

  * CPU Frame Time: ~33.5 ms (≈30 FPS)
     * Main bottleneck: WaitForLastPresent caused by VSync
  * GC Alloc per frame: negligible (only 70 B occasionally)
     * Memory usage: ~670 MB total, no significant spikes

* **Actions:**

  1. Disabled VSync to prevent frame waiting
  2. Forced target frame rate to 60 FPS
  3. Disabled unnecessary render features
  4. Set Universal Render Pipeline settings for mobile

* **Result:**

  * CPU Frame Time: dropped from ~33 ms to ~16.6 ms, achieving a stable 60 FPS
  * Main Thread Idle Time: increased, confirming system headroom
  * Memory usage: remains consistent (~0.67 GB)
     * GPU workload stabilized (based on profiler bars)
     * Rendering: reduced draw overhead confirmed in Frame Debugger
* **Screenshots:** 
---

## 6. Trade-offs and Reflections

* To ensure a realistic and polished delivery within the 72-hour constraint, I made a key decision to remove the dependency on Netcode for Entities and rework the functionality into a local singleplayer tactical game. Instead, by pivoting to a local DOTS-based architecture, I retain the core performance, structure, and data-oriented benefits while accelerating gameplay iteration and polish.
    * Adventages: 
     * Faster iteration cycle with no network setup.
     * Easier to debug and test gameplay logic.
     * Enables focus on tactical mechanics, UI feedback, and visual polish.
     * Keeps DOTS benefits and integrates cleanly with the project’s ECS structure.
    * Disadvantages:
     * Multiplayer functionality is no longer available.
     * Some base systems (e.g., LoadLevelSystem, Spawner, RPC flows) must be refactored to work offline.
* How I mitigate the trade-off:
     * Architecture keeps compatibility with DOTS patterns and could be extended to multiplayer in the future.
     * System replacements maintain structural alignment with the original project for clarity.
     * Using LLMs such as ChatGPT to save development time.
---

## 7. Next Steps / Pending Improvements

* Extend shared logic extraction to more systems (e.g. collision, scoring)
* Add test coverage for pure logic modules
* Refactor all systems to consistently follow new mode-based architecture (e.g. SinglePlayerAsteroidSystem, MultiplayerAsteroidSystem using shared logic)
---

> **Note:** Include commit references and captures in `/Docs/` folder for the live demo.
