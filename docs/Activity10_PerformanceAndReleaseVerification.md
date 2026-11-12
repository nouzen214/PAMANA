# Activity 10: Performance Optimization, Testing, and Release Verification — Laboratory Report

**Course:** System Fundamentals Lab (UPHSL College of Computer Studies)  
**Section:** J4T | **Group:** IT9  
**Submitted by Group IT9:**  
- **Masculino, Jhon Darrel C.**  
- **Anahaw, Samantha Isabel N.**  
- **Policarpio, Lance Angelo A.**  
- **Sy, Stephanie Sofia V.**  

**Submitted to:** Ma’am Grachel Eliza C. Greta  
**Project:** PAMANA (Philippine Heritage Educational 2D Top-Down WebGL RPG)  
**Date:** November 2026  

---

## PART I. PERFORMANCE CONTEXT AND RESOURCE APPLICABILITY (15 Points)

### Environment Specification
The environment where the PAMANA WebGL application is evaluated and verified is documented below:

| Specification Item | Details |
| :--- | :--- |
| **Project Title** | PAMANA: A 2D Top-Down Cultural Heritage Educational RPG |
| **Current Version** | `0.4.0` (Baseline) / `0.4.1` (Optimized Verification Build) |
| **Target Platform** | Web (WebGL 2.0 / HTML5 Browser Deployment) |
| **Application Type** | Game / Web |
| **Test Device / Computer** | Windows 11 PC Workstation (ASUS / Dell Laptop Testing Rig) |
| **Operating System** | Windows 11 Home 64-bit (Version 23H2 / Build 22631) |
| **Processor** | 12th Gen Intel(R) Core(TM) i7-12700H @ 2.30 GHz (14 Cores, 20 Threads) |
| **Memory / RAM** | 16.0 GB DDR4 RAM @ 3200 MHz |
| **Network Connection** | Broadband Wi-Fi (PLDT Home Fiber, ~100 Mbps) & Local HTTP Host |
| **Profiling / Monitoring Tools Used** | Unity Profiler (CPU Timeline, Memory Module, Deep Profile), Unity Frame Debugger, Google Chrome DevTools (Performance, Memory Heap Snapshots, Network Waterfall) |

---

### Expected Behavior of the Application

| Performance Area | Expected Behavior | Why It Matters |
| :--- | :--- | :--- |
| **Startup / Loading** | The WebGL build loads from the hosted site (GitHub Pages or local server) and reaches the Main Menu within 3–5 seconds on standard school-grade computers, with the progress bar advancing smoothly without freezing or crashing the tab. | Players open the game directly in a web browser; a slow, hanging, or unresponsive first load leads to user bounce or causes students to assume the application is broken. |
| **Runtime Responsiveness** | Player avatar movement, dialogue interactions, inventory menus, and Alterworld quiz battles respond immediately to user inputs (60 FPS on desktop, stable ≥30 FPS on low-end hardware) without perceptible frame stutter. | Delayed avatar response or lagging quiz answer buttons disrupt the educational flow, leading to misclicks and player frustration. |
| **Memory Usage** | In-tab browser RAM consumption remains stable (under 500 MB) and does not accumulate over time as the player travels between the Main Lobby, museum hallways, and Alterworld trial arenas. | Browsers enforce strict WebAssembly memory ceilings per tab. Unchecked memory leaks eventually cause browser out-of-memory crashes (`WebGL context lost`). |
| **Save / Load** | Player progress (chosen avatar, field guide book unlock, recovered museum artifacts, and room access) saves within 100–200 ms and loads seamlessly upon login or page refresh without blocking gameplay. | Players must never lose earned historical knowledge or recovered relics. A slow save mechanism locks the main thread and interrupts exploration. |
| **Network Response (if applicable)** | Firebase Authentication, profile sync, and leaderboard retrieval complete quickly, with an automated fallback to local browser `PlayerPrefs` if the network connection drops. | Philippine school computer labs experience varying network stability; graceful offline fallback ensures uninterrupted classroom learning. |

---

### Resource Applicability Matrix

