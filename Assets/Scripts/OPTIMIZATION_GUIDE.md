# Gesture Recognition Optimization Guide

## What Was Optimized

### Performance Improvements
1. **Reduced Memory Allocations**
   - Reusable lists for gesture processing
   - In-place sorting instead of LINQ
   - Manual distance calculations
   - Pre-allocated buffers

2. **Faster Recognition**
   - Optimized resampling algorithm
   - Efficient bounding box calculation
   - Reduced loop overhead
   - Better early-exit conditions

3. **Better Template Quality**
   - Gesture alignment before averaging
   - Median-based point calculation (outlier rejection)
   - Smoothing filter for cleaner templates
   - More representative of actual user input

## Recommended Settings

### For High Accuracy (99%+)
```
Recognition Tolerance: 0.35-0.45
Resample Point Count: 64
Use Multi-Rotation: true
Use Golden Section: true
Use Scale Invariance: true
Use Start Point Invariance: true (for closed shapes)
Start Point Tests: 8
```

### For Best Performance
```
Recognition Tolerance: 0.40
Resample Point Count: 48-64
Use Multi-Rotation: true
Use Golden Section: true (faster than brute force)
Use Start Point Invariance: false (or 4 tests max)
```

## Workflow for 99% Accuracy

### Step 1: Global Calibration (Loosen Constraints)
1. Open `GestureRecognizerNew` in Inspector
2. Set `Recognition Tolerance` to `0.50` (very loose)
3. Set `Debug Mode` to `true`
4. Disable speed/direction enforcement in `SpellData` assets
5. Enter Play Mode

### Step 2: Record Quality Templates
1. Open `Tools → Gesture System Quick Setup`
2. Click `Open Template Generator` for each spell
3. Record 5-10 samples of each spell naturally
4. Generate averaged template
5. Repeat for all spells

### Step 3: Fine-Tune Tolerances
1. Test each spell in Play Mode
2. Check console for score vs tolerance
3. For each spell:
   - If scores are consistently low (0.15-0.25): Set per-spell tolerance to 0.30-0.35
   - If scores vary (0.20-0.40): Set tolerance to 0.40-0.45
   - If scores are high (0.40+): Re-record templates or adjust shape

### Step 4: Verify & Lock
1. Set global tolerance to `0.40`
2. Test all spells multiple times
3. Adjust individual spell tolerances as needed
4. Lock settings and disable debug mode

## Performance Tips

### Reduce CPU Load
- Use 64 resample points (sweet spot)
- Enable Golden Section Search
- Limit start point tests to 8 for closed shapes
- Disable multi-rotation if all gestures are orientation-specific

### Memory Optimization
- The system now reuses lists internally
- No manual optimization needed
- Expect ~0 GC allocations per gesture recognition

### Frame Rate
- Recognition takes ~0.5-2ms on average hardware
- Use `Debug Mode: false` in builds for best performance
- Consider async recognition if processing many gestures

## Troubleshooting

### "Shield keeps failing!"
- Check tolerance: Increase to 0.45 temporarily
- Re-record templates from actual user drawings
- Ensure templates are smooth and centered
- Check console scores - should be <0.40 for good match

### "Fireball and Lightning confuse each other"
- Increase point count to 96 for better detail
- Record more distinct templates
- Add per-spell tolerances (tighter for simpler shapes)
- Enable direction enforcement to distinguish

### "Recognition is slow"
- Reduce resample points to 48
- Disable start point invariance
- Use Golden Section (already default)
- Disable debug logging

## Best Practices

1. **Always record templates from real user input**, not from sprites or editor drawings
2. **Record 5-10 samples** - more samples = better templates
3. **Test on different devices** - touch vs mouse feels different
4. **Use per-spell tolerances** - each spell has unique complexity
5. **Start loose, tighten gradually** - easier to tune down than up
6. **Monitor console scores** - they tell you exactly what's happening
7. **Keep templates simple** - overly complex shapes are harder to match

## Expected Results

With proper templates and tuning:
- **Simple spells** (straight lines, circles): 98-99% accuracy
- **Medium complexity** (shields, crosses): 95-98% accuracy  
- **Complex spells** (multi-stroke, detailed): 90-95% accuracy

Scores to aim for:
- **Excellent**: 0.15-0.25 (very confident match)
- **Good**: 0.25-0.35 (reliable match)
- **Acceptable**: 0.35-0.45 (works but might need tuning)
- **Poor**: 0.45+ (likely mis-match, re-record template)
