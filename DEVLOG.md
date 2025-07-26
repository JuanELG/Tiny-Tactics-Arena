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

| Change #  | Task                                    | Commit / Artifact   	      |
| --------- | --------------------------------------- | ----------------------------- |
|    1      | START: Fork Asteroids repo              | 		              |
|    2      | Add initial DEVLOG.md	              | db350075		      |
|    3      | Add multiplayer playmode package        | b2fd2250		      |
---

## 3. Key Design Decisions

*  
---

## 4. AI Usage

* **ChatGPT:**

  * In-depth investigation of Unity DOTS and questions and answers from the basics

* **GitHub Copilot:**

  * Generation of commit messages and necessary documentation for the project

* **AI Leverage:**

  * ChatGPT helped resolve questions and deep research on DOTS in 3 hours (vs. ~8 hours of searching through documents and videos).
---

## 5. Optimization and Performance Evidence

* **Tool:** Unity Profiler (Android Device)
* **Initial metrics:**

  * 

* **Actions:**

  1. 
* **Result:**

  * 
  * 
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

* 
---

> **Note:** Include commit references and captures in `/Docs/` folder for the live demo.
