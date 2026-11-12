# Activity 9: Software Security Fundamentals — Laboratory Report
**Course:** System Fundamentals Lab (UPHSL College of Computer Studies)  
**Section:** J4T | **Group:** IT9  
**Members:** Masculino, Jhon Darrel C. | Anahaw, Samantha Isabel N. | Policarpio, Lance Angelo A. | Sy, Stephanie Sofia V.  
**Project:** PAMANA (Philippine Heritage Educational 2D Top-Down WebGL RPG)  
**Instructor:** Ma'am Grachel Eliza C. Greta  

---

## PART IV. SECURITY REVIEW OF CURRENT IMPLEMENTATION (15 Points)

### 1. Review of Key Security Areas

#### Area 1: Access Control
- **Intended Roles:** Students/players, guest users, and project administrators.
- **Enforcement Mechanism:**
  - **Server-Side:** Firebase Realtime Database Security Rules strictly restrict read and write access to authenticated paths (`/users/{$userId}` where `auth.uid == $userId`). Users cannot read or overwrite records belonging to other students.
  - **Client-Side:** In-game progression (`GameProgression.cs` and `RoomDoor.cs`) enforces that players cannot enter subsequent Alterworld battle trials or unlock rooms until they have fulfilled prerequisites (reading all 5 artifact plaques and returning recovered artifacts to their pedestals). Access control is enforced via scene transition scripts, not merely by hiding UI buttons.

#### Area 2: Authentication
- **Authentication Mechanism:** Integrated via Firebase Authentication supporting Google Sign-In (OAuth 2.0 Single Sign-On) with an automated Guest fallback session.
- **Credential Handling:** The client application never handles, transmits, or stores plain passwords. In WebGL, authentication is delegated to Google's OAuth flow through the official Firebase Web SDK (`firebase-auth-compat.js`), receiving a short-lived JSON Web Token (JWT).
- **Session Termination:** `FirebaseAuthManager.UnbindGoogle()` and `FirebaseDatabaseManager.ClearAllLocalGameData()` terminate the Firebase session, clear in-memory tokens, purge cached `PlayerPrefs` keys, and reset progression to prevent account bleed on shared computers.

#### Area 3: Secrets and Configuration
- **Status:** No private administrative service account credentials, server secret keys, or database master passwords are committed to Git or embedded in the WebGL build.
- **Configuration:** `FirebaseConfig.cs` contains only public client identifiers (`apiKey`, `authDomain`, `databaseUrl`, `projectId`, `appId`).
- **Protection:** In Google Cloud Console, the Web API key is restricted by HTTP referrer to the authorized hosting domains (e.g., GitHub Pages and localhost testing). Server-level database access is governed by Firebase Security Rules.

#### Area 4: Input Validation
- **Inspected Fields:** Custom character name input (`CharacterSelectManager.cs`), inventory item counts, economy values (coins), and quiz answer submissions.
- **Vulnerabilities Identified:** Character names previously allowed unbounded input lengths and unescaped characters, which could lead to JSON payload malformation or UI overflow.
- **Action Taken:** Enforced strict length truncation (1–15 characters), stripped non-alphanumeric/script characters (`<script>`, quotes, brackets), and clamped game score/item counters.

#### Area 5: Dependency and Package Review

| Dependency | Purpose | Source | Current Status / Concern |
| :--- | :--- | :--- | :--- |
| **Unity Universal Render Pipeline (URP)** `v17.0.4` | 2D lighting, sorting, and pixel-perfect rendering pipeline for WebGL | Unity Package Manager (`com.unity.render-pipelines.universal`) | Up to date, verified stable for Unity 6 2D WebGL builds. |
| **Unity Input System** `v1.14.1` | Cross-platform keyboard, mouse, and gamepad input event processing | Unity Package Manager (`com.unity.inputsystem`) | Fully compatible, native event binding, no known security advisories. |
| **Unity uGUI & TextMeshPro** `v2.0.0` | In-game HUD, modal interfaces, and dynamic typography | Unity Package Manager (`com.unity.ugui`) | Secure core package; rich text tags sanitized where student input is displayed. |
| **Firebase Web SDK** `v10.12.0` | Client-side OAuth authentication and Realtime Database WebGL bridge | Google CDN (`https://www.gstatic.com/firebasejs/10.12.0/`) | Latest stable compat release; loaded over enforced HTTPS. |
| **UnityWebRequest Module** `v1.0.0` | HTTPS REST communication with Firebase Realtime Database endpoints | Unity Engine Built-in Module (`com.unity.modules.unitywebrequest`) | Enforces TLS/HTTPS encryption on all payload transmissions. |

