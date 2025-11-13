# Editor Tools Guide - Arcanum Draw Match System

## 🎯 Quick Setup Tools

I've created **3 powerful editor tools** to automate the entire match system setup. No manual configuration needed!

---

## 🚀 Method 1: Complete Match Setup Wizard (RECOMMENDED)

**Location:** `Tools → Arcanum Draw → Complete Match Setup Wizard`

### What It Does:
✅ **One-click complete setup** for the entire match system  
✅ **Step-by-step wizard** with visual feedback  
✅ **Auto-detects** existing components  
✅ **Configures everything** automatically  

### How to Use:
1. Click `Tools → Arcanum Draw → Complete Match Setup Wizard`
2. Click "Complete Setup Now" button
3. Done! Everything is configured.

### What Gets Created:
```
Scene Hierarchy:
├── MatchManager (new)
│   └── MatchManager component (configured)
├── Player1 (existing)
│   └── PlayerStats component (added)
├── Player2 (existing)
│   └── PlayerStats component (added)
└── MatchHUDCanvas (new)
    └── MatchHUD (new)
        ├── MatchStateText
        ├── MatchTimerText
        ├── VictoryPanel
        ├── Player1HealthBarContainer
        └── Player2HealthBarContainer
```

### Step-by-Step Mode:
If you want to see each step, use the wizard's "Next" button:
1. **Welcome** - Overview and options
2. **Player Setup** - Adds PlayerStats components
3. **Match Manager Setup** - Creates and configures MatchManager
4. **HUD Setup** - Creates complete Match HUD UI
5. **Complete** - Success screen with test instructions

---

## 🎮 Method 2: Individual Component Tools

### Create Match Manager
**Location:** `GameObject → Arcanum Draw → Create Match Manager`  
**Or:** `Tools → Arcanum Draw → Setup Match Manager`

**What It Does:**
- Creates MatchManager GameObject
- Auto-finds Player1 and Player2
- Adds PlayerStats if missing
- Sets player tags to "Player"
- Links all references

**When to Use:**
- You only need the MatchManager
- You want to manually control HUD creation
- You're building the system piece by piece

---

### Create Match HUD
**Location:** `GameObject → Arcanum Draw → Create Match HUD`  
**Or:** `Tools → Arcanum Draw → Setup Match HUD`

**What It Does:**
- Creates new Canvas with proper settings
- Builds complete HUD hierarchy
- Includes:
  - Match state text (top center)
  - Match timer (top right)
  - Victory panel (center, hidden by default)
  - Player 1 health bar (top left)
  - Player 2 health bar (top right)
  - Health text displays
  - Player name labels
- Auto-links MatchManager if found
- All UI elements properly anchored and styled

**When to Use:**
- You already have MatchManager
- You want a professional match HUD
- You need health bars for both players

---

## 📋 Usage Scenarios

### Scenario 1: Fresh Project Setup
```
1. Tools → Arcanum Draw → Complete Match Setup Wizard
2. Click "Complete Setup Now"
3. Press Play to test
```
**Time: 10 seconds**

---

### Scenario 2: Existing Project with Players
```
1. GameObject → Arcanum Draw → Create Match Manager
2. GameObject → Arcanum Draw → Create Match HUD
3. Press Play to test
```
**Time: 20 seconds**

---

### Scenario 3: Only Need HUD
```
1. GameObject → Arcanum Draw → Create Match HUD
2. Assign MatchManager reference in Inspector
```
**Time: 10 seconds**

---

## 🔧 Tool Features

### Complete Match Setup Wizard
| Feature | Description |
|---------|-------------|
| Auto-Detection | Finds existing players and components |
| Smart Setup | Only adds what's missing |
| Visual Feedback | Shows progress for each step |
| Error Handling | Warns if prerequisites missing |
| Undo Support | All changes can be undone (Ctrl+Z) |

### Match Manager Setup Tool
| Feature | Description |
|---------|-------------|
| Auto-Configure | Default settings applied |
| Player Detection | Finds Player1/Player2 automatically |
| Component Addition | Adds PlayerStats if missing |
| Tag Assignment | Sets "Player" tag automatically |
| Reference Linking | Connects all player references |

### Match HUD Setup Tool
| Feature | Description |
|---------|-------------|
| Canvas Creation | Screen Space Overlay, 1920x1080 |
| HUD Hierarchy | Complete structure with proper anchoring |
| TextMeshPro | Uses TMP for all text elements |
| Responsive Design | Scales with screen size |
| Visual Polish | Outlines, colors, proper sizing |
| Auto-Linking | Connects to MatchManager if found |

---

## 🎨 What You Get

### Canvas Settings
```
Canvas:
  Render Mode: Screen Space - Overlay
  Sorting Order: 10

Canvas Scaler:
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 1920 x 1080
  Match: 0.5 (blend width/height)
```

### UI Layout

```
MatchHUDCanvas (Canvas)
└── MatchHUD (RectTransform - full screen)
    ├── MatchStateText (Top Center)
    │   • Font Size: 48
    │   • Color: Yellow
    │   • Outline: Black
    │
    ├── MatchTimerText (Top Right)
    │   • Font Size: 36
    │   • Color: White
    │   • Outline: Black
    │
    ├── VictoryPanel (Full Screen Overlay)
    │   • Background: Black (80% opacity)
    │   • Initially Hidden
    │   └── VictoryText (Center)
    │       • Font Size: 96
    │       • Color: Yellow
    │
    ├── Player1HealthBarContainer (Top Left)
    │   ├── PlayerName ("Player 1")
    │   ├── Player1HealthBar (Slider)
    │   │   • Direction: Left to Right
    │   │   • Fill Color: Green
    │   └── HealthText ("100/100")
    │
    └── Player2HealthBarContainer (Top Right)
        ├── PlayerName ("Player 2")
        ├── Player2HealthBar (Slider)
        │   • Direction: Right to Left
        │   • Fill Color: Green
        └── HealthText ("100/100")
```

