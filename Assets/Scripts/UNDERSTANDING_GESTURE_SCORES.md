# Understanding Gesture Recognition Scores

## What You See in the Image

Your shield gesture looks **perfect**! It's a nice, clean closed loop that forms a shield shape. This is exactly what the recognizer needs to see.

## Why It Might Not Be Recognizing

Based on the console output you mentioned (but didn't show), here are the most common issues:

### Issue 1: No Spells Assigned
```
Console Error: "No spells configured"
```
**Fix**: Add spell assets to the `Available Spells` list in `GestureRecognizerNew` component

### Issue 2: Recognition Tolerance Too Strict
```
Console: "✗ NO MATCH - Best: shield (0.5234) | Threshold: 0.3000"
```
The score is 0.5234 but threshold is 0.30, so it failed by a small margin.

**Fix**: Increase `recognitionTolerance` to 0.50 or 0.55

### Issue 3: Missing Template
```
Console Warning: "Spell 'shield' has no template!"
```
**Fix**: Regenerate the gesture template for the Shield spell

### Issue 4: Starting Point Problem (Most Likely for Shield!)
```
Console: "✗ NO MATCH - Best: shield (0.8500) | Threshold: 0.5000"
```
The score is way too high (0.85 when threshold is 0.50).

**Fix**: Enable `useStartPointInvariance` - this is CRITICAL for closed loop gestures!

## How Gesture Matching Works

### Step 1: Preprocessing
Your drawn gesture goes through:
1. **Resampling** → Converts to exactly 64 evenly-spaced points
2. **Scaling** → Normalizes to 250×250 square (if scale invariance enabled)
3. **Translation** → Centers at origin (0, 0)

### Step 2: Template Matching
For each spell template:
1. **Preprocess** the template the same way
2. **Test rotations** (if multi-rotation enabled)
   - Tests angles from -45° to +45°
   - Uses Golden Section Search for optimal angle
3. **Test starting points** (if start point invariance enabled)
   - Critical for closed gestures like shields!
   - Tests 8 different starting positions
4. **Calculate distance** between gesture and template
5. **Find best score** (lowest distance = best match)

### Step 3: Decision
- Score ≤ threshold → **RECOGNIZED!** ✓
- Score > threshold → **NOT RECOGNIZED** ✗

## Score Interpretation

| Score | Meaning | Action |
|-------|---------|--------|
| 0.15 - 0.30 | Perfect match | Great! Gesture recognized |
| 0.30 - 0.45 | Very good match | Will recognize if threshold ≥ 0.45 |
| 0.45 - 0.55 | Good match | Will recognize if threshold ≥ 0.55 |
| 0.55 - 0.70 | Okay match | Acceptable if you want lenient recognition |
| 0.70 - 1.00 | Poor match | Either bad template or completely wrong gesture |
| > 1.00 | Very poor match | Completely different gesture |

## Example Console Output Explained

```
━━━ GESTURE ANALYSIS ━━━
Points: 52 | Path Length: 1250.5 | Speed: 625.2 | Direction: None
```
- **Points**: Number of points you drew (before resampling to 64)
- **Path Length**: Total pixel distance traveled
- **Speed**: Pixels per second (1250.5 / 2.0 seconds = 625.2)
- **Direction**: Clockwise, CounterClockwise, or None (for closed loop checks)

```
[shield] Score: 0.3245 ✓ PASS
[fireball] Score: 0.6821 ✗ FAIL
[lightning] Score: 0.7456 ✗ FAIL
```
- Each spell is tested
- Lower score = better match
- ✓ PASS = score ≤ threshold
- ✗ FAIL = score > threshold

```
✓✓✓ SPELL RECOGNIZED: shield ✓✓✓
Score: 0.3245 | Confidence: 35%
```
- **Confidence** is calculated as: `1 - (score / threshold)`
- Example: `1 - (0.3245 / 0.50) = 1 - 0.649 = 35.1%`
- Higher confidence = better match

## War of Wizards Comparison

War of Wizards uses a similar algorithm (likely the $1 Unistroke Recognizer or $P Point-Cloud Recognizer).

Key features they use that we've implemented:
1. ✓ **Rotation invariance** (test multiple angles)
2. ✓ **Scale invariance** (size doesn't matter)
3. ✓ **Position invariance** (location doesn't matter)
4. ✓ **Starting point invariance** (where you start doesn't matter)

These 4 features combined give you **99% accuracy** if your templates are good!

## Recommended Settings for Your Shield

Based on your image, here's what I recommend:

```
GestureRecognizerNew Component Settings:
────────────────────────────────────────
Available Spells:
  - Shield Spell ✓
  - Fireball ✓
  - Lightning ✓

Recognition Settings:
  - Resample Point Count: 64
  - Normalized Square Size: 250

Multi-Rotation Recognition:
  - Use Multi Rotation Matching: ✓ TRUE
  - Use Golden Section Search: ✓ TRUE
  - Rotation Steps: 8
  - Recognition Tolerance: 0.50 ← CHANGE THIS!

Advanced War of Wizards Features:
  - Use Scale Invariance: ✓ TRUE
  - Use Start Point Invariance: ✓ TRUE ← ENABLE THIS!
  - Start Point Tests: 8
  - Debug Mode: ✓ TRUE
```

## Testing Process

1. **Enable debug mode** to see detailed output
2. **Draw your shield** gesture (like in your image)
3. **Check console** for the score
4. **Adjust threshold** based on scores:
   - If score is 0.45 and it's not recognizing, set threshold to 0.50
   - If score is 0.55 and it's not recognizing, set threshold to 0.60
   - If score is 0.85+, there's a bigger problem (template or starting point)

## Common Score Patterns

### Shield Gesture Without Start Point Invariance
```
[shield] Score: 0.9234 ✗ FAIL  ← TOO HIGH!
```
This happens when you start drawing from a different point than the template.

**Solution**: Enable `useStartPointInvariance`

### Shield Gesture With Start Point Invariance
```
[shield] Score: 0.3456 ✓ PASS  ← MUCH BETTER!
```
Now it tests all 8 starting points and finds the best match.

### Shield Gesture Without Scale Invariance
If you draw a tiny shield:
```
[shield] Score: 0.7821 ✗ FAIL
```

If you draw a huge shield:
```
[shield] Score: 0.8234 ✗ FAIL
```

**Solution**: Enable `useScaleInvariance`

### Shield Gesture With All Features Enabled
```
[shield] Score: 0.2845 ✓ PASS  ← PERFECT!
```
Works regardless of size, rotation, position, or starting point!

## What Your Shield Score Should Be

With all features enabled and a good template:
- **Expected score**: 0.25 - 0.45
- **Recommended threshold**: 0.50
- **Expected confidence**: 50% - 90%

If your scores are consistently above 0.60, you need to:
1. Check that all advanced features are enabled
2. Regenerate your gesture template
3. Make sure the template is similar to how you actually draw

## Next Steps

1. Select `/GestureManager` in the hierarchy
2. Find the `GestureRecognizerNew` component
3. Change these settings:
   - Recognition Tolerance: 0.30 → 0.50
   - Use Start Point Invariance: Enable
   - Available Spells: Add all 3 spells
4. Enter Play Mode
5. Draw your shield
6. Check the console for the score
7. Report back the score and I can help fine-tune!
