# Implementation Checklist - Refactored Gesture System

## Quick Setup Guide

### 🔴 Step 1: Clean Up Old System
- [ ] Select `/InputManager` GameObject
- [ ] Remove `GestureInputManager` component (old)
- [ ] Remove `LineDrawer` component (old)
- [ ] Rename GameObject to `GestureManager`

---

### 🟡 Step 2: Add New Components
- [ ] Select `/GestureManager`
- [ ] Add Component → `GestureDrawingManager`
- [ ] Add Component → `GestureLineRenderer`

---

### 🟢 Step 3: Configure RunePadController
- [ ] Select `/GestureSetUp/RunePad`
- [ ] Assign `Rune Pad Rect` → `/GestureSetUp/RunePad`
- [ ] Assign `Line Container` → `/GestureSetUp/RunePad/LineContainer`

---

### 🟢 Step 4: Configure GestureDrawingManager
- [ ] Select `/GestureManager`
- [ ] Assign `Rune Pad Controller` → `/GestureSetUp/RunePad`
- [ ] Assign `Line Renderer` → `/GestureManager` (itself)
- [ ] Set `Min Distance Between Points` = **1.0**
- [ ] Set `Double Tap Time Window` = **0.3**
- [ ] Set `Double Tap Max Distance` = **50**

---

### 🟢 Step 5: Configure GestureLineRenderer
- [ ] Select `/GestureManager`
- [ ] Assign `Line Container` → `/GestureSetUp/RunePad/LineContainer`
- [ ] Assign `Circle Sprite` → Your circle sprite asset
- [ ] Set `Line Width` = **10**
- [ ] Set `Line Color` = **Cyan (0, 255, 255, 255)**
- [ ] Set `Clear Fade Duration` = **0.5**
- [ ] Set `Min Points To Display` = **2**

---

### 🔵 Step 6: Verify LineContainer
- [ ] Select `/GestureSetUp/RunePad/LineContainer`
- [ ] Verify Anchors: Min **(0, 0)**, Max **(1, 1)**
- [ ] Verify Pivot: **(0.5, 0.5)**
- [ ] Verify Anchored Position: **(0, 0)**
- [ ] Verify Size Delta: **(0, 0)**
- [ ] Verify Scale: **(1, 1, 1)**

---

### 🔵 Step 7: Verify RunePad Masking
- [ ] Select `/GestureSetUp/RunePad`
- [ ] Check for `RectMask2D` component
- [ ] If missing, Add Component → `Rect Mask 2D`

---

## ✅ Testing Checklist

### Basic Drawing
- [ ] Enter Play Mode
- [ ] Touch inside RunePad
- [ ] Circle appears **exactly** at touch point
- [ ] Drag finger
- [ ] Line follows smoothly with no gaps
- [ ] Release finger
- [ ] Line stays visible

### Multiple Gestures
- [ ] Draw first gesture → Stays visible
- [ ] Draw second gesture → Both visible
- [ ] Draw third gesture → All three visible

### Double-Tap Clear
- [ ] Tap twice quickly inside RunePad
- [ ] All lines begin fading
- [ ] Lines disappear smoothly after 0.5 seconds
- [ ] Console shows "Double-Tap Detected" message

### Single Tap (No Drag)
- [ ] Tap once without moving
- [ ] Release immediately
- [ ] No dot appears
- [ ] Console shows "Gesture too short - discarded"

### Boundary Clipping
- [ ] Draw gesture that goes outside RunePad
- [ ] Line is clipped at RunePad edges
- [ ] Only portion inside pad is visible

---

## 🐛 Common Issues & Fixes

### Issue: Line doesn't start at cursor
**Fix:**
- Verify LineContainer is assigned in RunePadController
- Check LineContainer Scale = (1, 1, 1)
- Check Console for position debug messages

### Issue: Line leaks outside RunePad
**Fix:**
- Add `RectMask2D` to `/GestureSetUp/RunePad`
- Verify LineContainer is child of RunePad

### Issue: Double-tap doesn't work
**Fix:**
- Increase `Double Tap Time Window` to 0.5
- Increase `Double Tap Max Distance` to 100
- Check Console for tap detection messages

### Issue: Lines don't appear at all
**Fix:**
- Assign Circle Sprite in GestureLineRenderer
- Check Line Color alpha is 255 (not transparent)
- Verify Canvas is visible

### Issue: Lines don't fade when clearing
**Fix:**
- Check `Clear Fade Duration` > 0
- Verify lines are persistent (min 2 points)
- Check Console for "Cleared N lines" message

---

## 📊 Expected Console Output

### On Touch Down:
```
Drawing Initiated at Screen: (540, 300), Local: (0, -450)
New line started at local position: (0, -450)
```

### On Touch Up:
```
Line finalized with 25 points - Line persists
Gesture Completed: 25 points recorded
Ready for gesture recognition system (Phase 2.2)
```

### On Double-Tap:
```
Double-Tap Detected: Clearing all gesture lines
Cleared 3 lines with fade effect
```

### On Single Tap:
```
Gesture too short - discarded (single tap, no drag)
Line discarded: Only 1 points
```

---

## 🎯 Success Criteria

Your system is working correctly when:

✅ Line starts **precisely** at touch point (no gap)
✅ Line follows finger **smoothly** (no stuttering)
✅ Lines **persist** after release (stay visible)
✅ Multiple lines can be **drawn simultaneously**
✅ Double-tap **clears all** lines with smooth fade
✅ Single taps **don't leave dots** behind
✅ Lines are **clipped** to RunePad boundaries
✅ Console shows **clear debug messages**

---

## 📝 Notes

- Keep `/Assets/Scripts/REFACTORED_IMPLEMENTATION_GUIDE.md` open for detailed explanations
- Old scripts (`GestureInputManager.cs`, `LineDrawer.cs`) can be deleted after testing
- New system is **cleaner**, **more precise**, and **easier to maintain**
- Ready for **Phase 2.2: Gesture Recognition** integration

---

**Setup Time Estimate:** 5-10 minutes
**Difficulty:** Easy (just following checkboxes)
**Result:** Precisely working gesture drawing system
