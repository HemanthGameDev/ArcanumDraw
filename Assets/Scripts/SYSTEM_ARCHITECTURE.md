# System Architecture - Gesture Recognition

## Visual Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        PLAYER INPUT                         │
│                  (Touch/Mouse on RunePad)                   │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              GESTURE DRAWING MANAGER                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  • Collects touch points (List<GesturePoint>)         │  │
│  │  • Records timestamps                                 │  │
│  │  • Manages LineRenderer visuals                       │  │
│  │  • Double-tap to clear                                │  │
│  │  • Converts screen → local coordinates               │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │ On finger lift
                         │ gesturePoints + totalTime
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  GESTURE RECOGNIZER                         │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  STEP 1: Pre-process Input                           │  │
│  │  • Convert Vector3 → Vector2                         │  │
│  │  • Calculate speed (pathLength/time)                 │  │
│  │  • Calculate direction (angle sum)                   │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  STEP 2: Normalize Gesture                           │  │
│  │  • Resample to 64 points                             │  │
│  │  • Rotate to zero (if allowRotation)                 │  │
│  │  • Scale to 250x250 square                           │  │
│  │  • Translate to origin (0,0)                         │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  STEP 3: Compare to Templates                        │  │
│  │  FOR each spell in availableSpells:                  │  │
│  │    • Check speed constraints ─────┐                  │  │
│  │    • Check direction constraints ─┤ Early Exit      │  │
│  │    • Normalize template          │                  │  │
│  │    • Calculate path distance ─────┘                  │  │
│  │    • Track best match                                │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  STEP 4: Return Result                               │  │
│  │  • Success: bestScore ≤ tolerance                    │  │
│  │  • Failure: no match found                           │  │
│  │  • Include: spell, confidence, speed, direction      │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
              ┌──────────┴──────────┐
              │                     │
        SUCCESS ✅              FAILURE ❌
              │                     │
              ▼                     ▼
┌─────────────────────────┐  ┌──────────────────┐
│    SPELL CASTER         │  │  Log Message     │
│  ┌───────────────────┐  │  │  "No match"      │
│  │ Check Mana        │  │  └──────────────────┘
│  │ • Has enough? ────┼──┐
│  └───────────────────┘  │ │
│                         │ │
│  ┌───────────────────┐  │ │ ❌ FAIL
│  │ Check Cooldown    │  │ │
│  │ • Not active? ────┼──┤
│  └───────────────────┘  │ │
│                         │ │
│  ┌───────────────────┐  │ │
│  │ Cast Spell ✅     │◄─┘
│  │ • Deduct mana     │
│  │ • Start cooldown  │
│  │ • Spawn effect    │
│  │ • Apply force     │
│  │ • Clear visuals   │
│  └───────────────────┘  │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────┐
│                    SPELL EFFECT                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  • Instantiate prefab at spellSpawnPoint              │  │
│  │  • Calculate direction to targetOpponent              │  │
│  │  • Apply Rigidbody force                              │  │
│  │  • Projectile flies!                                  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Flow

### Input Phase
```
Player Touch
   ↓
InputSystem (New Input System)
   ↓
GestureDrawingManager.OnTouchBegan()
   ↓
IsPositionInsideRunePad() → YES
   ↓
InitiateDrawing()
   ↓
currentGesturePoints.Add(point)
gestureStartTime = Time.time
   ↓
[User drags finger]
   ↓
Update() → UpdateDrawing()
   ↓
AddPointToCurrentGesture()
lineRenderer.AddPointToCurrentLine()
   ↓
[User lifts finger]
   ↓
OnTouchEnded()
   ↓
CompleteDrawing()
```

### Recognition Phase
```
ProcessCompletedGesture()
   ↓
totalDrawingTime = Time.time - gestureStartTime
   ↓
gestureRecognizer.RecognizeGesture(points, time)
   ↓
┌─────────────────────────────────┐
│ GestureRecognizer               │
│                                 │
│ Convert to 2D                   │
│    ↓                            │
│ Calculate metrics               │
│ • pathLength                    │
│ • drawSpeed                     │
│ • drawDirection                 │
│    ↓                            │
│ Preprocess gesture              │
│ • Resample(64)                  │
│ • RotateBy(angle) [conditional] │
│ • ScaleToSquare(250)            │
│ • TranslateToOrigin()           │
│    ↓                            │
│ FOR each spell:                 │
│   ├─ enforceSpeed? Check ───┐   │
│   ├─ enforceDirection? Check┘   │
│   ├─ Preprocess template        │
│   ├─ CalculatePathDistance()    │
│   └─ Track if best              │
│    ↓                            │
│ Return best match               │
└─────────────────────────────────┘
   ↓
GestureRecognitionResult
   ↓
IF success:
   spellCaster.AttemptCastSpell(spell)
ELSE:
   Log "No match"
```