| Resource | Applicable? | Where It Affects the System | Possible Concern |
| :--- | :---: | :--- | :--- |
| **CPU** | **Yes** | Per-frame processing in 2D exploration, sprite sorting calculations, collision checks, enemy quiz battle states, and UI rendering on the browser's single WebAssembly/JavaScript thread. | Unoptimized per-frame scripts (`LateUpdate()`) or excessive math on static scene objects can lower FPS and cause micro-stutters on low-end school laptops. |
| **Memory / RAM** | **Yes** | Scene loading, sprite atlases, audio clips, and instantiated UI canvases retained inside the browser tab’s WebAssembly memory heap. | Assets or UI dialogs instantiated repeatedly without garbage collection control can cause heap growth and GC spikes during extended play sessions. |
| **Network** | **Yes** | Initial WebGL build payload download (~25.7 MB compressed) from GitHub Pages, along with Firebase Authentication and Realtime Database REST API sync requests. | High latency or packet drops during asset download or cloud sync could freeze the application if asynchronous operations are unmanaged. |
| **Disk / Storage** | **No** | The client game performs no local physical disk file reads or writes. In WebGL, client persistence is handled entirely via browser IndexedDB / `PlayerPrefs` key-value storage. | None. The game performs no disk-intensive operations, and the local state data footprint is negligible (<50 KB). |
| **GPU** | **No** | Rendering pipeline is restricted to lightweight 2D unlit sprites, pixel-art tilemaps, and Canvas UI using the Universal Render Pipeline (URP 2D) without heavy 3D post-processing. | None. Modern integrated graphics (Intel Iris Xe / AMD Radeon) handle 2D sprite rendering at locked 60 FPS with less than 5% GPU utilization. |

---

## PART II. FUNCTIONAL VALIDATION AND PERFORMANCE BASELINE (20 Points)

### Scenario Selection
- **Selected System Scenario:** *Main Lobby Exploration, Book Pedestal Proximity Interaction, Field Guide Unlock, and Book Modal UI Access.*
- **Scenario Description:** The player loads the `Lobby` scene with a selected student avatar, moves through the museum hall, approaches the newly restored `BookPedestal`, interacts to unlock the Field Guide, verifies visual depth sorting around the pedestal, and opens the open-book modal interface via the HUD button and keyboard shortcuts.

---

### A. Functional Validation (Three Core Functional Tests)

| Requirement / Feature | Test Action / Input | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :---: |
| **1. Player Character Movement & Collision Constraints** | Press [W], [A], [S], [D] or Arrow keys to navigate the avatar across the `Lobby` floor tiles toward the walls and central pedestal. | Avatar moves smoothly at designated walking speed; physical boundaries (`RoomWalls` and `BookPedestal` box colliders) prevent the character from walking through solid walls or out of bounds. | Avatar moved fluidly; all collider perimeters stopped character penetration cleanly without clipping. | **Pass** |
| **2. Book Pedestal Proximity Interaction & Field Guide Unlock** | Walk within trigger distance (1.6 units) of the `BookPedestal` in the Main Lobby and press the interaction key [E] / click the interaction prompt. | Displays interaction banner ("FIELD GUIDE UNLOCKED!"), registers `IsBookUnlocked = true` in `GameProgression`, triggers floating book pickup animation, and activates the HUD Book button in the bottom-right corner. | Unlock banner popped up with gold accents; sound played; HUD book button became permanently visible. | **Pass** |
| **3. Field Guide Book Modal Activation & Page Pagination** | Click the HUD Book button (or press [H] / [F1]), click the navigation arrow buttons ([<] Prev / [>] Next), and press [ESC] / close button. | Open-book modal UI overlays smoothly over dimmed background; page turning responds immediately; game timescale pauses cleanly; closing dismisses modal and restores movement. | Book frame opened instantly with zero hitching; arrow buttons registered clicks; closed cleanly returning control to player. | **Pass** |

---

### B. Performance Baseline (Baseline Measurements Prior to Optimization)

Measurements were recorded on Version `0.4.0` in the `Lobby` scene under consistent hardware and testing conditions.

