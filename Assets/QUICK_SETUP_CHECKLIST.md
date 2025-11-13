# Phase 1.1 - Quick Setup Checklist

## ⚡ 5-Minute Setup Guide

### ✅ Step 1: Add PlayerStats (2 min)
```
1. Select Player1 in Hierarchy
2. Add Component → PlayerStats
   - Max Health: 100
   - Current Health: 100
   - UI Controller: (drag PlayerUIController from Player1 or GestureSetUp)

3. Select Player2 in Hierarchy
4. Add Component → PlayerStats
   - Max Health: 100
   - Current Health: 100
   - UI Controller: (leave empty for now)
```

### ✅ Step 2: Create MatchManager (2 min)
```
1. Right-click in Hierarchy → Create Empty
2. Rename to "MatchManager"
3. Add Component → MatchManager
   - Match Start Delay: 3
   - Match Time Limit: 300
   - Use Time Limit: ☐ (unchecked)
   - Player 1 Stats: (drag Player1)
   - Player 2 Stats: (drag Player2)
   - Player 1 UI: (drag GestureSetUp/PlayerUIController if exists)
   - Player 2 UI: (leave empty)
```

### ✅ Step 3: Update PlayerUIController (1 min)
```
1. Find GameObject with PlayerUIController component
   (Usually under GestureSetUp or UIManager)
2. In PlayerUIController component:
   - Player Stats: (drag Player1)
```

### ✅ Step 4: Verify Tags (30 sec)
```
1. Select Player1 → Tag: Player ✓
2. Select Player2 → Tag: Player ✓
```

### ✅ Step 5: Test! (Enter Play Mode)
```
Expected Console Output:
✓ "Match Manager: Players initialized"
✓ "Match starting in 3 seconds..."
✓ "MATCH STARTED! Begin casting!"
✓ Cast fireball → "Player2 took 10 damage! HP: 90/100"
✓ Continue until → "Player1 wins the match!"
✓ "Match ended"
```

---

## 🎯 Component Summary

### Scene Hierarchy Should Look Like:
```
SampleScene
├── Main Camera
├── Directional Light
├── BackGround
├── ArcanumPlatform
├── Player1 ⭐ + PlayerStats
├── Player2 ⭐ + PlayerStats
├── GestureSetUp
│   └── (PlayerUIController here or in UIManager)
├── MatchManager ⭐ NEW GameObject
└── EventSystem
```

### Required Components:

**Player1:**
- Transform
- MeshFilter
- MeshRenderer
- BoxCollider
- SpellCaster (existing)
- **PlayerStats** ⭐ NEW

**Player2:**
- Transform
- MeshFilter
- MeshRenderer
- BoxCollider
- **PlayerStats** ⭐ NEW

**MatchManager (new GameObject):**
- Transform
- **MatchManager** ⭐ NEW

---

## 🔧 Inspector Settings Quick Reference

### PlayerStats Component
| Field | Value |
|-------|-------|
| Max Health | 100 |
| Current Health | 100 |
| UI Controller | PlayerUIController reference |

### MatchManager Component
| Field | Value |
|-------|-------|
| Match Start Delay | 3 |
| Match Time Limit | 300 |
| Use Time Limit | false |
| Player 1 Stats | Player1 |
| Player 2 Stats | Player2 |
| Player 1 UI | PlayerUIController |
| Player 2 UI | (empty) |

### PlayerUIController Component (Updated)
| Field | Value |
|-------|-------|
| Health Slider | (existing) |
| Mana Slider | (existing) |
| Health Text | (existing) |
| Mana Text | (existing) |
| Spell Caster | (existing) |
| **Player Stats** | **Player1** ⭐ NEW |
| Drawing Manager | (existing) |
| Gesture Recognizer | (existing) |

---

## 🎮 Test Sequence

1. **Press Play**
   - ✅ See "Match starting..." in console
   - ✅ Wait 3 seconds
   - ✅ See "MATCH STARTED!"

2. **Draw Fireball Gesture** (circle counter-clockwise)
   - ✅ Fireball spawns
   - ✅ Fireball flies toward Player2
   - ✅ Console shows damage

3. **Check Health Bar**
   - ✅ Player2 health decreases
   - ✅ Number updates (e.g., 90/100)

4. **Continue Casting**
   - ✅ Cast 10 fireballs total
   - ✅ Player2 HP reaches 0
   - ✅ Victory message appears
   - ✅ "Player1 wins the match!"

---

## 🐛 Common Issues & Fixes

| Problem | Solution |
|---------|----------|
| "Fireball hit but no PlayerStats component found!" | Add PlayerStats to Player2 |
| Spells pass through player | Set Player tag to "Player" |
| Health bar doesn't update | Set Player Stats reference in PlayerUIController |
| Match doesn't end | Check MatchManager has player references |
| No damage in console | Check both players have PlayerStats |
| Multiple MatchManagers warning | Delete duplicate MatchManager GameObjects |

---

## 📊 Expected Damage Flow

```
Fireball Cast
    ↓
Fireball spawns (speed: 5 units/s)
    ↓
Fireball travels toward Player2
    ↓
Collision detected with Player2
    ↓
Console: "🔥 Fireball triggered collision with Player2 (Tag: Player)"
    ↓
SpellProjectile.ApplyDamage(Player2)
    ↓
PlayerStats.TakeDamage(10)
    ↓
Console: "Player2 took 10 damage! HP: 90/100"
    ↓
OnHealthChanged event fires
    ↓
PlayerUIController updates health bar
    ↓
Health bar shows 90/100
    ↓
If HP ≤ 0 → Match ends
```

---

## ✨ Success Indicators

You'll know everything works when:
- ✅ Console shows match start countdown
- ✅ Console shows "MATCH STARTED!"
- ✅ Fireball damages reduce HP (console logs)
- ✅ Health bar visually decreases
- ✅ Health text shows correct values
- ✅ Match ends when HP reaches 0
- ✅ Winner is announced
- ✅ No errors in console

---

## 🎯 Quick Commands

**Restart Match (in code):**
```csharp
MatchManager.Instance.RestartMatch();
```

**Get Current Match State:**
```csharp
MatchState state = MatchManager.Instance.CurrentState;
```

**Get Winner:**
```csharp
PlayerStats winner = MatchManager.Instance.Winner;
```

**Manual Damage Test:**
```csharp
// In console or debug script
Player1.GetComponent<PlayerStats>().TakeDamage(25f);
```

---

That's it! Your match system is ready to go! 🎉
