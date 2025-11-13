# Phase 1.1: Core Match Logic & State Management - Implementation Guide

## ✅ Completed Features

### 1.1.1 Player Stats (HP System) ✅
**Status:** Fully Implemented

**New Script:** `PlayerStats.cs`
- Max HP system with configurable values
- `TakeDamage(float)` method to subtract damage
- `Heal(float)` method to restore health
- Death detection when HP ≤ 0
- Health changed events for UI updates
- Reset health functionality

**Key Features:**
- Event-based architecture: `OnHealthChanged`, `OnPlayerDied`
- Property accessors: `MaxHealth`, `CurrentHealth`, `IsAlive`, `HealthPercent`
- Integrated with PlayerUIController for automatic UI updates

---

### 1.1.2 Game Loop (Match State Machine) ✅
**Status:** Fully Implemented

**New Script:** `MatchManager.cs`
- Singleton pattern for global access
- Complete state machine implementation

**Match States:**
1. `PreMatch` - Initial state before match starts
2. `MatchStarting` - Countdown phase (3 seconds default)
3. `CastingPhase` - Active gameplay where players cast spells
4. `MatchEnding` - Victory sequence
5. `MatchEnded` - Final state

**Match Flow:**
```
PreMatch → MatchStarting (3s delay) → CastingPhase → MatchEnding → MatchEnded
```

**Victory Conditions:**
- Player HP reaches 0 → Opponent wins
- Time limit reached (optional) → Winner by most HP
- Draw if both players have equal HP when time expires

**Features:**
- Configurable match start delay (default: 3 seconds)
- Optional time limit system (default: 300 seconds / 5 minutes)
- Match timer tracking
- Restart match functionality
- Pause/Resume functionality
- Winner determination logic

---

### 1.1.3 HUD Update ✅
**Status:** Enhanced and Integrated

**Updated Script:** `PlayerUIController.cs`
- Now integrates with `PlayerStats` component
- Automatic health bar updates via events
- Displays both local player and health values

**New Script:** `MatchHUD.cs`
- Centralized match UI management
- Match state display (Pre-Match, Fight!, Match Ended, etc.)
- Match timer display (elapsed or countdown)
- Victory panel with winner announcement
- Dual health bar display for both players
- Player name labels

**UI Elements Supported:**
- Health bars (sliders)
- Health text (numeric display)
- Mana bars (from existing system)
- Match state text
- Match timer
- Victory panel

---

### 1.1.4 Spell Effects Integration ✅
**Status:** Fully Integrated

**Updated Scripts:**
- `SpellProjectile.cs` (Fireball damage)
- `LightningEffect.cs` (Lightning damage)

**Changes Made:**
```csharp
// Before
private void ApplyDamage(GameObject target)
{
    Debug.Log($"Fireball hit {target.name} for {damage} damage!");
}

// After
private void ApplyDamage(GameObject target)
{
    PlayerStats targetStats = target.GetComponent<PlayerStats>();
    if (targetStats != null)
    {
        targetStats.TakeDamage(damage);
        Debug.Log($"💥 Fireball hit {target.name} for {damage} damage!");
    }
}
```

**Spell Damage Values:**
- Fireball: 10 damage (configurable in SpellData)
- Lightning: 25 damage (configurable in SpellData)
- Shield: Defensive spell (no damage)

**Integration:**
- Spells now properly reduce target player HP
- HP reduction triggers UI updates automatically
- HP reaching 0 triggers match end
- Match Manager detects player death and declares winner

---

### 1.1.5 Arena Setup ✅
**Status:** Verified Existing Setup

**Current Arena Structure:**
```
/ArcanumPlatform (existing arena platform)
/Player1 (Position: configured in scene)
/Player2 (Position: configured in scene)
/BackGround (visual backdrop)
/Main Camera (positioned for duel view)
/Directional Light (scene lighting)
```

**Boundaries:**
- Arena platform provides visual boundaries
- Player positions fixed for duel setup
- 2.5D view angle for spell visibility

**Notes:**
- Current arena is functional for core gameplay
- Players are correctly positioned within the space
- No additional boundaries needed for Phase 1.1

---

## 🔧 Setup Instructions

### Step 1: Add PlayerStats to Players