| Metric | Test Scenario | Baseline Result | Tool Used |
| :--- | :--- | :--- | :--- |
| **Average Framerate & Frame Time** | Continuous player movement around `Lobby` with active sprite renderers, static props, and UI Canvas. | **58.8 FPS** (17.01 ms frame time) with occasional dips to **51.2 FPS** (19.5 ms) during interaction triggers. | Unity Profiler (CPU Usage) & Chrome DevTools Performance Panel |
| **Per-Frame Sorting Script CPU Overhead** | Profiling `YSort.LateUpdate()` execution across all scene objects (Player, BookPedestal, decorative furniture). | **0.82 ms** per frame CPU time consumed by dynamic sorting recalculations on stationary objects. | Unity Profiler (CPU Timeline - Deep Profiling mode) |
| **Garbage Collection (GC Alloc) on Modal Toggle** | Opening and closing the Field Guide Book Modal and Inventory Bag Modal during exploration. | **54.2 KB** GC Alloc spike per modal trigger, producing an intermittent **14.8 ms** garbage collection pause every 6–8 toggles. | Unity Profiler (GC Alloc counter) & Chrome Memory Allocation Profiler |
| **Cold Startup & WebGL Download Duration** | Cold launch of the WebGL build from local HTTP web server with cache disabled. | **3.82 seconds** initial load time (25.7 MB uncompressed payload: Wasm binary 8.0 MB, Data package 17.6 MB). | Google Chrome DevTools (Network Waterfall, Throttling: Disabled) |

---

## PART III. BOTTLENECK ANALYSIS AND PERFORMANCE DECISION (15 Points)

### Detailed Analysis
- **Primary Area Reviewed:** **CPU / Memory**
- **Observed Behavior:**
  During exploration in the `Lobby` and Hallways, immovable environment props (specifically the `BookPedestal`, display cases, and static decorative tables) were executing `YSort.LateUpdate()` on every single rendered frame. Despite remaining at fixed coordinate positions, each static object recalculated `sortingOrder = baseOrder - Mathf.RoundToInt((transform.position.y + sortingOffset) * 100f)` sixty times per second. Furthermore, modal UI interfaces (such as the newly added Book Modal and Bag Modal) were being created and destroyed dynamically via runtime script instantiations, generating recurring garbage collection allocation spikes (~54 KB) that resulted in micro-stutter frame drops on browser execution.
- **Supporting Evidence:**
  The Unity Profiler CPU Timeline revealed that `YSort.LateUpdate()` accounted for ~0.82 ms of main thread execution per frame across all static props in the scene. In the Chrome DevTools Performance timeline, frequent minor GC execution blocks (12–15 ms) coincided with modal opening, momentarily dipping framerates below the 60 FPS target.
- **Possible Cause:**
  1. `YSort.cs` lacked static evaluation logic, forcing every attached GameObject to execute dynamic per-frame coordinate math regardless of whether the object could actually move.
  2. UI elements were instantiated on-demand rather than pre-baked inside `InGameCanvas.prefab` with `SetActive(false)` toggles.
- **Effect on User Experience:**
  On dedicated desktop PCs, the impact is subtle, but on low-end school computer lab workstations or student Chromebooks running single-threaded WebGL, unnecessary per-frame CPU load and periodic GC pauses create noticeable hitching and control sluggishness.

---

### Bottleneck Identification Matrix

| Observation | Supporting Evidence | Possible Cause | Priority |
| :--- | :--- | :--- | :---: |
| **1. Redundant per-frame sorting math on static environmental props** | Profiler shows `YSort.LateUpdate()` executing 60 times/sec for immovable objects (`BookPedestal`, static furniture), taking 0.82 ms CPU time per frame. | Missing `isStatic` guard in `YSort.cs` to calculate order once on `Awake()` and disable per-frame updates on non-moving props. | **High** |
| **2. Garbage collection spikes and frame drops during UI modal instantiation** | Unity Profiler recorded 54.2 KB GC Alloc per modal open event; Chrome DevTools showed 14.8 ms GC pause causing frame dip to 51 FPS. | Dynamic runtime instantiation of modal GameObject trees instead of pre-baking modal prefabs inside the persistent Canvas hierarchy. | **High** |
| **3. Sprite sorting layering conflict on pedestal table top** | Pedestal sprite sorting occasionally rendered in front of resting book item or player sprite when positioned at specific boundary coordinates. | Inconsistent baseline sorting orders and lack of dedicated surface sorting offset component for tabletop props. | **Medium** |

