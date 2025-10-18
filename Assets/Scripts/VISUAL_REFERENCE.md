# 🎨 ARCANUM DRAW - VISUAL REFERENCE GUIDE

## 📐 SCREEN LAYOUT

```
┌─────────────────────────────────────────────────────────┐
│  [❤️ HP: ████████░░] [⚡ Mana: ██████░░░░] [Avatar]    │ ← Top-left HUD (Phase 4)
│                                                         │
│                                                         │
│                  🏛️ 2.5D Arena View                     │
│                                                         │
│              👤 Player 1    vs    👤 Player 2           │
│                                                         │
│                  [Spell Projectiles]                    │
│                                                         │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │ [🔥] [⚡] [❄️] [🌀] [💨]                          │   │ ← Spell Icons (Phase 4)
│  ├─────────────────────────────────────────────────┤   │
│  │                                                 │   │
│  │        ✨ Glowing Line Trail ✨                  │   │ ← Line Drawing (Phase 1)
│  │                                                 │   │
│  └─────────────────────────────────────────────────┘   │ ← RunePad Border (Phase 1)
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 PHASE 1 COMPONENTS (What you're building now)

### 1. RunePad (The Casting Area)
```
┌────────────────────────────────────┐
│  ╔══════════════════════════════╗  │ ← Magical border (optional)
│  ║                              ║  │
│  ║  👆 Touch & Draw Here       ║  │ ← Semi-transparent area
│  ║                              ║  │    (Dark blue tint)
│  ║                              ║  │
│  ╚══════════════════════════════╝  │
└────────────────────────────────────┘

Position: Bottom-center
Size: 800 x 400 pixels
Color: RGBA(0.1, 0.1, 0.2, 0.3)
```

### 2. Line Trail (Active Drawing)
```
Touch Start ───→ Drawing ───→ Touch End ───→ Fade Out

    👆              ✏️              🖐️              💨
                    │
                    ↓
            ╭─────────╮
            │  ●───●  │  ← Glowing cyan line
            │ ●     ● │     Width: 10 pixels
            │●       ●│     Color: #00FFFF
            ╰─────────╯     Emission: ON
```

---

## 🎨 COLOR PALETTE

### Phase 1 Colors:
```
RunePad Background:    RGB(25, 25, 51)    Alpha: 0.3
Line Trail:            RGB(0, 255, 255)   (Cyan)
Line Emission:         RGB(0, 255, 255)   (Cyan glow)
```

### Future Phases:
```
Fire Spell:            RGB(255, 100, 0)   (Orange-red)
Lightning:             RGB(255, 255, 100) (Yellow)
Ice:                   RGB(100, 200, 255) (Light blue)
Wind:                  RGB(200, 255, 200) (Light green)
```

---

## 📏 MEASUREMENTS & SPECIFICATIONS

### RunePad
- **Position:** Anchored bottom-center
- **Offset Y:** 200 pixels from bottom
- **Width:** 800 pixels (40% of 1920px screen)
- **Height:** 400 pixels
- **Padding:** 20 pixels internal padding (future)

### Line Trail
- **Width:** 10 pixels (adjustable: 8-12)
- **Color:** Cyan with emission
- **Fade Duration:** 0.3 seconds
- **Min Points:** 2 points minimum
- **Point Distance:** 5 pixels between recorded points

### Canvas Settings
- **Render Mode:** Screen Space - Overlay
- **Scaling Mode:** Scale with Screen Size
- **Reference Resolution:** 1920 x 1080
- **Match:** 0.5 (balance width/height)

---

## 🎬 ANIMATION TIMELINE

### Drawing Gesture Flow (Phase 1)
```
Time: 0.0s → 0.0s → 0.5s → 1.0s → 1.0s → 1.3s
      │      │      │      │      │      │
      │      │      │      │      │      └─ Line fully faded
      │      │      │      │      └─ Fade starts (0.3s duration)
      │      │      │      └─ Touch released
      │      │      └─ Continue drawing
      │      └─ Line appears & follows
      └─ Touch starts
```

### Future: Gesture Recognition Flow (Phase 2)
```
Time: 0.0s → 1.0s → 1.05s → 1.2s → 1.5s
      │      │      │       │      │
      │      │      │       │      └─ Spell cast
      │      │      │       └─ Perfect symbol pulses
      │      │      └─ Snap to perfect symbol
      │      └─ Touch released
      └─ Drawing gesture