1. **Select Player1** in Hierarchy
2. **Add Component** → `PlayerStats`
3. In Inspector, configure:
   - Max Health: `100`
   - UI Controller: Drag `PlayerUIController` reference (if available)

4. **Select Player2** in Hierarchy
5. **Add Component** → `PlayerStats`
6. Configure same as Player1

### Step 2: Create Match Manager GameObject

1. **Create Empty GameObject** in scene: `GameObject → Create Empty`
2. **Rename** to `MatchManager`
3. **Add Component** → `MatchManager`
4. In Inspector, configure:
   - Match Start Delay: `3`
   - Match Time Limit: `300` (5 minutes)
   - Use Time Limit: `false` (uncheck for unlimited time)
   - Player 1 Stats: Drag `Player1` GameObject
   - Player 2 Stats: Drag `Player2` GameObject
   - Player 1 UI: Drag Player1's `PlayerUIController` reference
   - Player 2 UI: Drag Player2's `PlayerUIController` reference (if exists)

### Step 3: Update PlayerUIController References

1. **Select GameObject** with `PlayerUIController` component
2. In Inspector, find `PlayerUIController` component
3. Set **Player Stats** field:
   - Drag the corresponding Player GameObject (Player1 or Player2)

### Step 4: (Optional) Create Match HUD

1. **Create UI Canvas** if not exists: `GameObject → UI → Canvas`
2. **Create Empty GameObject** under Canvas: `MatchHUD`
3. **Add Component** → `MatchHUD`
4. Create UI elements:

**Match State Text:**
- Create `Text - TextMeshPro` → "MatchStateText"
- Position: Top center
- Drag to `Match State Text` field

**Match Timer Text:**
- Create `Text - TextMeshPro` → "MatchTimerText"
- Position: Top right
- Drag to `Match Timer Text` field

**Victory Panel:**
- Create `Panel` → "VictoryPanel"
- Add child `Text - TextMeshPro` → "VictoryText"
- Drag panel to `Victory Panel` field
- Drag text to `Victory Text` field

**Player Health Bars (Top of Screen):**
- Create 2 `Slider` components for player health
- Drag to `Player 1 Health Bar` and `Player 2 Health Bar`
- Add TextMeshPro for health values
- Add TextMeshPro for player names

5. Set **Match Manager** reference to the MatchManager GameObject

### Step 5: Verify Player Tags

**CRITICAL:** Ensure both players have the "Player" tag
1. Select `Player1` → Inspector → Tag: `Player` ✅
2. Select `Player2` → Inspector → Tag: `Player` ✅

Without this tag, spells won't detect hits!

### Step 6: Test the System

1. **Enter Play Mode**
2. **Observe Console:**
   - "Match starting in 3 seconds..."
   - "MATCH STARTED! Begin casting!"
3. **Cast Fireball** at opponent:
   - Draw gesture
   - Fireball spawns
   - On hit: "Player2 took 10 damage! HP: 90/100"
4. **Watch Health Bars** decrease
5. **Continue until one player reaches 0 HP**
6. **Victory Message:** "Player1 wins the match!"
7. **Match State:** "Match Ended"

---

## 📊 Event Flow Diagram

```
Match Start
    ↓
MatchManager.Start()
    ↓
InitializePlayers() → Subscribe to OnPlayerDied events
    ↓
StartMatchSequence() → Wait 3 seconds
    ↓
State: CastingPhase (Players can cast spells)
    ↓
Player casts Fireball → Hits opponent
    ↓
SpellProjectile.ApplyDamage()
    ↓
PlayerStats.TakeDamage() → currentHealth -= damage
    ↓
OnHealthChanged event → Updates UI
    ↓
If HP ≤ 0 → OnPlayerDied event
    ↓
MatchManager.OnPlayerDefeated() → Determine winner
    ↓
State: MatchEnding → Show victory
    ↓
State: MatchEnded
```

---

## 🎮 Gameplay Features

### Health System
- Starting HP: 100
- Visible in health bar (slider + text)
- Updates in real-time
- Death at HP ≤ 0

### Match Phases
- **Pre-Match:** Setup phase
- **Starting (3s):** Get ready countdown
- **Casting:** Active gameplay
- **Ending:** Victory sequence
- **Ended:** Match complete