---

### Performance Decision Selection
> **[X] A. Optimization Is Necessary**  
> *Your measurements show a meaningful performance concern that should be addressed. Proceed with a reasonable and controlled improvement.*
>
> *(Justification: While the desktop workstation achieved near-60 FPS, WebGL targets educational school lab environments where single-threaded CPU budgets are strictly limited. Removing redundant per-frame calculations on static props and eliminating GC allocation churn during UI interactions prevents frame stutter and provides a silky, responsive experience across all client devices.)*

---

## PART IV. PERFORMANCE IMPROVEMENT PLAN (15 Points)

### Option A. Implemented Performance Optimization

| Item | Description |
| :--- | :--- |
| **Component Optimized** | 2D Depth Sorting Architecture (`YSort.cs` & `TableItemSort.cs`) and UI Modal System (`BookModalUI.cs` / `InGameCanvas.prefab`). |
| **Original Problem** | 1. Immovable world props (such as `BookPedestal`) executed `LateUpdate()` every frame to recalculate sprite sorting orders, wasting CPU cycles on static entities.<br>2. UI modals generated dynamic heap allocations and GC spikes upon opening, causing micro-stutters. |
| **Change Implemented** | **1. Static Y-Sort Caching (`YSort.cs`):**<br>Added an explicit `[SerializeField] private bool isStatic = false;` flag. In `Awake()`, `UpdateSort()` calculates the precise sorting order once based on the object’s initial world position. In `LateUpdate()`, an early exit condition `if (!isStatic)` bypasses execution entirely for stationary props. Moving entities (Player, roaming NPCs) retain `isStatic = false`.<br><br>**2. Table Surface Sorting Component (`TableItemSort.cs`):**<br>Created a dedicated component for tabletop items (`BookVisual`) that locks sorting directly to the parent pedestal's sorting layer + offset (`+2`), guaranteeing visual consistency without per-frame matrix math.<br><br>**3. Pre-Baked Inactive UI Hierarchy (`InGameCanvas.prefab`):**<br>Integrated `BookModal` and `BagModal` directly into the persistent Canvas prefab with an initial inactive state (`SetActive(false)`). Toggling now uses zero-allocation canvas activation rather than runtime instantiation. |
| **Expected Improvement** | - Reduces `YSort` CPU execution time by **>85%** across static props.<br>- Eliminates **100%** of runtime GC allocations on modal open/close (0 KB GC Alloc).<br>- Restores a locked **60.0 FPS** (16.6 ms frame budget) without GC hitching. |
| **Possible Trade-off** | If a developer moves a prop marked `isStatic = true` via script during runtime, its sorting order will not automatically refresh unless `UpdateSort()` is called manually (fully acceptable, as museum pedestals and furniture are permanently immovable). |

---

## PART V. RE-PROFILING AND FUNCTIONAL REGRESSION TESTING (10 Points)

### A. Performance Comparison (Before vs. After Optimization)

Re-profiling was executed on Build Version `0.4.1` using the identical test scenario, device, and profiler parameters.

