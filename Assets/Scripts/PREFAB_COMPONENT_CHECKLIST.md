# Spell Prefab Component Checklist

## 🔥 FireballEffect.prefab

### Current Components
```
FireballEffect
├── Transform
├── Mesh Filter (Sphere)
├── Mesh Renderer (Red material)
├── Sphere Collider
└── Rigidbody
```

### Required Changes
```diff
FireballEffect
├── Transform
├── Mesh Filter (Sphere)
├── Mesh Renderer (Red material)
├── Sphere Collider
+   └── Is Trigger: ✓ ENABLE THIS
├── Rigidbody
│   └── Use Gravity: ✗ DISABLE THIS
+ └── SpellProjectile ← ADD THIS COMPONENT
+     ├── Lifetime: 5
+     ├── Speed: 15
+     ├── Damage: 20
+     └── Use Rigidbody Force: ✓
```

**Action Steps:**
1. Open `FireballEffect.prefab`
2. Click "Add Component" → Type "SpellProjectile"
3. In Sphere Collider, check ✓ `Is Trigger`
4. In Rigidbody, uncheck ✗ `Use Gravity`
5. Apply → Save

---

## ⚡ LighteningEffect.prefab

### Current Components
```
LighteningEffect
├── Transform
├── Mesh Filter (Sphere)
├── Mesh Renderer (Blue material)
├── Sphere Collider
└── Rigidbody
```

### Required Changes
```diff
LighteningEffect
├── Transform
├── Mesh Filter (Sphere)
├── Mesh Renderer (Blue material)
- ├── Sphere Collider ← OPTIONAL: Can remove
- └── Rigidbody ← REMOVE THIS (lightning doesn't fly)
+ └── LightningEffect ← ADD THIS COMPONENT
+     ├── Lifetime: 2
+     ├── Strike Delay: 0.2
+     ├── Damage: 25
+     ├── Chain Range: 5
+     ├── Max Chain Targets: 3
+     └── Flash Duration: 0.1
+ └── Light (Optional) ← ADD FOR FLASH EFFECT
+     ├── Type: Point
+     ├── Range: 10
+     ├── Intensity: 5
+     └── Color: Light Blue
```

**Action Steps:**
1. Open `LighteningEffect.prefab`
2. Select Rigidbody → Right-click → "Remove Component"
3. Click "Add Component" → Type "LightningEffect"
4. (Optional) Add Component → Type "Light" for flash effect
5. Apply → Save

---

## 🛡️ Shieldeffect.prefab

### Current Components
```
Shieldeffect
├── Transform
│   └── Scale: (0.005, 0.0060869996, 0.000253625) ← Z is TINY!
├── Mesh Filter (Sphere)
├── Mesh Renderer (Cyan material)
├── Sphere Collider
└── Rigidbody
```

### Required Changes
```diff
Shieldeffect
├── Transform
-   └── Scale: (0.005, 0.0060869996, 0.000253625)
+   └── Scale: (0.5, 0.5, 0.5) ← FIX THIS
├── Mesh Filter (Sphere)
├── Mesh Renderer (Cyan material)
│   └── Material: Must support transparency
- ├── Sphere Collider ← OPTIONAL: Can remove
- └── Rigidbody ← REMOVE THIS (shield doesn't move)
+ └── ShieldEffect ← ADD THIS COMPONENT
+     ├── Duration: 5
+     ├── Fade In Duration: 0.3
+     ├── Fade Out Duration: 0.5
+     ├── Damage Absorption: 50
+     ├── Max Scale: 2
+     ├── Pulse Speed: 2
+     └── Shield Color: (0, 0.5, 1, 0.5)
```

**Action Steps:**
1. Open `Shieldeffect.prefab`
2. In Transform, change Scale to `X:0.5, Y:0.5, Z:0.5`
3. Select Rigidbody → Right-click → "Remove Component"
4. Click "Add Component" → Type "ShieldEffect"
5. Verify material supports transparency (Rendering Mode: Transparent)
6. Apply → Save

