# 🔍 VISUAL DEBUG CHECKLIST - Make Sure Line is Visible

## 📋 Pre-Test Checklist

Before clicking Play, verify these settings:

### 1. Hierarchy Check ✅
```
Your scene should look like this:

SampleScene
├── GestureSetUp (Canvas)
│   └── RunePad (Image + RunePadController)
│       └── LineContainer (RectTransform)
└── InputManager (GestureInputManager + LineDrawer)
```

**Verify:**
- [ ] LineContainer is CHILD of RunePad
- [ ] InputManager exists at root level
- [ ] GestureSetUp is a Canvas

---

### 2. InputManager Inspector Check ✅

Select `InputManager` and verify BOTH components:

#### GestureInputManager:
```
Run Pad:                [RunePad]              ← Must show RunePad
Line Drawer:            [InputManager]         ← Drag itself here
Min Distance Between:   5
Double Click Time:      0.3
```

#### LineDrawer:
```
Line Container:         [LineContainer]        ← Must be assigned!
Line Width:             10 (or 20 for testing)
Line Color:             Cyan (R:0 G:1 B:1 A:1) ← A=1 is critical!
Fade Out Duration:      0.3
Min Points For Line:    2
Circle Sprite:          None (ignore)
```

**Critical Checks:**
- [ ] Line Container shows "LineContainer" (not "None")
- [ ] Line Color alpha (A) is `1.0` not `0.0`
- [ ] Line Width is at least `10`

---

### 3. RunePad Visual Check ✅

Select `/GestureSetUp/RunePad`:

```
Image Component:
- Source Image:  None (OK)
- Color:         Blue-ish with alpha (should be visible)
- Raycast Target: ✓ Checked (important!)

RectTransform:
- Anchor:        Bottom-Center
- Pos X:         0
- Pos Y:         0 (anchored position, NOT -1170!)
- Width:         800
- Height:        400
```

**Visual Test:**
- [ ] Can you see a blue/colored area at bottom of Game view?
- [ ] If not visible, increase Image Color alpha to 0.5
- [ ] If still not visible, change RunePad local scale to (1, 1, 1)

---

### 4. Canvas Settings Check ✅

Select `/GestureSetUp`:

```
Canvas:
- Render Mode:   Screen Space - Overlay  ← Must be Overlay!
- Pixel Perfect: Unchecked (doesn't matter)

Canvas Scaler:
- UI Scale Mode: Scale with Screen Size
- Reference:     1920 x 1080
```

**Critical:**
- [ ] Render Mode is "Screen Space - Overlay" (NOT Camera or World Space)

---

## 🎮 Testing Procedure

### Test 1: Desktop Mouse/Mousepad

1. Click Play
2. Look at bottom-center of Game view
3. **Double-click QUICKLY** in the blue area (2 clicks within 0.3 seconds)
4. **Keep holding** after second click
5. **Drag mouse** around
6. **Release** to finish

**Expected Result:**
```
Visual:  Cyan line follows cursor
Console: "Drawing started - Mode: Mouse (Double-click)"
         "UI Line started - Color: RGBA(0.000, 1.000, 1.000, 1.000)"
         "Line finished with X points - Starting fade"
```

**If line not visible:**
- Increase Line Width to `30`
- Try different color (Red: R:1 G:0 B:0 A:1)
- Check Game view is focused (click on it)

---

### Test 2: Touch (Device Simulator)

1. Open Window → General → Device Simulator
2. Select a phone (iPhone or Android)
3. Click Play
4. **Single touch** in blue area and drag

**Expected Result:**
```
Visual:  Cyan line follows touch
Console: "Drawing started - Mode: Touch"
         "UI Line started - Color: ..."
         "Line finished with X points - Starting fade"
```

---

## 🐛 Troubleshooting Decision Tree

### Problem: Can't see RunePad area
```
Is GestureSetUp Canvas visible?
├─ NO  → Canvas Render Mode wrong? Should be "Screen Space - Overlay"
└─ YES → Is RunePad Image visible?
          ├─ NO  → Increase Image Color alpha to 0.5
          │        OR check RunePad scale (should be 6,6,1 or 1,1,1)
          └─ YES → RunePad is OK, continue testing
```