| Metric | Before (v0.4.0) | After (v0.4.1) | Difference | Interpretation |
| :--- | :---: | :---: | :---: | :--- |
| **Average Framerate** | 58.8 FPS | **60.0 FPS** | **+1.2 FPS (+2.0%)** | Achieved locked, perfectly stable 60.0 FPS on WebGL browser display. |
| **Main Thread Frame Time** | 17.01 ms | **15.18 ms** | **-1.83 ms (-10.8%)** | Increased per-frame headroom, keeping well within the 16.66 ms frame budget. |
| **`YSort.LateUpdate()` CPU Time** | 0.82 ms / frame | **0.08 ms / frame** | **-0.74 ms (-90.2%)** | Static objects bypassed `LateUpdate()`; CPU time spent on sorting dropped drastically. |
| **Modal Toggle GC Alloc Spike** | 54.2 KB | **0.0 KB (Zero Alloc)** | **-54.2 KB (-100%)** | Pre-baking UI hierarchy into Canvas eliminated all heap allocation churn. |
| **Periodic GC Hitch Duration** | 14.8 ms pause | **0.0 ms (No Pause)** | **-14.8 ms (-100%)** | Eliminated GC-induced frame drops during book and inventory inspection. |
| **Total Runtime Allocated Memory** | 433.8 MB | **432.1 MB** | **-1.7 MB (-0.4%)** | Browser memory heap remained clean, lean, and leak-free. |

---

### B. Functional Regression Testing

Verification that previously established gameplay systems remain 100% functional and error-free after the architectural changes:

| Feature / Test | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :---: |
| **1. Static Pedestal Depth Sorting (`BookPedestal`)** | Pedestal renders behind the player when the player stands in front, and in front when the player walks behind it. Book item sits firmly on top of the pedestal. | Layer sorting evaluated accurately on `Awake()`; player walks smoothly in front and behind; book remains layered on pedestal surface. | **Pass** |
| **2. Dynamic Player Y-Sorting (`Player`)** | Moving player avatar dynamically updates `sortingOrder` in real time as the character travels vertically across the room. | Player avatar sorting adjusted continuously in `LateUpdate()`; no clipping or popping behind floor tiles or walls. | **Pass** |
| **3. Field Guide Book Modal Toggle & Controls** | Book modal opens on HUD click; arrow buttons navigate pages; [ESC] key and close button hide modal and re-enable player movement. | Modal opens instantaneously without lag; arrow buttons paginate cleanly; player movement properly paused and restored upon close. | **Pass** |
| **4. Game Progress Persistence & Cloud Sync** | Unlocking the Field Guide saves `IsBookUnlocked = true` to `PlayerPrefs` and Firebase Realtime Database; state persists upon scene reload. | Progress saved and retrieved successfully; upon scene reload, pedestal remained empty and HUD book button remained unlocked. | **Pass** |

---

## PART VI. NEW BUILD, VERSION, PATCH, AND HOTFIX (15 Points)

### Conceptual Framework

#### A. When to Create Another Build
A new build must be produced whenever an approved change alters the code, assets, configuration, or performance of the software candidate intended for testing, verification, or distribution. In this milestone:
- Architectural performance improvements were implemented in `YSort.cs` and `TableItemSort.cs`.
- The Field Guide Book modal was pre-baked into the `InGameCanvas.prefab` to eliminate GC allocation spikes.
- Sprite depth sorting ordering bugs between the player, pedestal, and book visual were resolved.

#### B. Versioning Convention (Semantic Versioning: `MAJOR.MINOR.PATCH`)
- **MAJOR (`X.0.0`):** Breaking architectural changes, full project rewrites, or milestone production releases (e.g., Capstone Final Defense release `1.0.0`).
- **MINOR (`0.X.0`):** New functional gameplay features, additional museum wings, or new Alterworld trial rooms (e.g., `0.3.0` $\rightarrow$ `0.4.0` when introducing the Alterworld wings and quiz battle trials).
- **PATCH (`0.4.X`):** Non-breaking bug corrections, performance optimizations, memory leak fixes, and security patches (e.g., `0.4.0` $\rightarrow$ `0.4.1` for this performance optimization milestone).

#### C. Normal Bug Fix vs. Hotfix
- **Normal Bug Fix:** Carried out during scheduled development sprints or QA testing phases before an official release cycle (e.g., fixing `YSort` static logic during development).
- **Hotfix:** An urgent, out-of-band corrective patch applied directly to an already deployed/released production version to fix a critical failure (e.g., a crash preventing users from loading saves) without introducing new development features.

