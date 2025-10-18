# 🎮 INPUT BEHAVIOR GUIDE - How to Draw Gestures

## 🖱️ DESKTOP/LAPTOP (Mouse & Mousepad)

### How It Works: DOUBLE-CLICK TO DRAW

```
Step 1: First Click
┌─────────────┐
│   Click 1   │ ← Click once in RunePad
└─────────────┘
      ↓
   (Waiting...)
      ↓
Step 2: Second Click (within 0.3 seconds)
┌─────────────┐
│   Click 2   │ ← Click again quickly
└─────────────┘
      ↓
   ✅ ACTIVATED!
      ↓
Step 3: Hold and Drag
┌─────────────┐
│ Hold + Drag │ ← Keep button pressed and move mouse
└─────────────┘
      ↓
   ✨ Line appears and follows cursor
      ↓
Step 4: Release
┌─────────────┐
│   Release   │ ← Let go of mouse button
└─────────────┘
      ↓
   💨 Line fades out
```

### Visual Example:
```
Click 1          Click 2         Drag            Release
   ↓                ↓              ↓                ↓
   •                •         •──────────•         •────────💨
   ▼                ▼              ▼                ▼
 (wait)         (activate)      (draw)          (fade)
```

### Important Notes:
- ⏱️ You have **0.3 seconds** between clicks
- ⚠️ Must hold mouse button down after second click
- ⚠️ Must be inside the RunePad area
- ⚠️ Single click does nothing

### Common Mistakes:
❌ Click too slow (> 0.3 seconds apart)
❌ Release after second click (must hold)
❌ Click outside RunePad area
✅ Quick double-click + hold + drag

---

## 📱 MOBILE/TABLET (Touch Screen)

### How It Works: TOUCH AND DRAG

```
Step 1: Touch
┌─────────────┐
│    Touch    │ ← Touch screen in RunePad
└─────────────┘
      ↓
   ✅ DRAWING STARTS IMMEDIATELY!
      ↓
Step 2: Drag
┌─────────────┐
│     Drag    │ ← Move finger while touching
└─────────────┘
      ↓
   ✨ Line follows finger
      ↓
Step 3: Release
┌─────────────┐
│   Release   │ ← Lift finger from screen
└─────────────┘
      ↓
   💨 Line fades out
```

### Visual Example:
```
Touch           Drag            Release
  ↓              ↓                ↓
  👆         👆─────────👆        👆────────💨
  ▼              ▼                ▼
(start)        (draw)          (fade)
```

### Important Notes:
- ⚡ Instant activation on first touch
- ⚠️ Must be inside the RunePad area
- ✅ No double-tap needed!

---

## 🎯 WHY THIS DESIGN?

### Desktop Reasoning:
**Problem:** Mouse moves constantly on screen  
**Solution:** Double-click activation prevents accidental drawing

**Benefits:**
- 🎯 Intentional drawing only
- 🚫 No accidental lines when clicking UI
- 👍 Natural for desktop users

### Mobile Reasoning:
**Problem:** Touch is always intentional  
**Solution:** Immediate response for better UX

**Benefits:**
- ⚡ Instant feedback
- 👍 Natural for mobile users
- 🎮 Matches mobile game conventions

---

## 🧪 TESTING SCENARIOS

### Test 1: Desktop Double-Click
```
Action                      Expected Result
─────────────────────────────────────────────────────
Single click in RunePad  → Nothing happens
Double-click in RunePad  → "Drawing started - Mode: Mouse"
Drag after double-click  → Cyan line follows cursor
Release button           → Line fades out
Single click outside     → Nothing happens
```

### Test 2: Mobile Touch
```
Action                      Expected Result
─────────────────────────────────────────────────────
Touch in RunePad         → "Drawing started - Mode: Touch"
Drag finger              → Cyan line follows touch
Release finger           → Line fades out
Touch outside RunePad    → Nothing happens
```

### Test 3: Mode Detection
```
Platform                    Detection
─────────────────────────────────────────────────────
Unity Editor (Mouse)     → Mouse mode (double-click)
Unity Editor (Touch sim) → Touch mode (single touch)
Android Build            → Touch mode (single touch)
iOS Build                → Touch mode (single touch)
Windows Build            → Mouse mode (double-click)
```

---

## 🎨 VISUAL FEEDBACK

### Console Messages:
```
When you start drawing:
"Drawing started - Mode: Mouse (Double-click)"
or
"Drawing started - Mode: Touch"

When line is created:
"Line started - Material: Assigned, Color: (0.0, 1.0, 1.0, 1.0)"

When drawing ends:
"Line finished with 25 points - Starting fade"
or
"Line too short (1 points), destroying immediately"
```

---

## 🔧 ADJUSTABLE SETTINGS

### In GestureInputManager Inspector:

**Double Click Time Window:**
- Default: `0.3` seconds
- Decrease for faster double-click requirement
- Increase for easier double-click activation

**Min Distance Between Points:**
- Default: `5` pixels
- Decrease for smoother lines (more points)
- Increase for better performance (fewer points)

### In LineDrawer Inspector:

**Line Width:**
- Default: `10` pixels
- Increase if line is too thin to see
- Decrease for finer detail

**Fade Out Duration:**
- Default: `0.3` seconds
- Increase for slower fade
- Decrease for quicker disappearance

---

## 💡 PRO TIPS

### For Testing Desktop:
1. **Practice the double-click rhythm**: Click-Click-Drag
2. **Keep holding after second click**: Don't release!
3. **Start slow**: Practice getting the timing right
4. **Use Debug Console**: Watch for "Drawing started" message

### For Testing Mobile:
1. **Use Device Simulator**: Window → General → Device Simulator
2. **Select a phone**: iPhone or Android device
3. **Test touch**: Single touch and drag
4. **Watch for instant feedback**: Line should appear immediately

### For Troubleshooting:
1. **Always check Console**: Debug messages tell you what's happening
2. **Verify RunePad visibility**: Should see blue semi-transparent area
3. **Test in different areas**: Try different spots in RunePad
4. **Check references**: Make sure all Inspector fields are assigned

---

## 🚀 QUICK START COMMANDS

### To test right now:

1. **Fix RunePad position:**
   - Select `/GestureSetUp/RunePad`
   - Set Anchored Position Y to `200`

2. **Assign references:**
   - Select `InputManager`
   - Drag RunePad to "Run Pad" field
   - Drag InputManager to "Line Drawer" field
   - Drag LineContainer to "Line Container" field

3. **Click Play and test:**
   - **Desktop**: Double-click in blue area and drag
   - **Mobile**: Open Device Simulator and touch-drag

4. **Watch Console:**
   - You should see "Drawing started" message
   - Then "Line finished" when you release

---

**Remember:** Desktop = Double-click, Mobile = Single-touch!