### Problem: No line appears when drawing
```
Do you see Console messages?
├─ NO  → Input not working
│        ├─ EventSystem missing?
│        ├─ References not assigned?
│        └─ Input System not enabled?
└─ YES → Drawing works, but line invisible
          ├─ Line Width too small? Try 30
          ├─ Line Color alpha = 0? Set to 1
          ├─ LineContainer wrong parent?
          └─ UILineRenderer script error? Check Console
```

### Problem: Wrong input behavior
```
What happens when you click?
├─ Single click draws (WRONG for desktop)
│  └─ FIX: Code might be in touch mode, check Console "Mode:"
├─ Double-click does nothing
│  └─ FIX: Click faster (< 0.3 sec) OR increase Double Click Time
├─ Nothing happens at all
│  └─ FIX: Check RunePad reference assigned in GestureInputManager
└─ Line appears but wrong position
   └─ FIX: Check LineContainer is child of RunePad
```

---

## 📊 Console Message Decoder

### Good Messages ✅:
```
"Drawing started - Mode: Touch"
→ Input detected, drawing activated

"UI Line started - Color: RGBA(0.000, 1.000, 1.000, 1.000)"
→ Line object created successfully

"Line finished with 115 points - Starting fade"
→ Drawing ended, fade animation started
```

### Warning Messages ⚠️:
```
"Trying to add point but no UI line exists!"
→ Line object not created properly
→ Check Line Container reference

"Line too short (1 points), destroying immediately"
→ Not enough movement detected
→ This is normal for quick clicks
```

### Bad Messages ❌:
```
"NullReferenceException: Object reference not set..."
→ Missing reference in Inspector
→ Check all fields are assigned

"The referenced script on this Behaviour is missing!"
→ Script compilation error
→ Check Console for red errors
```

---

## 🎨 Visual Settings for Testing

### To Make Line MORE Visible:
```
Line Width:  30 (or even 50)
Line Color:  Pure Red (R:1, G:0, B:0, A:1)
             OR Pure White (R:1, G:1, B:1, A:1)
Fade Out:    2.0 (slower fade, easier to see)
```

### For Production (After Testing):
```
Line Width:  10 (nice and smooth)
Line Color:  Cyan (R:0, G:1, B:1, A:1) with glow
Fade Out:    0.3 (quick and snappy)
```

---

## 🔍 Scene View vs Game View

**IMPORTANT:** Lines only appear in **Game View**, not Scene View!

```
Scene View:
- Shows Unity editor scene
- Lines won't appear here
- Used for positioning objects

Game View:
- Shows actual game rendering
- Lines WILL appear here  ← Watch this!
- Used for testing gameplay
```

**Make sure you're looking at Game View when testing!**

---

## ✅ Success Criteria

You'll know it's working when:

1. **Visual:**
   - [ ] See RunePad blue area at bottom
   - [ ] See cyan line when drawing
   - [ ] Line follows input smoothly
   - [ ] Line fades out after release

2. **Console:**
   - [ ] "Drawing started" message
   - [ ] "UI Line started" message
   - [ ] "Line finished" message
   - [ ] No warnings or errors

3. **Behavior:**
   - [ ] Desktop: Double-click required
   - [ ] Mobile: Single touch works
   - [ ] Drawing only in RunePad area
   - [ ] No drawing outside RunePad

---

## 📸 Screenshot Your Settings

If it's still not working, screenshot these and share:

1. **InputManager Inspector** (both components expanded)
2. **RunePad Inspector** (showing Image and RectTransform)
3. **Hierarchy** (showing GestureSetUp and children)
4. **Console** (showing any warnings/errors)
5. **Game View** (showing where you're clicking)

---

## 🚀 Quick Fix Commands

### If line too thin to see:
1. Select InputManager
2. Find LineDrawer component
3. Change Line Width to `30`
4. Click Play again

### If color not visible:
1. Select InputManager
2. Find LineDrawer component
3. Change Line Color to Red (R:1, G:0, B:0, A:1)
4. Click Play again

### If RunePad not visible:
1. Select /GestureSetUp/RunePad
2. Find Image component
3. Change Color to Blue (R:0, G:0, B:1, A:0.5)
4. Check if you can see it now

---

**Follow this checklist step-by-step and the line WILL appear!** 🎯