#### D. Standard Hotfix Procedure
1. Identify and isolate the exact defect reported on the deployed release version.
2. Branch directly from the tagged release commit in version control (`git checkout -b hotfix/pedestal-depth-fix v0.4.0`).
3. Implement only the minimal focused correction required to resolve the defect.
4. Conduct targeted unit and functional tests on the affected component.
5. Perform regression testing on adjacent game systems.
6. Increment the **PATCH** version number (e.g., `0.4.0` $\rightarrow$ `0.4.1`).
7. Compile and produce a clean packaged build.
8. Execute a release smoke test on the packaged output.
9. Deploy the verified patch to the hosting environment (GitHub Pages).
10. Merge the hotfix branch back into the main development branch (`main` / `develop`) to ensure future builds retain the fix.

---

### F. Build Decision Record

| Item | Response |
| :--- | :--- |
| **Was application code / configuration changed?** | **Yes** |
| **Is another build required?** | **Yes** |
| **Previous Version** | `0.4.0` |
| **New Version, if applicable** | `0.4.1` |
| **Change Type** | **Performance / Bug Fix** |
| **Source Reference** | Commit `b7e9a2f` (Git Tag: `v0.4.1-patch`) |
| **Reason for Version Change** | Incremented PATCH version from `0.4.0` to `0.4.1` to incorporate CPU performance optimizations in `YSort.cs`, pre-instantiate UI modal hierarchies in `InGameCanvas.prefab`, and resolve sprite depth sorting layering on static pedestals. |

---

## PART VII. BUILD VERIFICATION AND SMOKE TEST (5 Points)

The smoke test was executed directly on the **actual compiled WebGL build package** (`docs/Build/`) running in Google Chrome, verifying that the distributable artifact is robust and release-ready.

| Smoke Test Area | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :---: |
| **1. Application Launches** | The WebGL loader bar completes, WebAssembly binaries compile without console exceptions, and the splash screen transitions to the game canvas. | Application initialized cleanly within 3.8 seconds; WebGL Canvas rendered at native resolution with zero fatal errors. | **Pass** |
| **2. Main Menu / Entry Point** | Main Menu buttons (Start Game, Settings, Credits) and Character Selection screen accept user mouse clicks and navigate to the Lobby. | Character selection loaded; choosing avatar and clicking "CONFIRM & PLAY" smoothly transitioned into the `Lobby` scene. | **Pass** |
| **3. Critical Feature (Lobby Exploration & Book Unlock)** | Avatar walks across Lobby floor, approaches `BookPedestal`, triggers unlock notification, and toggles the Field Guide Book modal. | Collision detection functioned properly; unlock banner displayed; Book modal opened and paginated with locked 60 FPS. | **Pass** |
| **4. Save / Load & Integration** | Player account authenticates with Firebase; unlocked book flag and progress persist across room doors and page reload. | Cloud REST payload synchronized player progression state; reloading the browser retained unlocked book state. | **Pass** |
| **5. Application Closes Correctly** | Navigating back to the Main Menu or closing/refreshing the browser tab terminates the WebGL runtime cleanly without memory leaks or hanging processes. | Returning to Main Menu released exploration assets cleanly; tab closed without browser hang or memory retention. | **Pass** |

---

## PART VIII. PERFORMANCE AND RELEASE MONITORING (5 Points)

### Pre-Release Verification Checklist

| Check Item | Expected Condition | Verified State |
| :--- | :--- | :---: |
| **Important Functional Tests** | All core player controls, room doors, dialogs, and quiz battles pass. | **Verified (Pass)** |
| **Critical Known Defects** | Zero blocking bugs; all known sorting issues documented and resolved. | **Verified (Resolved)** |
| **Performance Baseline** | No unexplained performance regressions; framerate locked at 60 FPS on standard hardware. | **Verified (60 FPS)** |
| **Memory Growth** | No abnormal continuous heap accumulation (<500 MB tab memory ceiling). | **Verified (432 MB Stable)** |
| **Network Requests** | REST API calls occur only during deliberate save events; no redundant polling loops. | **Verified (Clean REST calls)** |
| **Security Functions** | Input validation and character name boundary clamping operate reliably. | **Verified (Active in v0.4.1)** |
| **Version Number** | Semantic versioning updated in `PlayerSettings` and UI labels (`v0.4.1`). | **Verified (`v0.4.1`)** |
| **Source Reference** | Verified Git commit hash and release tag recorded in build metadata. | **Verified (`b7e9a2f`)** |
| **Packaged Build** | Clean WebGL export generated in `docs/Build/` and smoke-tested on live browsers. | **Verified (5/5 Smoke Tests)** |
| **Known Issues** | Minor non-blocking visual polish items documented in project backlog. | **Verified (Documented)** |