---

## ✅ Auto-Configuration Details

### Players (Player1, Player2)
- [x] PlayerStats component added
- [x] Tag set to "Player"
- [x] Max Health: 100
- [x] Current Health: 100
- [x] UI Controller reference linked (if exists)

### Match Manager
- [x] Match Start Delay: 3 seconds
- [x] Match Time Limit: 300 seconds
- [x] Use Time Limit: false (disabled)
- [x] Player 1 Stats: auto-linked
- [x] Player 2 Stats: auto-linked
- [x] Player 1 UI: auto-linked
- [x] Player 2 UI: auto-linked

### Match HUD
- [x] Match State Text: linked
- [x] Match Timer Text: linked
- [x] Victory Panel: linked
- [x] Victory Text: linked
- [x] Player 1 Health Bar: linked
- [x] Player 2 Health Bar: linked
- [x] Player 1 Health Text: linked
- [x] Player 2 Health Text: linked
- [x] Player 1 Name Text: linked
- [x] Player 2 Name Text: linked
- [x] Match Manager: auto-linked

---

## 🧪 Testing Your Setup

### Quick Test (30 seconds)
1. Press **Play**
2. Wait for console message: `"⚔️ MATCH STARTED!"`
3. Draw gesture to cast spell
4. Watch health bars decrease
5. Continue until victory

### Expected Console Output:
```
Match Manager: Players initialized
✓ Added PlayerStats to Player1
✓ Set Player1 tag to 'Player'
✓ Added PlayerStats to Player2
✓ Set Player2 tag to 'Player'
⚔️ Match starting in 3 seconds...
Match State: PreMatch → MatchStarting
⚔️ MATCH STARTED! Begin casting!
Match State: MatchStarting → CastingPhase
💥 Fireball hit Player2 for 10 damage!
Player2 took 10 damage! HP: 90/100
[... more damage ...]
💀 Player2 has been defeated!
🏆 Player1 wins the match!
Match State: CastingPhase → MatchEnding
🎉 Victory for Player1!
Match State: MatchEnding → MatchEnded
```

---

## 🐛 Troubleshooting

### "Player1 and/or Player2 not found in scene"
**Solution:** Create Player1 and Player2 GameObjects before running the wizard.

### "No MatchManager found in scene"
**Solution:** Run `GameObject → Arcanum Draw → Create Match Manager` first.

### "Health bars don't update"
**Solution:** Check that PlayerStats references are set in PlayerUIController.

### "Victory panel doesn't show"
**Solution:** Check that MatchManager has player references assigned.

### Canvas appears but UI is invisible
**Solution:** 
- Check Canvas render mode is "Screen Space - Overlay"
- Check Canvas sorting order is 10 or higher
- Ensure EventSystem exists in scene

---

## 🎯 Menu Reference

### Tools Menu
```
Tools
└── Arcanum Draw
    ├── Complete Match Setup Wizard ⭐ (Recommended)
    ├── Setup Match Manager
    └── Setup Match HUD
```

### GameObject Menu
```
GameObject
└── Arcanum Draw
    ├── Create Match Manager
    └── Create Match HUD
```

---

## 💡 Pro Tips

### Tip 1: Use the Complete Wizard
The Complete Match Setup Wizard is the fastest way to get started. It handles everything automatically.

### Tip 2: Customize After Creation
All created objects can be customized in the Inspector after creation. Change colors, positions, sizes, etc.

### Tip 3: Undo Support
All tools support Unity's Undo system. Press `Ctrl+Z` (or `Cmd+Z` on Mac) to undo any changes.

### Tip 4: Re-run Is Safe
You can re-run the tools safely. They detect existing components and only add what's missing.

### Tip 5: Check the Console
The tools provide detailed feedback in the Console. Check it to see what was created and configured.

---

## 📊 Comparison: Manual vs Automated Setup

| Task | Manual Time | Automated Time | Savings |
|------|-------------|----------------|---------|
| Add PlayerStats to both players | 2 min | Instant | 2 min |
| Set player tags | 1 min | Instant | 1 min |
| Create MatchManager | 1 min | Instant | 1 min |
| Configure MatchManager | 3 min | Instant | 3 min |
| Create Canvas | 1 min | Instant | 1 min |
| Create HUD structure | 5 min | Instant | 5 min |
| Style UI elements | 5 min | Instant | 5 min |
| Link all references | 5 min | Instant | 5 min |
| **TOTAL** | **23 min** | **10 sec** | **~22.5 min** |

---

## 🎉 Next Steps

After running the setup tools:

1. **Test in Play Mode**
   - Verify match countdown
   - Test spell damage
   - Confirm health bars update
   - Watch victory screen

2. **Customize Visuals**
   - Change health bar colors
   - Adjust text sizes and fonts
   - Modify victory panel appearance
   - Add custom backgrounds

3. **Extend Functionality**
   - Add more match states
   - Implement rounds system
   - Add spectator mode
   - Create replay system

4. **Polish**
   - Add sound effects for state changes
   - Animate health bar changes
   - Add damage numbers floating text
   - Create match intro sequence

---

**All tools are ready to use! Start with the Complete Match Setup Wizard for the fastest results!** 🚀