#### Area 6: Error Handling
- **Network Failures:** Handled gracefully in `FirebaseDatabaseManager.cs` via `LocalPlayerDataBackup_{userId}` in `PlayerPrefs`. When offline, saves persist locally and synchronize once connectivity is restored.
- **Authentication Failures:** Handled in `FirebaseAuthManager.cs` by invoking user-facing banner alerts (`NotificationBannerUI.Show()`) rather than crashing or throwing unhandled null references.
- **Corrupted Saves:** Missing or unparseable JSON records fall back to `CreateFreshPlayerData()`, preventing application halts.

---

### 2. Security Check Summary Table

| Security Check | Finding | Risk / Concern | Recommended Action |
| :--- | :--- | :--- | :--- |
| **1. User Input Validation** | Character naming input field accepted unrestricted string length and special/HTML characters. | Buffer/UI distortion, JSON payload injection, or malformed database keys. | Implement input sanitization (`SanitizeAndValidateName`) restricting characters to alphanumeric/hyphens with a 15-character limit. |
| **2. Database Boundary Clamping** | Outgoing player data (coins, artifact counts, room indices) was not clamped before cloud transmission. | A compromised or tampered local save file could inject negative currency or impossible artifact numbers (e.g., 99/5). | Add server-side/client-side bounds validation (`ValidatePlayerData`) clamping values to legitimate ranges. |
| **3. Account Isolation & Session Bleed** | Switching accounts on shared school computers could retain residual PlayerPrefs keys from prior sessions. | Unauthorized student progress contamination or overwriting another student's gameplay data. | Execute `ClearAllLocalGameData()` on sign-out/account switch to purge in-memory caches and stale PlayerPrefs. |
| **4. Transport Layer Encryption** | Game communicates with Firebase over network REST endpoints. | Eavesdropping or credential interception on open public/school Wi-Fi networks. | Enforce HTTPS/TLS on all endpoints; reject unencrypted HTTP requests in Unity WebGL settings. |
| **5. Client-Side Access Enforcement** | Room access relies on game state flags (`GameProgression.cs`). | Player attempting to bypass locked Alterworld doors or retry completed battles prematurely. | Enforce door state verification in `RoomDoor.Interact()` before loading room scenes, backed by database validation rules. |

---

## PART V. IMPLEMENT SECURITY IMPROVEMENTS (20 Points)

### Completed Improvements Overview

| Security Improvement | Risk Addressed | Component Changed | Evidence |
| :--- | :--- | :--- | :--- |
| **1. Strict Input Sanitization & Character Filtering** | Code/JSON injection, XSS-like payload strings, and UI overflow from excessively long player names. | `Assets/Scripts/CharacterSelectManager.cs` | `SanitizeAndValidateName()` method implementation and unit test pass. |
| **2. Defensive Progress & Numeric Boundary Validation** | Arbitrary value tampering (negative coins, out-of-bounds artifact retrieval counts, illegal room indices). | `Assets/Scripts/Firebase/FirebaseDatabaseManager.cs` | `ValidatePlayerData()` method executed prior to JSON serialization and PUT requests. |

---

### Detailed Improvement 1: Strict Input Sanitization & Character Filtering

1. **Before (What was the problem?):**  
   In `CharacterSelectManager.OnConfirmAndPlay()`, the user's raw text input from `nameInputField.text` was accepted without length constraints or character sanitization. If a student entered special characters (e.g., `<script>`, quotes, null terminators) or pasted a 200-character string, this unvalidated value was written directly to `PlayerPrefs` and dispatched to Firebase inside `PlayerData.characterName` and `PlayerData.username`. This created risks of JSON malformation, UI container clipping, and potential injection issues.