---

### Primary Performance Metric Definition
- **Primary Performance Metric to Monitor:** **Main Thread Frame Time (Target: $\le 16.66\text{ ms}$ / locked 60 FPS)**.
- **Reason:** As a web-based educational RPG deployed over WebGL to school computers with diverse hardware specifications, maintaining a consistent 60 FPS without micro-stutters is paramount. Single-threaded browser performance directly impacts gameplay satisfaction and student engagement during time-sensitive quiz battles.
- **When It Should Be Checked:** **Before Release & During Every Testing Cycle** (specifically after introducing new animated sprite sets, additional Alterworld wings, or complex UI screens).

---

## PART IX. INDIVIDUAL CONTRIBUTION RECORD (5 Points)

### Member 1: Masculino, Jhon Darrel C.
- **Group Number:** Group IT9
- **Area Worked On:** CPU Optimization / Code Architecture & Sorting Systems
- **Component / File / Asset:** `Assets/Scripts/YSort.cs`, `Assets/Scripts/TableItemSort.cs`, `Assets/Scripts/BookPedestal.cs`
- **Specific Contribution:** Profiled CPU usage and identified redundant per-frame sorting calculations across stationary scene props. Implemented static sorting order caching in `YSort.cs` (`isStatic = true`) to disable `LateUpdate()` on immovable objects. Created `TableItemSort.cs` to guarantee consistent sorting offsets for items resting on pedestals, eliminating depth clipping between the player, pedestal, and book visual.
- **Profiling / Testing Evidence:** Unity Profiler CPU Timeline showing `YSort.LateUpdate()` drop from 0.82 ms down to 0.08 ms per frame (-90.2% CPU reduction) and stable 60 FPS rendering.
- **Commit / Changeset Reference:** Commit `b7e9a2f` (*"perf(core): cache static YSort and optimize pedestal depth sorting"*)
- **Build Responsibility:** WebGL build compilation, PlayerSettings version configuration (`v0.4.1`), and standalone offline launcher packaging.
- **Reflection (approx. 100 words):**  
  > *I learned that functional completeness does not guarantee production readiness. An application can pass functional tests while silently squandering CPU cycles through unnecessary per-frame processing. By profiling with the Unity Profiler before modifying code, we identified that static environmental props were continuously recalculating sorting orders that never changed. Caching these calculations reclaimed valuable main-thread headroom, proving that data-driven optimization must always precede code changes. Every succeeding build must be verified through regression testing to ensure optimizations do not compromise visual or functional stability.*

---

### Member 2: Anahaw, Samantha Isabel N.
- **Group Number:** Group IT9
- **Area Worked On:** Memory Management / UI Architecture & Asset Optimization
- **Component / File / Asset:** `Assets/Scripts/BookModalUI.cs`, `Assets/Prefabs/InGameCanvas.prefab`, `Assets/UI/Book/`
- **Specific Contribution:** Refactored the Field Guide Book modal architecture from dynamic runtime prefab instantiation to a pre-baked Canvas hierarchy with zero-allocation toggling (`SetActive(false)`). Designed the dual-page book visual layout with pixel-art page-turning pagination arrows, ensuring clean aspect ratio scaling across Full HD and mobile browser viewports.
- **Profiling / Testing Evidence:** Unity Memory Profiler and Chrome DevTools Heap Snapshot demonstrating a complete drop in modal GC Alloc from 54.2 KB down to 0.0 KB (Zero Alloc), eliminating intermittent 14.8 ms GC pauses.
- **Commit / Changeset Reference:** Commit `4c8d19e` (*"refactor(ui): pre-instantiate BookModalUI in Canvas to eliminate GC churn"*)
- **Build Responsibility:** UI layout scaling verification across varying browser window aspect ratios (16:9 Full HD, 16:10, and windowed viewports).
- **Reflection (approx. 100 words):**  
  > *I learned that memory management in WebGL is exceptionally sensitive because browsers enforce strict memory ceilings on WebAssembly tabs. Dynamically instantiating and destroying complex UI canvases repeatedly triggers garbage collection cycles that manifest as frustrating micro-stutters during gameplay. Pre-baking the Book Modal directly into the Canvas hierarchy eliminated runtime allocations entirely. Functional testing confirmed that page navigation worked flawlessly, while memory profiling proved our heap footprint remained lean. Balancing visual fidelity with disciplined memory architecture is essential for building robust educational software.*

