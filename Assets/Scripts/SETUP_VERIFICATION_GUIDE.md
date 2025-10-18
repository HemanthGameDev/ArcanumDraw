# Setup Verification Guide - Arcanum Draw

## Critical Issue Found & Fixed

### The Problem
The line was not starting at the exact touch point due to a **coordinate space mismatch**:
- Screen positions were being converted to **RunePad's** local space
- But lines were being drawn in **LineContainer's** local space
- These two spaces have different pivots and anchors, causing misalignment

### The Solution
Updated `RunePadController.cs` to:
1. Take a reference to the LineContainer
2. Convert screen positions directly to LineContainer's local space
3. Use proper Canvas camera reference for accurate conversions

---

## Setup Checklist - Follow These Steps

### Step 1: Verify RunePadController References
1. Select the `/GestureSetUp/RunePad` GameObject
2. In the Inspector, find the `RunePadController` component
3. **CRITICAL:** Assign these references:
   - `Rune Pad Rect` → Drag `/GestureSetUp/RunePad` (itself)
   - `Line Container` → Drag `/GestureSetUp/RunePad/LineContainer`

### Step 2: Verify InputManager References
1. Select the `/InputManager` GameObject
2. In the Inspector, check `GestureInputManager` component:
   - `Run Pad` → Must reference `/GestureSetUp/RunePad`
   - `Line Drawer` → Should reference itself (InputManager)

3. Check `LineDrawer` component:
   - `Line Container` → Must reference `/GestureSetUp/RunePad/LineContainer`
   - `Circle Sprite` → Assign your circle sprite asset
   - `Line Width` → 10 (adjust as needed)
   - `Line Color` → Cyan (0, 255, 255, 255)
   - `Clear Fade Duration` → 0.5 seconds
   - `Min Points For Line` → 2

### Step 3: Verify LineContainer Setup
1. Select `/GestureSetUp/RunePad/LineContainer`
2. Verify RectTransform settings:
   - Anchors: Min (0, 0), Max (1, 1) - **stretches to fill RunePad**
   - Pivot: (0.5, 0.5) - **centered**
   - Anchored Position: (0, 0)
   - Size Delta: (0, 0)
   - Scale: (1, 1, 1)

### Step 4: Verify RunePad Masking
1. Select `/GestureSetUp/RunePad`
2. Ensure it has `RectMask2D` component (clips lines to pad boundaries)

### Step 5: Test Input Settings
Adjust these parameters in `GestureInputManager` for best results:
- `Min Distance Between Points` → 0.5 (very sensitive, smooth lines)
- `Aggressive Frame Count` → 10 (adds points every frame for first 10 frames)
- `Double Tap Time Window` → 0.3 seconds
- `Double Tap Max Distance` → 50 pixels

---

## How It Works Now

### Touch Detection
1. Touch anywhere in RunePad → `OnTouchStarted` fires
2. Immediately adds **3 points** at exact touch location
3. Creates visible circle at touch point

### Aggressive Early Tracking
- **First 10 frames:** Points added EVERY frame regardless of distance
- Fills any gap between touch and cursor movement
- After 10 frames: Normal distance checking (0.5 pixels)

### Coordinate Conversion
```
Screen Position → LineContainer Local Position
(Using proper Canvas camera and RectTransform)
```

### Double-Tap Clear
- Tap twice within 0.3 seconds and 50 pixels
- All lines fade out smoothly over 0.5 seconds
- Clean, professional animation

---

## Debugging Tips

### Enable Console Logs
When you draw, you should see:
```
Drawing started at screen position: (x, y)
Point 1: Screen=(sx, sy), Local=(lx, ly)
Point 2: Screen=(sx, sy), Local=(lx, ly)
Point 3: Screen=(sx, sy), Local=(lx, ly)
```

### What to Check
1. **Screen position should match your cursor/touch location**
2. **Local position should be relative to LineContainer center**
3. **All 3 initial points should have identical positions**

### If Line Still Doesn't Start at Touch
1. Check Console for coordinate logs
2. Verify LineContainer is stretched (anchors 0,0 to 1,1)
3. Ensure LineContainer scale is (1,1,1), not inheriting RunePad's scale
4. Check that Canvas is in ScreenSpaceOverlay mode

---

## Expected Behavior

✅ Touch RunePad → Circle appears INSTANTLY at exact touch point
✅ Drag finger → Line follows smoothly with no gaps
✅ Release → Line stays visible
✅ Draw multiple lines → All remain visible
✅ Double-tap → All lines fade out smoothly
✅ Single tap (no drag) → No dot left behind (minimum 2 points required)

---

## Performance Notes

- Lines are persistent (don't auto-vanish)
- Each line is a separate GameObject hierarchy
- Circles use Image components with your circle sprite
- Segments connect circles for smooth rounded appearance
- RectMask2D clips everything to RunePad boundaries

---

Last Updated: Phase 1 Implementation