### Casting Phase
```
SpellCaster.AttemptCastSpell(spell)
   ↓
currentMana >= spell.manaCost?
   ├─ NO → Log "Not enough mana" → ABORT
   └─ YES
       ↓
IsSpellOnCooldown(spell)?
   ├─ YES → Log "On cooldown" → ABORT
   └─ NO
       ↓
currentMana -= spell.manaCost
spellCooldowns[spell.spellID] = Time.time + cooldown
   ↓
SpawnSpellEffect(spell)
   ↓
┌────────────────────────────────┐
│ spellEffect = Instantiate(     │
│   spell.spellEffectPrefab,     │
│   spellSpawnPoint.position,    │
│   spellSpawnPoint.rotation     │
│ )                              │
│    ↓                           │
│ rb = spellEffect.Rigidbody     │
│ direction = (target - spawn)   │
│ rb.AddForce(                   │
│   direction * projectileForce  │
│ )                              │
└────────────────────────────────┘
   ↓
gestureDrawingManager.ClearAllDrawings()
   ↓
Spell flies towards opponent!
```

---

## Component Relationships

```
Scene Hierarchy:
┌─────────────────────────────────────┐
│ GestureManager                      │
│  ├─ GestureDrawingManager ────┐     │
│  ├─ GestureLineRenderer        │     │
│  └─ GestureRecognizer ◄────────┼─┐   │
└────────────────────────────────┼─┼───┘
                                │ │
┌────────────────────────────────┼─┼───┐
│ Player                         │ │   │
│  ├─ SpellCaster ◄──────────────┘ │   │
│  └─ SpellSpawnPoint              │   │
└──────────────────────────────────┼───┘
                                  │
┌──────────────────────────────────┼───┐
│ Opponent ◄───────────────────────┘   │
└──────────────────────────────────────┘

Project Assets:
┌──────────────────────────────────────┐
│ SpellData (ScriptableObject)         │
│  ├─ Fireball.asset ──┐               │
│  ├─ Lightning.asset ─┼──────┐        │
│  └─ Healing.asset ───┘      │        │
└─────────────────────────────┼────────┘
                              │
                        Referenced by:
                        ┌─────┴─────┐
                GestureRecognizer  SpellCaster
                (availableSpells)  (attempted)
```

---

## Algorithm: Template Matching

### Pre-processing Steps

```
Input: Raw gesture points (variable count, arbitrary position/scale/rotation)
Output: Normalized 64-point gesture (centered, scaled, optionally rotated)

FUNCTION PreprocessGesture(points, normalizeRotation):
    
    1. Resample(points, 64)
       ┌────────────────────────────────────┐
       │ Distribute points evenly along     │
       │ the path by arc length             │
       │                                    │
       │ Before: [10 points, uneven]        │
       │ After:  [64 points, evenly spaced] │
       └────────────────────────────────────┘
    
    2. IF normalizeRotation:
          RotateBy(indicativeAngle)
          ┌────────────────────────────────────┐
          │ Rotate so first point aligns with  │
          │ horizontal from centroid           │
          │                                    │
          │ Before: Tilted V                   │
          │ After:  Upright V                  │
          └────────────────────────────────────┘
    
    3. ScaleToSquare(250)
       ┌────────────────────────────────────┐
       │ Fit gesture into 250x250 box       │
       │ (maintains aspect ratio)           │
       │                                    │
       │ Before: Large/small circle         │
       │ After:  Standardized circle        │
       └────────────────────────────────────┘
    
    4. TranslateToOrigin()
       ┌────────────────────────────────────┐
       │ Move centroid to (0,0)             │
       │                                    │
       │ Before: Circle at (100, 200)       │
       │ After:  Circle at (0, 0)           │
       └────────────────────────────────────┘
    
    RETURN normalized_points
```

### Distance Calculation

```
FUNCTION CalculatePathDistance(gesture, template):
    
    totalDistance = 0
    
    FOR i = 0 to 64:
        distance = EuclideanDistance(gesture[i], template[i])
        totalDistance += distance
    
    averageDistance = totalDistance / 64
    
    RETURN averageDistance

Where:
    Low distance  = Good match  (e.g., 10.5)
    High distance = Poor match  (e.g., 95.3)
```

### Matching Example

```
Drawn Circle:
    Raw points: 127 points
    After resample: 64 points
    After scale: Fits in 250x250
    After translate: Centered at (0,0)

Template Circle (Fireball):
    Generated: 32 points
    After resample: 64 points
    Already scaled: 250x250
    Already centered: (0,0)

Path Distance:
    Sum of 64 point-to-point distances
    Example: 12.3 units average

Tolerance Check:
    12.3 ≤ 0.25? NO → Normalize!
    
Normalized Score:
    score = 12.3 / (diagonal of 250x250)
    score = 12.3 / 353.55
    score = 0.035
    
    0.035 ≤ 0.25? YES → Match! ✅
    
Confidence:
    1 - (score / tolerance)
    = 1 - (0.035 / 0.25)
    = 1 - 0.14
    = 0.86 = 86% ✅
```

---

## State Machines

### Drawing State
```
┌─────────┐
│  IDLE   │
└────┬────┘
     │ Touch Inside RunePad
     ▼
┌──────────────┐
│  COLLECTING  │ ◄──┐
└────┬─────────┘    │
     │ Drag        │
     └─────────────┘
     │ Lift Finger
     ▼
┌───────────────┐
│  RECOGNIZING  │
└────┬──────────┘
     │ Result Ready
     ├─ Success → Cast
     └─ Failure → Log
     │
     ▼
┌─────────┐
│  IDLE   │
└─────────┘
```