---

### Member 3: Policarpio, Lance Angelo A.
- **Group Number:** Group IT9
- **Area Worked On:** Network Performance & Database State Verification
- **Component / File / Asset:** `Assets/Scripts/Firebase/FirebaseDatabaseManager.cs`, `Assets/Scripts/GameProgression.cs`, `PlayerData.cs`
- **Specific Contribution:** Evaluated network payload sizes, REST API synchronization timing, and cloud save triggers for player progression data. Verified that unlocking the Field Guide (`IsBookUnlocked`) synchronizes efficiently with Firebase Realtime Database without blocking the main thread, and confirmed that offline `PlayerPrefs` fallback functions seamlessly during network dropouts.
- **Profiling / Testing Evidence:** Google Chrome DevTools Network Waterfall showing REST payload transfer sizes under 2.1 KB and request durations below 120 ms during state persistence events.
- **Commit / Changeset Reference:** Commit `9a3f71c` (*"perf(net): streamline player data JSON serialization and sync intervals"*)
- **Build Responsibility:** Network latency simulation testing (throttling tests under simulated Slow/Fast 3G network conditions).
- **Reflection (approx. 100 words):**  
  > *I learned that release verification requires validating performance across both ideal and degraded network conditions. In an educational game reliant on cloud persistence, network operations must never hitch the rendering thread or corrupt save files when connectivity fluctuates. Establishing quantitative baselines for request latency and payload size allowed us to streamline our JSON synchronization logic. Ensuring reliable fallback to local storage guarantees that students can continue learning even during classroom network interruptions, reinforcing that software reliability depends on anticipating real-world operational constraints.*

---

### Member 4: Sy, Stephanie Sofia V.
- **Group Number:** Group IT9
- **Area Worked On:** Quality Assurance / Functional Regression & Build Smoke Testing
- **Component / File / Asset:** Packaged WebGL Build (`docs/Build/`), Release Test Matrix, Smoke Test Execution Plan
- **Specific Contribution:** Authored and executed the comprehensive 5-point release smoke test suite on the final compiled WebGL build. Conducted functional regression tests across avatar movement, collision boundaries, dialogue interactions, and progress saving to confirm that performance optimizations introduced zero unintended side effects. Verified cross-browser compatibility across Google Chrome and Microsoft Edge.
- **Profiling / Testing Evidence:** Documented 5/5 Smoke Test Pass verification matrix and functional regression pass logs on the production WebGL build artifact.
- **Commit / Changeset Reference:** Commit `e21b840` (*"test(qa): execute release smoke test suite on WebGL build v0.4.1"*)
- **Build Responsibility:** Pre-release verification checklist sign-off and distribution integrity verification for the GitHub Pages deployment.
- **Reflection (approx. 100 words):**  
  > *I learned that smoke testing and regression testing are the critical safety gates that protect users from broken releases. Optimizing one component can easily cause unforeseen regressions in another—such as fixing depth sorting inadvertently altering collision triggers. Verifying the application not only in the Unity Editor but on the actual packaged WebGL build proved that our build pipeline produces stable artifacts. Testing transforms performance theory into tangible user confidence, ensuring that every build released to players delivers a polished, reliable, and engaging educational experience.*

---
*End of Laboratory Report — Activity 10: Performance Optimization, Testing, and Release Verification*

