# Chisel Mod Loader (CML)

A runtime modding and injection framework for games built on the Chisel/MonoGame engine. CML enables loading custom managed assemblies, patching engine routines via Harmony, and injecting modded entity types on startup.

---

## Repository Structure

The solution (`ChiselModLoader.slnx`) contains 4 projects:

- **ChiselModLoader.Injector.App**: The standalone host executable that resolves native libraries, reads configuration, loads runtime assemblies, and starts the game.
- **ChiselModLoader.Runtime**: The core runtime bootstrapper that applies Harmony engine patches and dynamically loads CML mod plugins.
- **ChiselModLoader.Shared**: Core data models, path helpers, and INI configuration parsers shared across CML modules.
- **ExampleMod**: A reference mod project demonstrating CML plugin attributes and custom entity creation.

---

## Setup & Building

### 1. Build the Solution
Open `ChiselModLoader.slnx` in Visual Studio and compile all 3 core projects along with the example mod under your desired build configuration.

### 2. Environment Setup
To run CML, you must set up your game files using one of the following methods:

- **Option A (Local Build Setup):** Copy your Chisel game files into the `/Game` directory located in the solution root.
- **Option B (Direct Game Installation):** Copy the contents of the compiled CML directory directly into your existing Chisel game installation folder.

---

## Debugging

To debug CML or mods directly within Visual Studio:

1. Ensure your game binaries and assets are present in the `/Game` root directory.
2. Set `ChiselModLoader.Injector.App` as your startup project in Visual Studio.
3. Switch your launch profile/configuration to `cml_injector_debug`.
4. Press **F5** to launch the injector with the Visual Studio debugger attached, allowing you to set breakpoints across CML modules and mod code.

---

## Installation & Folder Structure

When compiled and placed in your game directory, CML expects the following folder structure:

```text
Game/
├── ChiselModLoader.Injector.App.exe
├── ChiselModLoader.Runtime.dll
├── ChiselModLoader.Shared.dll
├── CMLInjector.cfg
└── CML/
    ├── CMLLibs/
    │   └── HarmonyLib.dll
    └── Mods/
        └── ExampleMod.dll
```

---

## Creating Mods

To create a mod for CML, reference `ChiselModLoader.Runtime.dll` and define a class inheriting from `CMLMod` marked with the `CMLPluginInfo` attribute. Beyond that, writing a mod works just like writing code inside the engine itself,including creating custom entities:

```csharp
using ChiselModLoader.Runtime;

[CMLPluginInfo("MyCustomMod", "1.0.0")]
public class MyMod : CMLMod
{
    public override void Preload()
    {
        logger.LogDebug("Mod loaded!");
    }

    public override void Update()
    {
        // Runs every frame
    }
}
```

Since custom assemblies are registered with the engine's entity compiler on startup, any custom entity classes declared in your mod assembly are processed seamlessly alongside core engine entities.

---

## Disclaimer & Non-Endorsement

Chisel Mod Loader (CML) is an independent, open-source project. It is not affiliated with, authorized, maintained, sponsored, or endorsed by the developer of chisel. All product names, logos, brands, trademarks, and registered trademarks belong to their respective owners.