# Quick Test Checklist - Gesture Recognition System

Use this checklist to quickly set up and test your system.

---

## ✅ Pre-Flight Checklist (5 min)

### Scene Setup

- [ ] **GestureManager** GameObject exists
  - [ ] Has `GestureDrawingManager` component
  - [ ] Has `GestureLineRenderer` component
  - [ ] Has `GestureRecognizer` component ← **ADD THIS**

- [ ] **Player** GameObject created
  - [ ] Has `SpellCaster` component ← **ADD THIS**
  - [ ] Has child `SpellSpawnPoint` (empty Transform) ← **ADD THIS**

- [ ] **Opponent** GameObject created ← **ADD THIS**

### Component References

**GestureDrawingManager (on GestureManager):**
- [ ] Rune Pad Controller → (your RunePad)
- [ ] Line Renderer → (same object)
- [ ] **Gesture Recognizer → GestureManager** ← ASSIGN
- [ ] **Spell Caster → Player** ← ASSIGN

**SpellCaster (on Player):**
- [ ] Current Mana: `100`
- [ ] Max Mana: `100`
- [ ] Mana Regen Rate: `5`
- [ ] **Spell Spawn Point → Player/SpellSpawnPoint** ← ASSIGN
- [ ] **Target Opponent → Opponent** ← ASSIGN
- [ ] Projectile Force: `10`
- [ ] **Gesture Drawing Manager → GestureManager** ← ASSIGN

---

## ✅ Create First Spell (3 min)

### 1. Create SpellData Asset

- [ ] Project → Right-click → Create → Arcanum Draw → Spell Data
- [ ] Name: `Fireball`

### 2. Configure Properties

```
Basic:
- Spell Name: "Fireball"
- Spell ID: "FIREBALL_SPELL"

Game:
- Mana Cost: 20
- Cooldown Time: 3
- Spell Effect Prefab: (create next)

Recognition:
- Recognition Tolerance: 0.25
- Allow Rotation: ☐ (unchecked)
- Enforce Speed: ☑ (checked)
- Expected Speed Range: X=5, Y=15
- Enforce Direction: ☑ (checked)
- Expected Direction: Clockwise
```

### 3. Generate Template

- [ ] Scroll to bottom of Inspector
- [ ] Click **"Circle"** button
- [ ] Verify "Template Points: 32"

---

## ✅ Create Projectile Prefab (2 min)

### Quick Fireball

- [ ] Hierarchy → Create → 3D Object → Sphere
- [ ] Rename: `FireballProjectile`
- [ ] Add Rigidbody:
  - Mass: `1`
  - Use Gravity: `☐` (unchecked)
- [ ] Scale: `(0.5, 0.5, 0.5)`
- [ ] Material: Red/Orange (optional)
- [ ] Drag to Project → Create Prefab
- [ ] Delete from scene

### Assign to Spell

- [ ] Select `Fireball` SpellData
- [ ] Spell Effect Prefab → Drag `FireballProjectile`

---

## ✅ Add to Recognizer (30 sec)

- [ ] Select `GestureManager`
- [ ] Find `Gesture Recognizer` component
- [ ] Available Spells → Size: `1`
- [ ] Element 0 → Drag `Fireball` SpellData

---

## 🎮 Test Sequence (2 min)

### Test 1: Basic Recognition

**Action:** Play → Draw clockwise circle (moderate speed)

**Expected Console Output:**
```
✅ Gesture Completed: XX points recorded
✅ Recognized: Fireball (XX%)
✅ Speed: XX.XX | Direction: Clockwise
✅ Cast Fireball! Mana: 80/100
✅ Spawned Fireball effect at (X, Y, Z)
✅ Applied force to Fireball towards target
```

**Expected Visual:**
- ✅ Fireball spawns
- ✅ Flies towards opponent
- ✅ Drawn line disappears

**PASS:** ☐

---

### Test 2: Speed Constraint

**Action A:** Draw circle VERY SLOWLY

**Expected:**
```
❌ No matching spell found
```

**PASS:** ☐

**Action B:** Draw circle VERY FAST

**Expected:**
```
❌ No matching spell found
```

**PASS:** ☐

---

### Test 3: Direction Constraint

**Action:** Draw counter-clockwise circle

**Expected:**
```
❌ No matching spell found
(or lower confidence)
```

**PASS:** ☐

---

### Test 4: Mana Depletion

**Action:** Cast Fireball 5 times (100 → 0 mana)

**Expected after 5th cast:**
```
✅ Recognized: Fireball
✅ Not enough mana to cast Fireball. Need 20, have 0
```

**Wait 4 seconds (mana regens to 20)**

**Action:** Draw circle again