2. **Change (What did you implement?):**  
   Implemented `SanitizeAndValidateName(string input, string defaultFallback)` in `CharacterSelectManager.cs`:
   - Enforces a strict 15-character ceiling (`Length <= 15`).
   - Uses a strict character whitelist: only letters (`a-z`, `A-Z`), digits (`0-9`), spaces, hyphens (`-`), and underscores (`_`) are preserved.
   - Automatically strips all HTML tags, quotation marks, and control symbols.
   - If the sanitized result is empty or whitespace, it safely defaults to the assigned character preset (`"Darrel"` or `"Darla"`).

3. **After (What should happen now?):**  
   Any input containing malicious symbols or exceeding 15 characters is automatically truncated and stripped into a clean alphanumeric string prior to storage or network transmission. The game UI maintains visual layout integrity, and downstream Firebase records receive clean data.

4. **Evidence (Code / Test):**
```csharp
// Located in Assets/Scripts/CharacterSelectManager.cs
public static string SanitizeAndValidateName(string input, string defaultFallback)
{
    if (string.IsNullOrWhiteSpace(input)) return defaultFallback;

    string trimmed = input.Trim();

    // 1. Length constraint: 1 to 15 characters
    if (trimmed.Length > 15)
    {
        trimmed = trimmed.Substring(0, 15).Trim();
    }

    // 2. Character whitelist: letters, digits, spaces, hyphens, and underscores only
    var sb = new System.Text.StringBuilder();
    foreach (char c in trimmed)
    {
        if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
        {
            sb.Append(c);
        }
    }

    string sanitized = sb.ToString().Trim();
    return string.IsNullOrEmpty(sanitized) ? defaultFallback : sanitized;
}
```

---

### Detailed Improvement 2: Defensive Progress & Numeric Boundary Validation

1. **Before (What was the problem?):**  
   `FirebaseDatabaseManager.SavePlayerData()` serialized whatever values were passed in `PlayerData` directly into JSON and pushed them to Firebase. If local save storage was modified in the browser's IndexedDB or memory (e.g., setting negative coins, or claiming `retrievedCount = 99` in Room 1 where only 5 artifacts exist), the invalid state was accepted, corrupting evaluation metrics and game progression balance.

2. **Change (What did you implement?):**  
   Created and integrated `ValidatePlayerData(PlayerData data)` inside `FirebaseDatabaseManager.cs`, called immediately before serializing payloads in `SavePlayerData()`:
   - Clamps currency: `coins = Mathf.Max(0, data.coins);`
   - Clamps room indices: `roomNumber = Mathf.Clamp(rp.roomNumber, 1, 12);`
   - Clamps artifact counts: `retrievedCount` and `restoredPedestalsCount` strictly bounded between `0` and `5`.
   - Validates character name string length to at most 20 characters.

3. **After (What should happen now?):**  
   Even if corrupted state or tampered client values exist in memory, the data is normalized and clamped to valid gameplay boundaries before being sent to Firebase. Out-of-bounds metrics are prevented from entering the research database.

4. **Evidence (Code / Test):**
```csharp
// Located in Assets/Scripts/Firebase/FirebaseDatabaseManager.cs
public static void ValidatePlayerData(PlayerData data)
{
    if (data == null) return;

    // 1. Coins and economy boundaries
    data.coins = Mathf.Max(0, data.coins);

    // 2. Character name sanity
    if (!string.IsNullOrEmpty(data.characterName) && data.characterName.Length > 20)
    {
        data.characterName = data.characterName.Substring(0, 20).Trim();
    }

    // 3. Room progress data clamping and structure validation
    if (data.roomProgress != null)
    {
        foreach (var rp in data.roomProgress)
        {
            if (rp == null) continue;
            rp.roomNumber = Mathf.Clamp(rp.roomNumber, 1, 12);
            rp.retrievedCount = Mathf.Clamp(rp.retrievedCount, 0, 5);
            rp.restoredPedestalsCount = Mathf.Clamp(rp.restoredPedestalsCount, 0, 5);
        }
    }
}
```

---

## PART VI. SECURITY TESTING AND NEGATIVE TEST CASES (10 Points)