### Spell Cooldown State
```
Spell Cast
    ↓
┌────────────┐
│  ON_COOLDOWN  │
│  (timer = now + cooldownTime)
└────┬─────────┘
     │ Time passes
     │ IF Time.time < timer:
     │    Still cooling down
     │ ELSE:
     │    Ready to cast
     ▼
┌──────────┐
│  READY   │
└──────────┘
```

### Mana State
```
     ┌────────────────┐
     │  currentMana   │
     └────┬───────────┘
          │
    ┌─────┴─────┐
    │           │
Cast Spell   Regen (Update)
    │           │
    ▼           ▼
Decrease    Increase
(instant)   (over time)
    │           │
    └─────┬─────┘
          ▼
   ┌──────────────┐
   │ Clamped      │
   │ 0 ≤ mana ≤ max│
   └──────────────┘
```

---

## Memory Layout

### GestureRecognizer
```
availableSpells: List<SpellData>
    ├─ [0] Fireball
    ├─ [1] Lightning
    └─ [2] Healing

During Recognition:
    processedGesture: Vector2[64]
    processedTemplate: Vector2[64]
    
    FOR each spell:
        Calculate distance → float
        Track best → (SpellData, float)
```

### SpellCaster
```
currentMana: float = 100
maxMana: float = 100

spellCooldowns: Dictionary<string, float>
    ├─ "FIREBALL_SPELL" → 125.3 (Time.time when ready)
    ├─ "LIGHTNING_BOLT" → 122.8
    └─ "HEALING_CIRCLE" → 0 (ready)

Check cooldown:
    IF Time.time < cooldowns[spellID]:
        Still on cooldown
    ELSE:
        Ready to cast
```

---

## Performance Metrics

### Time Complexity

**Per Gesture Recognition:**
```
1. Resample: O(n) where n = original point count
2. Rotate: O(m) where m = resampled count (64)
3. Scale: O(m)
4. Translate: O(m)
5. For each spell (s spells):
   - Preprocess template: O(m)
   - Calculate distance: O(m)
   - Total: O(s × m)

Overall: O(n + s×m)

Typical: O(150 + 10×64) = O(150 + 640) = O(790)
```

### Space Complexity

```
Per gesture:
    Raw points: ~100-200 Vector3 (1.2-2.4 KB)
    Resampled: 64 Vector2 (0.5 KB)
    Template: 64 Vector2 (0.5 KB)
    
Total per recognition: ~2-4 KB (negligible)
```

### Timing

```
Typical frame (60 FPS = 16.67ms):
    Input handling: < 0.1ms
    Drawing update: < 0.5ms
    Recognition: < 5ms
    Casting: < 1ms
    
Total overhead: < 7ms (< 42% of frame)
```

---

## Integration Points

### With Existing Systems

```
RunePadController
    ↓ Provides coordinate conversion
GestureDrawingManager
    ↓ Provides visual feedback
GestureLineRenderer
    ↓ Receives gesture points
GestureRecognizer ← NEW
    ↓ Recognizes patterns
SpellCaster ← NEW
    ↓ Executes spells
Spell Effects (Prefabs)
```

### With Future Systems

```
GestureRecognizer
    → UI Manager (show recognized spell)
    → Tutorial System (gesture hints)
    → Achievement System (perfect casts)
    → Combo System (chain detection)

SpellCaster
    → Player Stats (track casts)
    → Progression System (unlock spells)
    → Inventory System (mana potions)
    → Effects Manager (visual/audio)
```

---

## Configuration Examples

### Easy Setup (Beginner Friendly)
```
recognitionTolerance: 0.5
allowRotation: true
enforceSpeed: false
enforceDirection: false

Result: Forgiving recognition
```

### Balanced Setup (Normal Difficulty)
```
recognitionTolerance: 0.25
allowRotation: false
enforceSpeed: true
expectedSpeedRange: (10, 30)
enforceDirection: false

Result: Moderate precision required
```

### Expert Setup (High Skill)
```
recognitionTolerance: 0.15
allowRotation: false
enforceSpeed: true
expectedSpeedRange: (15, 25)
enforceDirection: true
expectedDirection: Clockwise

Result: Precise execution required
```

---

## Debug Workflow

```
Problem: Gesture not recognized

Step 1: Check Console
    └─ "Best match confidence: XX%"
    
Step 2: IF confidence close (60-70%):
    └─ Increase tolerance
    
Step 3: ELSE IF confidence low (< 50%):
    └─ Check constraints:
        ├─ Speed enforced? Disable or widen range
        ├─ Direction enforced? Disable or change
        └─ Template correct? Regenerate
    
Step 4: Test again
    └─ Iterate until working
```

---

**This architecture supports:**
- ✅ Scalable spell library
- ✅ Fast recognition (< 10ms)
- ✅ Flexible constraints
- ✅ Easy debugging
- ✅ Future enhancements

**Ready for production!** 🚀
