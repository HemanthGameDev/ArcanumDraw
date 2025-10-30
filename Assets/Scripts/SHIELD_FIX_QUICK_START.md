# SHIELD NOT WORKING? - QUICK FIX (30 seconds)

## The Problem

You drew a **perfect shield** but it's not being recognized. The system is **TOO STRICT**.

## The Solution (3 Steps)

### Step 1: Select GestureManager
In Hierarchy, click on `/GestureManager`

### Step 2: Change Tolerance
In Inspector, find `GestureRecognizerNew` component:
- Find: **Recognition Tolerance**
- Change from: `0.25` or `0.30`
- Change to: **`0.40`** ⭐

### Step 3: Test
- Press Play
- Draw shield again
- **IT SHOULD WORK NOW!** ✨

---

## Why This Works

**War of Wizards uses 0.40-0.45 tolerance**, not 0.25!

- **0.25** = Expert mode (too hard)
- **0.40** = Normal mode (just right) ⭐
- **0.55+** = Easy mode (too easy)

---

## Optional: Runtime Tuning

Want to adjust tolerance while playing?

1. Add `GestureRecognitionTuner.cs` to `/GestureManager`
2. In Play Mode:
   - Press **[=]** to increase tolerance
   - Press **[-]** to decrease tolerance
   - Press **[R]** to reset to 0.40
   - Press **[H]** to toggle help

---

## Expected Console Output (When Working)

```
━━━ GESTURE ANALYSIS ━━━
Points: 87 | Path Length: 450.2 | Speed: 892.3

[shield] Score: 0.3214 ✓ PASS ← SHOULD SEE THIS!
[fireball] Score: 0.6542 ✗ FAIL  
[lightning] Score: 0.7123 ✗ FAIL

✓✓✓ SPELL RECOGNIZED: shield ✓✓✓
```

---

## Still Not Working?

### If shield score is 0.40-0.50:
- Increase tolerance to **0.45** or **0.50**

### If shield score is above 0.50:
- Template may need regeneration
- But try **0.55** tolerance first

### If no console output at all:
- Check that `GestureDrawingManager` → `Use New Recognizer` is **TRUE**
- Check that `GestureRecognizerNew` component has spells assigned

---

## Pro Tips

1. **For Shield**: Use tolerance **0.40-0.45** (closed shape needs more tolerance)
2. **For Fireball**: Use tolerance **0.35-0.40** (complex shape)
3. **For Lightning**: Use tolerance **0.30-0.35** (simple zig-zag)

4. **Each spell can have its own tolerance!**
   - Edit the spell asset directly
   - Set per-spell tolerance separately

---

## Summary

**TL;DR: Change `Recognition Tolerance` from 0.25 → 0.40 and your shield will work!**

Read `/Assets/Scripts/GESTURE_RECOGNITION_99PERCENT_GUIDE.md` for full details.