---

## 📝 Quick Copy-Paste Values

### FireballEffect → SpellProjectile Settings
```
Lifetime: 5
Speed: 15
Damage: 20
Use Rigidbody Force: ✓
```

### LighteningEffect → LightningEffect Settings
```
Lifetime: 2
Strike Delay: 0.2
Damage: 25
Chain Range: 5
Max Chain Targets: 3
Flash Duration: 0.1
Fade Out Duration: 0.5
Lightning Color: R:0.5 G:0.5 B:1 A:1
```

### Shieldeffect → ShieldEffect Settings
```
Duration: 5
Fade In Duration: 0.3
Fade Out Duration: 0.5
Damage Absorption: 50
Max Scale: 2
Pulse Speed: 2
Pulse Intensity: 0.1
Shield Color: R:0 G:0.5 B:1 A:0.5
```

---

## ✅ Verification Checklist

After updating each prefab, verify:

### FireballEffect
- [ ] Has `SpellProjectile` component
- [ ] Sphere Collider has `Is Trigger` checked
- [ ] Rigidbody has `Use Gravity` unchecked
- [ ] Prefab saved (no asterisk in tab name)

### LighteningEffect
- [ ] Has `LightningEffect` component
- [ ] Rigidbody removed (or Use Gravity unchecked)
- [ ] (Optional) Has Light component for flash
- [ ] Prefab saved

### Shieldeffect
- [ ] Has `ShieldEffect` component
- [ ] Transform scale is reasonable (not 0.000253625 on Z!)
- [ ] Rigidbody removed
- [ ] Material supports transparency
- [ ] Prefab saved

---

## 🎮 Testing Each Spell

### Test Script (Copy to Console if needed)
```
1. Enter Play Mode
2. Draw gesture for each spell
3. Observe behavior:

FIREBALL:
✓ Spawns at spell spawn point
✓ Flies straight (or towards Player2)
✓ Lasts 5 seconds OR until collision
✓ Console: "Spawned fireball effect at..."
✓ Console: "Hit [target] for 20 damage!"

LIGHTNING:
✓ Appears at Player2 position
✓ Flashes briefly
✓ Chains to nearby targets (if any within 5 units)
✓ Lasts 2 seconds
✓ Console: "Lightning struck [target] for 25 damage!"

SHIELD:
✓ Appears at Player1 position
✓ Grows from 0 to max size
✓ Follows Player1 as you move (test in Scene view)
✓ Pulses gently
✓ Lasts 5 seconds then fades
```

---

## 🐛 Troubleshooting Quick Reference

| Problem | Solution |
|---------|----------|
| **Spell disappears instantly** | Did you add the script component? |
| **Fireball doesn't move** | Check Rigidbody exists + Use Gravity OFF |
| **Fireball falls down** | Uncheck Use Gravity on Rigidbody |
| **Lightning doesn't appear** | Check Player2 is assigned as targetOpponent |
| **Shield doesn't show** | Check Transform scale (Z might be 0.0002) |
| **Shield doesn't follow** | Script auto-sets this - verify component exists |
| **No damage messages** | Normal - damage system placeholder, will add later |
| **Can't save prefab** | Click "Overrides" dropdown → "Apply All" |

---

## 📸 Before & After Comparison

### BEFORE (Not Working)
```
Spell spawns → Exists for 1 frame → Unity cleanup → Disappears
(No script managing lifetime)
```

### AFTER (Working!)
```
FIREBALL:
Spawn → Apply force → Fly towards target → [5 seconds pass] → Destroy
                                      ↓ (or)
                              Hit target → Impact → Destroy

LIGHTNING:
Spawn → [0.2s delay] → Strike target → Chain to nearby → Fade out → Destroy

SHIELD:
Spawn → Fade in → Follow caster → Pulse → [5s pass] → Fade out → Destroy
                              ↓ (or)
                    Absorption depleted → Fade out → Destroy
```

---

**You're all set!** Just follow the steps for each prefab and your spells will work perfectly. 🎯✨