| Test # | Test Name / Area | Attempt / Input | Expected Secure Behavior | Actual Result | Status |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **1** | **Input Validation (XSS / Injection)** | Enter `<script>alert('xss')</script>` in the character naming input field. | Injection characters (`<`, `>`, `'`, `(`, `)`) are rejected or stripped; character name remains safe alphanumeric text. | Sanitized to `"scriptalert"`; dangerous characters stripped; length bounded. | **PASS** |
| **2** | **Boundary / Tampering Test** | Inject tampered `PlayerData` containing `coins = -500`, `retrievedCount = 99`, and `roomNumber = 999`. | Values are clamped to valid ranges: coins clamped to `0`, retrieved count clamped to `5`, room clamped to `12`. | Values normalized: `coins=0`, `retrievedCount=5`, `roomNumber=12`. Malformed values rejected. | **PASS** |
| **3** | **Authorization (Premature Door Access)** | Attempt to enter Alterworld Room 2 door without completing Room 1 and returning artifact. | Transition is denied; door displays educational lock message requiring Room 1 completion. | Door remains locked; UI banner explains missing prerequisite; scene does not load. | **PASS** |
| **4** | **Session Isolation (Account Switching)** | Sign in with Student Account A, progress in game, then execute `UnbindGoogle()` and sign in as Account B. | Account B starts with clean slate; zero progress inherited from Account A; PlayerPrefs wiped. | `ClearAllLocalGameData()` executed; `PlayerPrefs` wiped; Account B loads its own cloud record. | **PASS** |
| **5** | **Network Failure / Offline Resilience** | Disconnect network connectivity while an auto-save / progress save is initiated. | Game does not crash or freeze; save is queued to local `PlayerPrefs` backup; banner informs player. | `PutCoroutine` catches network timeout; stores `LocalPlayerDataBackup_{userId}`; gameplay continues. | **PASS** |

---

## PART VII. SECURITY CHECKLIST FOR FUTURE RELEASES (5 Points)

### PAMANA Pre-Release Security Checklist

| # | Security Release Check | Verification Criteria & Method | Done? |
| :---: | :--- | :--- | :---: |
| **1** | **No Sensitive Credentials Committed** | Verify repository contains no Google service account JSON files, private RSA keys, or database administrative secrets. Confirm only domain-restricted public Web API key is used. | **[X] YES** |
| **2** | **Authentication & Session Termination** | Test Google Sign-In with both valid and cancelled/failed attempts. Confirm `UnbindGoogle()` and `ClearAllLocalGameData()` flush in-memory tokens and wipe local `PlayerPrefs`. | **[X] YES** |
| **3** | **Authorization & Data Isolation** | Verify Firebase Security Rules require `auth.uid == $userId`. Confirm a logged-in user cannot read or overwrite `/users/{otherUid}` records via manual REST requests. | **[X] YES** |
| **4** | **User Input Validation & Sanitization** | Test character name input with long strings (>15 chars), symbols, and injection strings. Confirm `SanitizeAndValidateName()` normalizes input before writing to storage. | **[X] YES** |
| **5** | **Firebase Rules & API Key Domain Restrictions** | Ensure Realtime Database is in production mode (rules not set to `.read: true, .write: true`). Confirm API key in Google Cloud Console is restricted to authorized hosting domains. | **[X] YES** |
| **6** | **Dependency & Package Audit** | Review `Packages/manifest.json` and external CDN scripts in `docs/index.html`. Confirm no deprecated packages with known CVEs are imported without review. | **[X] YES** |
| **7** | **Debug Logs & Development Flags Disabled** | Ensure `Debug.Log` statements containing sensitive state are stripped or disabled in release WebGL builds, and debug cheat hotkeys are deactivated. | **[X] YES** |
| **8** | **User-Friendly Error Handling** | Verify network errors, failed logins, or dropped database connections display student-friendly `NotificationBannerUI` alerts rather than raw stack traces. | **[X] YES** |
| **9** | **Known Security Issues Documented** | Ensure WebGL client-side limitations (e.g., client memory inspection in browser dev tools) are acknowledged and mitigated with backend validation. | **[X] YES** |
| **10** | **Offline & Boundary Test Execution** | Test running the WebGL build completely offline to verify local fallback (`PlayerPrefs`) works seamlessly without game-breaking crashes. | **[X] YES** |

---

### Continuing Development Workflow

$$\text{Develop} \longrightarrow \text{Review} \longrightarrow \text{Merge} \longrightarrow \text{Security Check} \longrightarrow \text{Build} \longrightarrow \text{Test} \longrightarrow \text{Release}$$

