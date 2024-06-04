# EFXManager

`EFXManager` is a Unity Editor tool designed to manage and preview effects (FX) prefabs in your project.

![EFXManager Window](/EFXManager.png)

## Features
1. **FX Prefab Manager**:
   - Add prefabs to the manager by dragging and dropping them into the separate FX Prefab Manager window.
   - The FX Prefab Manager window lists all the added prefabs.
   - A ScriptableObject holds the list of all prefabs in the assets and will be created at `/Assets/`. Then you can move it wherever you want.

2. **Preview Settings**:
   - **Preview Size Slider**: Adjusts the size of the prefab previews.
   - **Background Color Picker**: Changes the background color of the preview area.
   - **Hide Grid Button**: Toggles the visibility of the grid in the preview area.
   - **Trail Mode Settings**: Adjusts the speed and radius of the trail effect.

3. **Preview Controls**:
   - **Left Mouse Button (LMB)**: Plays the effect of a selected prefab.
   - **Middle Mouse Button (MMB)**: Rotates the view around the effect.
   - **Scroll Wheel**: Zooms in and out of the preview area.

4. **Scene Controls**:
   - **'Add to Scene' Button**: Adds the prefab to the scene at the origin.
   - **'Add to Selected' Button**: Adds the prefab to the currently selected object in the scene.

## Installation

1. Copy `EFXManager` folder into your Unity Editor folder.

## How to Use

1. Open the tool from the Menu under "Escripts > FXManager".
2. Use the EFX Prefab Manager window to add and organize your VFX prefabs.
3. Adjust the preview settings to customize the preview area to your liking.
4. Use the preview and scene controls to inspect and place FX prefabs in your project.
