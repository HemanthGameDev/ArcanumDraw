# PHASE 1: Foundation (Input & Drawing) - Implementation Guide

## 🎯 GOAL
Create the basic gesture drawing system with visual feedback.

---

## 📋 CHECKLIST

### Step 1: Rune Pad Setup ⬜
- [ ] Create RunePad UI panel with proper anchoring
- [ ] Apply semi-transparent background
- [ ] Add magical border sprite/effect
- [ ] Position at bottom-center of screen

### Step 2: Input System Configuration ⬜
- [ ] Configure Input Actions for touch/mouse
- [ ] Create InputManager script
- [ ] Test input detection in Rune Pad area

### Step 3: Line Renderer Setup ⬜
- [ ] Create LineDrawer script
- [ ] Implement real-time line drawing
- [ ] Add glowing material for trail
- [ ] Implement fade-out effect

### Step 4: Integration & Testing ⬜
- [ ] Connect InputManager to LineDrawer
- [ ] Test drawing gestures
- [ ] Optimize line rendering performance
- [ ] Test on mobile simulator

---

## 📦 DELIVERABLES

### Scripts Created:
1. `RunePadController.cs` - Manages the casting area
2. `GestureInputManager.cs` - Handles touch/mouse input
3. `LineDrawer.cs` - Renders the drawing trail
4. `GesturePoint.cs` - Data structure for gesture points

### UI Elements:
1. RunePad Panel (RectTransform)
2. RunePad Border Image
3. Line Container (for line renderers)

### Materials:
1. LineTrail Material (glowing effect)

---

## 🔧 IMPLEMENTATION DETAILS

### STEP 1: RUNE PAD SETUP

**What:** Create the interactive drawing zone

**How:**
1. In Hierarchy, right-click `GestureSetUp` → UI → Panel
2. Rename to "RunePad"
3. Set RectTransform:
   - Anchor Preset: Bottom-Center
   - Pos Y: 150
   - Width: 800
   - Height: 400
4. Set Image component:
   - Color: Semi-transparent (R:0.1, G:0.1, B:0.2, A:0.3)
5. Attach `RunePadController.cs` script

**Why:** This creates the defined area where players draw spells

---

### STEP 2: INPUT SYSTEM CONFIGURATION

**What:** Set up New Input System for touch and mouse

**How:**
1. Open `InputSystem_Actions.inputactions`
2. Add new Action Map: "Gesture"
3. Add Actions:
   - "Touch" (Button) → Binding: <Touchscreen>/primaryTouch/press
   - "Position" (Value) → Binding: <Touchscreen>/primaryTouch/position
   - "MouseClick" (Button) → Binding: <Mouse>/leftButton
   - "MousePosition" (Value) → Binding: <Mouse>/position
4. Save and generate C# class
5. Create GameObject "InputManager" in scene
6. Attach `GestureInputManager.cs` script

**Why:** New Input System provides unified input handling for mobile and desktop

---

### STEP 3: LINE RENDERER SETUP

**What:** Create visual trail that follows player's finger

**How:**
1. Create Material: `/Assets/Materials/LineTrailMaterial.mat`
   - Shader: Universal Render Pipeline/Particles/Unlit
   - Rendering Mode: Transparent
   - Base Color: Bright cyan (R:0, G:1, B:1, A:1)
   - Enable Emission
   - Emission Color: Bright cyan
2. Create empty GameObject under RunePad: "LineContainer"
3. Attach `LineDrawer.cs` to InputManager
4. Assign RunePad and Material references

**Why:** LineRenderer provides smooth, performant line drawing with visual effects

---

### STEP 4: INTEGRATION & TESTING

**What:** Connect all components and verify functionality

**How:**
1. Ensure all script references are assigned in Inspector
2. Enter Play Mode
3. Click/touch and drag in RunePad area
4. Verify glowing line appears and follows input
5. Test in Device Simulator (mobile view)
6. Check line fades after releasing

**Why:** Testing ensures all components work together correctly

---

## 🎨 VISUAL SPECIFICATIONS

### Line Trail:
- **Color:** Bright cyan (#00FFFF) with emission
- **Width:** 8-12 units
- **Fade Time:** 0.3 seconds after release
- **Material:** Additive/transparent for glow effect

### Rune Pad:
- **Size:** 800x400 (adjustable for screen sizes)
- **Color:** Dark blue-tinted semi-transparent
- **Position:** Bottom-center, 150 units from bottom
- **Border:** 4-6 pixel magical effect (optional for now)

---

## ⚠️ IMPORTANT NOTES

1. **Input System:** Ensure Input System package is set to "Both" or "Input System Package (New)" in Project Settings → Player → Active Input Handling
2. **Canvas:** Make sure Canvas is set to Screen Space - Overlay
3. **Event System:** Required for UI input (already in scene)
4. **Performance:** Line pooling will be added in later phases
5. **Mobile Testing:** Use Device Simulator window to test touch input

---

## 🚀 NEXT PHASE PREVIEW

Phase 2 will add:
- Gesture data structure (V-shape, Spiral, Circle, etc.)
- Pattern matching algorithm
- Recognition feedback visuals
- Perfect symbol snap effect

---

## 📝 NOTES SECTION
(Use this space to track issues, questions, or modifications)

```
Date: ___________
Issue/Note: 
Solution:

---

```