### Victory Conditions
- **Knockout:** Opponent HP reaches 0
- **Time Limit:** Most HP when time expires (if enabled)
- **Draw:** Equal HP at time limit

### Damage Values
- Fireball: 10 HP
- Lightning: 25 HP (direct hit) + chains
- Shield: 0 HP (defensive)

---

## 🧪 Testing Checklist

- [ ] Player1 has PlayerStats component
- [ ] Player2 has PlayerStats component
- [ ] Both players tagged as "Player"
- [ ] MatchManager exists in scene
- [ ] MatchManager references both players
- [ ] Health bars display correctly
- [ ] Match starts after 3-second countdown
- [ ] Fireball reduces target HP
- [ ] Lightning reduces target HP
- [ ] HP bars update when damaged
- [ ] Match ends when player reaches 0 HP
- [ ] Winner is correctly announced
- [ ] Console shows proper damage logs

---

## 🔍 Debugging Tips

### Spells Don't Reduce HP
- Check if Player has `PlayerStats` component
- Check if Player is tagged as "Player"
- Check Console for damage logs
- Verify spell damage values in SpellData

### Health Bar Doesn't Update
- Check `PlayerUIController` has `Player Stats` reference
- Check health slider is assigned
- Check `OnHealthChanged` event is subscribed

### Match Doesn't End
- Check `MatchManager` has player references
- Check `OnPlayerDied` events are subscribed
- Check Console for "defeated" messages

### Multiple MatchManagers
- Only one MatchManager should exist (Singleton)
- Destroy extras if they appear

---

## 📝 Code Integration Summary

### New Components Required on GameObjects

**Player1:**
- `SpellCaster` (existing)
- `PlayerStats` ⭐ NEW
- `PlayerUIController` (existing, updated)

**Player2:**
- `PlayerStats` ⭐ NEW
- (Optional) `PlayerUIController` for local display

**MatchManager (New GameObject):**
- `MatchManager` ⭐ NEW

**UI Canvas:**
- `MatchHUD` ⭐ NEW (optional but recommended)

---

## 🎯 Next Phase Recommendations

With Phase 1.1 complete, you now have:
- ✅ Working HP system
- ✅ Match state machine
- ✅ Victory conditions
- ✅ HUD integration
- ✅ Spell damage integration

**Suggested Next Steps:**
1. **Phase 1.2:** Add more spell variety (different damage types, effects)
2. **Phase 1.3:** Implement shield blocking mechanics
3. **Phase 2:** Add multiplayer networking
4. **Phase 3:** Add more visual polish (damage numbers, health bar animations)

---

## 🐛 Known Limitations

- Shield spell doesn't block damage yet (Phase 1.3)
- No respawn system (match ends permanently)
- No round system (single match only)
- Health regeneration not implemented
- No damage resistance/armor system

---

## 📚 API Reference

### PlayerStats

```csharp
// Properties
float MaxHealth { get; }
float CurrentHealth { get; }
bool IsAlive { get; }
float HealthPercent { get; }

// Methods
void TakeDamage(float damage)
void Heal(float amount)
void ResetHealth()
void SetMaxHealth(float newMaxHealth)

// Events
event Action<float, float> OnHealthChanged  // (currentHP, maxHP)
event Action OnPlayerDied
```

### MatchManager

```csharp
// Properties (Static)
static MatchManager Instance { get; }

// Properties
MatchState CurrentState { get; }
float MatchTimeRemaining { get; }
float MatchTimeElapsed { get; }
PlayerStats Winner { get; }

// Methods
void RestartMatch()
void PauseMatch()
void ResumeMatch()
string GetMatchTimeString()
PlayerStats GetPlayer1()
PlayerStats GetPlayer2()
PlayerStats GetOpponent(PlayerStats player)

// Events
event Action<MatchState> OnMatchStateChanged
event Action<PlayerStats> OnPlayerWon
event Action OnMatchTimeLimitReached
```

### MatchState Enum

```csharp
enum MatchState
{
    PreMatch,
    MatchStarting,
    CastingPhase,
    MatchEnding,
    MatchEnded
}
```

---

**Implementation Complete! 🎉**

All Phase 1.1 features have been successfully implemented and integrated.