**Expected:**
```
✅ Cast Fireball! Mana: 0/100
```

**PASS:** ☐

---

### Test 5: Cooldown

**Action:** Cast Fireball

**Expected:**
```
✅ Cast Fireball! Mana: 80/100
```

**Action:** Immediately draw another circle

**Expected:**
```
✅ Recognized: Fireball
✅ Fireball is on cooldown. Wait 2.Xs
```

**Wait 3 seconds**

**Action:** Draw circle again

**Expected:**
```
✅ Cast Fireball! Mana: 60/100
```

**PASS:** ☐

---

### Test 6: Wrong Shape

**Action:** Draw a square

**Expected:**
```
✅ Gesture Completed: XX points recorded
❌ No matching spell found
```

**PASS:** ☐

---

## 🐛 Troubleshooting

### Nothing happens when drawing

**Check:**
- [ ] Is cursor/finger inside RunePad?
- [ ] Is `GestureDrawingManager` enabled?
- [ ] Check Console for errors

---

### "GestureRecognizer reference is missing"

**Fix:**
- [ ] Select GestureManager
- [ ] Drag GestureManager to Gesture Recognizer field

---

### "SpellCaster reference is missing"

**Fix:**
- [ ] Create Player with SpellCaster component
- [ ] Drag Player to Spell Caster field in GestureDrawingManager

---

### "No matching spell found" (but drew circle correctly)

**Fix Option 1:** Lower strictness
- [ ] Select Fireball SpellData
- [ ] Recognition Tolerance: `0.5` (was 0.25)

**Fix Option 2:** Disable constraints
- [ ] Enforce Speed: `☐` (uncheck)
- [ ] Enforce Direction: `☐` (uncheck)

**Fix Option 3:** Check available spells
- [ ] GestureManager → Gesture Recognizer
- [ ] Verify Fireball is in Available Spells list

---

### Spell recognized but doesn't cast

**Check Console for reason:**

**"Not enough mana"**
- [ ] Wait for mana to regen
- [ ] Or increase starting mana in SpellCaster

**"On cooldown"**
- [ ] Wait for cooldown timer
- [ ] Or reduce cooldown time in SpellData

---

### Fireball spawns but doesn't move

**Fix:**
- [ ] Verify Fireball prefab has Rigidbody
- [ ] Check "Target Opponent" is assigned in SpellCaster
- [ ] Check "Projectile Force" > 0 in SpellCaster
- [ ] Rigidbody constraints: all unchecked

---

### Drawing doesn't clear after cast

**Fix:**
- [ ] Assign "Gesture Drawing Manager" in SpellCaster
- [ ] Method `ClearAllDrawings()` will be called automatically

---

## ✅ Success Criteria

All tests must PASS:

- [ ] ✅ Test 1: Basic Recognition
- [ ] ✅ Test 2: Speed Constraint
- [ ] ✅ Test 3: Direction Constraint
- [ ] ✅ Test 4: Mana Depletion
- [ ] ✅ Test 5: Cooldown
- [ ] ✅ Test 6: Wrong Shape

**All PASS?** System is working perfectly! 🎉

---

## 🎯 Next: Create More Spells

### Lightning Bolt (V-Shape)

```
1. Create SpellData: "Lightning"
2. Mana: 25, Cooldown: 2
3. Recognition Tolerance: 0.3
4. Allow Rotation: ☐
5. Click "V-Shape" button
6. Create lightning prefab (thin cylinder)
7. Add to Available Spells
```

**Test:** Draw V → Lightning casts

---

### Healing Circle (Slow Circle)

```
1. Create SpellData: "Healing"
2. Mana: 30, Cooldown: 5
3. Enforce Speed: ☑
4. Speed Range: X=1, Y=5 (SLOW!)
5. Click "Circle" button
6. Create healing prefab (green sphere)
7. Add to Available Spells
```

**Test:** Draw circle slowly → Healing casts  
**Test:** Draw circle fast → Fireball casts

---

### Spiral Attack

```
1. Create SpellData: "Tornado"
2. Mana: 40, Cooldown: 8
3. Recognition Tolerance: 0.35
4. Click "Spiral" button
5. Create tornado prefab
6. Add to Available Spells
```

**Test:** Draw spiral → Tornado casts

---

## 📊 Performance Check

**Frame Rate:** Should be 60 FPS  
**Draw → Cast Delay:** < 0.1 seconds  
**Console Spam:** Minimal (only on events)

**Issues?**
- Reduce `resamplePointCount` to 32
- Limit `availableSpells` to < 10

---

**Estimated Time:** 12 minutes  
**Difficulty:** Easy  
**Prerequisites:** Drawing system working (Phase 2.2)

**Ready? Let's test!** 🚀