```

---

## 🖼️ HIERARCHY STRUCTURE

### Current (Phase 1)
```
SampleScene
├── Main Camera
├── Directional Light
├── Area Light
├── Global Volume
├── BackGround
├── ArcanumPlatform
├── Player1
├── Player2
├── GestureSetUp (Canvas)
│   └── RunePad (Image + RunePadController)
│       └── LineContainer (RectTransform)
│           └── [Lines spawn here at runtime]
├── InputManager (GestureInputManager + LineDrawer)
└── EventSystem
```

### Future (Phase 4)
```
GestureSetUp (Canvas)
├── RunePad
│   ├── Border (Image)
│   └── LineContainer
├── SpellLoadout
│   ├── SpellIcon1 (Fire)
│   ├── SpellIcon2 (Lightning)
│   ├── SpellIcon3 (Ice)
│   ├── SpellIcon4 (Wind)
│   └── SpellIcon5 (Earth)
└── PlayerHUD
    ├── HealthBar
    ├── ManaBar
    └── AvatarIcon
```

---

## ✨ VISUAL EFFECTS BREAKDOWN

### Phase 1 VFX:
1. **Line Glow:**
   - Shader: URP/Particles/Unlit
   - Emission enabled
   - Additive blending

2. **Fade Animation:**
   - Linear alpha fade
   - Duration: 0.3 seconds
   - Easing: Linear (simple)

### Future Phase 2 VFX:
1. **Recognition Pulse:**
   - Scale: 1.0 → 1.2 → 1.0
   - Duration: 0.3 seconds
   - Easing: EaseOutBack

2. **Particle Burst:**
   - Count: 20-30 particles
   - Lifetime: 0.5 seconds
   - Spread: 360 degrees

---

## 📱 RESPONSIVE DESIGN

### Desktop (1920x1080)
```
RunePad: 800x400px
Position: Bottom-center, Y+200
Icons: 60x60px each
```

### Tablet (1024x768)
```
RunePad: 600x300px
Position: Bottom-center, Y+150
Icons: 50x50px each
```

### Mobile (750x1334)
```
RunePad: 700x350px (full width - margin)
Position: Bottom-center, Y+100
Icons: 45x45px each
```

---

## 🎯 GESTURE SHAPES (Future Phase 2)

### Basic Gestures to Implement:
```
V-Shape (Lightning):     Fireball (Spiral):    Ice (Circle):
    │                        ╭──╮                  ╭───╮
   ╱ ╲                      ╱    ╲                │   │
  ╱   ╲                    │      │               │   │
                           ╰──────╯               ╰───╯

Line (Wind):            Wave (Water):         Triangle (Earth):
────────────            ╱╲╱╲╱╲                    ╱\
                       ╱  ╲  ╲                   ╱  \
                                                 ╱────\
```

---

## 🔍 TESTING CHECKLIST

### Visual Tests:
- [ ] RunePad is visible and semi-transparent
- [ ] RunePad is positioned correctly (bottom-center)
- [ ] Line trail is glowing cyan color
- [ ] Line trail has visible emission
- [ ] Line fades smoothly (not instant)
- [ ] UI scales properly on different resolutions

### Interaction Tests:
- [ ] Line appears when clicking/touching RunePad
- [ ] Line follows mouse/finger smoothly
- [ ] Line stops at RunePad boundaries
- [ ] No line appears outside RunePad
- [ ] Line fades after releasing

### Performance Tests:
- [ ] No lag when drawing fast
- [ ] Multiple lines don't cause slowdown
- [ ] Memory usage is reasonable
- [ ] Works in Device Simulator

---

## 📐 REFERENCE SIZES

### UI Element Sizing Guide:
```
Component          Width    Height   Notes
─────────────────────────────────────────────
RunePad            800px    400px    Main drawing area
Spell Icon         60px     60px     Square, rounded corners
Health Bar         200px    20px     Top-left
Mana Bar           200px    20px     Below health
Avatar Icon        40px     40px     Next to bars
Border Width       4px      -        Around RunePad
Line Trail         10px     -        Dynamic length
```

---

## 💡 IMPLEMENTATION TIPS

1. **Canvas Setup:**
   - Always use Screen Space - Overlay for UI
   - Set reference resolution for consistent sizing
   - Use anchors for responsive positioning

2. **Line Rendering:**
   - LineRenderer uses local space for UI
   - Parent to RunePad for proper positioning
   - Pool lines for better performance (Phase 3)

3. **Input Handling:**
   - New Input System unifies touch/mouse
   - Check position is inside RunePad bounds
   - Record points at regular intervals

4. **Material Setup:**
   - Use URP shaders for compatibility
   - Enable emission for glow effect
   - Set transparent rendering mode

---

**Use this reference while implementing Phase 1!**  
Keep it open alongside the Quick Setup guide.

