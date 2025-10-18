# Phase 1: Quick Setup Checklist

## ✅ Scripts Created (Complete!)
- [x] GesturePoint.cs
- [x] RunePadController.cs
- [x] GestureInputManager.cs
- [x] LineDrawer.cs

---

## 🎬 STEP-BY-STEP SETUP (Do in Order!)

### 1️⃣ Configure Input System (5 min)

1. **Open** `InputSystem_Actions.inputactions` (double-click in Project window)
2. **Create Action Map:**
   - Click "+" next to "Action Maps"
   - Name it: `Gesture`
3. **Create Actions** (in Gesture map):
   
   **Action: Touch**
   - Click "+" next to Actions
   - Name: `Touch`
   - Action Type: `Button`
   - Click "+" next to Touch → Add Binding
   - Path: `<Touchscreen>/primaryTouch/press`
   
   **Action: Position**
   - Click "+" next to Actions
   - Name: `Position`
   - Action Type: `Value`
   - Control Type: `Vector 2`
   - Click "+" next to Position → Add Binding
   - Path: `<Touchscreen>/primaryTouch/position`
   
   **Action: MouseClick**
   - Click "+" next to Actions
   - Name: `MouseClick`
   - Action Type: `Button`
   - Click "+" next to MouseClick → Add Binding
   - Path: `<Mouse>/leftButton`
   
   **Action: MousePosition**
   - Click "+" next to Actions
   - Name: `MousePosition`
   - Action Type: `Value`
   - Control Type: `Vector 2`
   - Click "+" next to MousePosition → Add Binding
   - Path: `<Mouse>/position`

4. **Save Asset** (Ctrl+S)
5. **Check "Generate C# Class"** (top of inspector)
6. **Click "Apply"** button

---

### 2️⃣ Create Line Trail Material (3 min)

1. **In Project window:** Right-click `/Assets/Materials` → Create → Material
2. **Name it:** `LineTrailMaterial`
3. **In Inspector, set:**
   - Shader: `Universal Render Pipeline/Particles/Unlit`
   - Surface Type: `Transparent`
   - Base Map: Leave empty
   - Base Color: `Cyan` (R:0, G:1, B:1, A:1)
   - Emission: ✅ Check "Emission"
   - Emission Color: `Cyan` (R:0, G:1, B:1)
   - Emission Intensity: `2`

---

### 3️⃣ Setup Hierarchy (7 min)

**In Hierarchy window:**

1. **Rename existing Image:**
   - Select `/GestureSetUp/Image`
   - Rename to: `RunePad`

2. **Configure RunePad:**
   - Select `RunePad`
   - **RectTransform:**
     - Click Anchor Preset (top-left of RectTransform)
     - Hold ALT+SHIFT, click Bottom-Center
     - Pos X: `0`
     - Pos Y: `200`
     - Width: `800`
     - Height: `400`
   - **Image Component:**
     - Color: R:`0.1`, G:`0.1`, B:`0.2`, A:`0.3`
   - **Add Component:** `RunePadController` script

3. **Create LineContainer:**
   - Right-click `RunePad` → Create Empty
   - Name: `LineContainer`
   - **RectTransform:**
     - Anchors: Stretch-Stretch (bottom-right preset)
     - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
     - Pos Z: `0`

4. **Create InputManager:**
   - Right-click in Hierarchy (root) → Create Empty
   - Name: `InputManager`
   - **Add Component:** `GestureInputManager` script
   - **Add Component:** `LineDrawer` script

---

### 4️⃣ Connect References (5 min)

**Select `InputManager`:**

1. **GestureInputManager Component:**
   - Run Pad: Drag `RunePad` here
   - Line Drawer: Drag `InputManager` itself (it has LineDrawer component)
   - Min Distance Between Points: `5`

2. **LineDrawer Component:**
   - Line Container: Drag `RunePad/LineContainer` here
   - Line Trail Material: Drag `LineTrailMaterial` here
   - Line Width: `10`
   - Line Color: Cyan (R:0, G:1, B:1, A:1)
   - Fade Out Duration: `0.3`
   - Min Points For Line: `2`

---

### 5️⃣ Verify Input System Settings (2 min)

1. **Edit → Project Settings → Player**
2. **Active Input Handling:** Set to `Input System Package (New)` or `Both`
3. **Close Project Settings**

---

### 6️⃣ Test! (3 min)

1. **Click Play ▶️**
2. **Click and drag** in the blue RunePad area
3. **Expected Result:**
   - Glowing cyan line follows your mouse/touch
   - Line fades out after releasing
   - Line only appears inside RunePad area

4. **Test in Device Simulator:**
   - Open Window → General → Device Simulator
   - Select a phone device
   - Test touch input

---

## 🐛 TROUBLESHOOTING

### Line doesn't appear:
- Check LineDrawer has LineTrailMaterial assigned
- Check Material shader is Particles/Unlit
- Verify LineContainer is child of RunePad

### Input not working:
- Ensure InputSystem_Actions has "Generate C# Class" checked
- Check Project Settings → Player → Active Input Handling
- Regenerate Input Actions: Click "Apply" again

### Console errors about InputSystem_Actions:
- The C# class needs to be generated
- Open InputSystem_Actions, check "Generate C# Class", click "Apply"

---

## 📸 WHAT IT SHOULD LOOK LIKE

```
Hierarchy:
└── GestureSetUp (Canvas)
    └── RunePad (Image + RunePadController)
        └── LineContainer (Empty RectTransform)
            └── (Lines spawn here at runtime)

Root Level:
└── InputManager (GestureInputManager + LineDrawer)
```

**Visual:**
- Blue semi-transparent panel at bottom-center
- When you draw: bright glowing cyan line
- Line fades smoothly when you release

---

## ✅ COMPLETION CRITERIA

- [ ] You can draw in RunePad area
- [ ] Line is glowing cyan color
- [ ] Line follows your input smoothly
- [ ] Line fades after release
- [ ] Drawing outside RunePad does nothing
- [ ] No console errors

**When all checked, Phase 1 is COMPLETE! 🎉**

---

## 📝 YOUR NOTES

```
Date: ___________
What worked:


What didn't work:


Questions:

```

