# Ruinfall Modding Framework - Developer Guide

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Launcher Development Guide](#launcher-development-guide)
- [Mod Development Guide](#mod-development-guide)

## Overview

The Ruinfall Modding Framework is a feature-complete game modding framework with the following core features:

- **Core Mod Concept**: The game itself functions as a core mod
- **Dependency Management**: Mod dependency system with version constraints
- **Resource Management**: Namespaced resource system based on `Identifier`
- **Event System**: High-performance event bus with priority and async support
- **Deferred Registration**: Solves initialization issues with cross-mod references
- **Unified Resource Location**: URL system is integrated with identifiers

## Quick Start

### Requirements

- .NET 9.0 or higher
- Supported OS: Windows, Linux (Theoretically), macOS (Theoretically)

### Basic Directory Structure

```
root
├─ Launcher.exe
└─ run/
    |
    ├─ log/
    └─ mod/
        └─ mod1/
            ├─ config.json
            ├─ mod.json
            ├─ texture/
            ├─ build/
            └─ ...
        - mod2/
            └─ ...
```

## Launcher Development Guide

### Write a Launcher

```csharp
namespace YourGame;

public static class Program {
    public static void Main(string[] args) {
        // 1. Start the framework
        MinoFramework.Start(args);
        
        // 2. Load all mods (from 'mod' directory)
        Mod.LoadDirectory(Url.Local("mod"));
        
        // 3. Freeze mod loading, perform dependency checking and initialization
        Mod.Freeze();
        
        // 4. Configure log output
        Log.Instance.OutputTo(Url.Local("log/latest.log"));
    }
}
```

You may want to create an AssetLoader and spread it to all mods.

```csharp
// Setting up an asset loader
var loader = new AssetLoader("modloader");

// Add processors for different file types
loader.AddProcessor(
    url => url.Path.EndsWith(".png"),
    (id, url) => {
        var image = Image.Parse(url.Read());
        var texture = RenderSystem.Create<Texture>(
            TextureDesc.CreateByImage(image));
        Assets.Set(id, texture);
    }
);

loader.AddProcessor(
    url => url.Path.EndsWith(".ttf"),
    (id, url) => {
        var font = Font.Load(url);
        font.SetResolution(64);
        Assets.Set(id, font);
    }
);

// ... other processors

// Publish to all mods.
EventBus.Instance.Post(new LoadEvent(loader));
```

## Mod Development Guide

### Basic Mod Structure

```
yourmod/
 ├─ config.json  // config of your mod. Completely custom.
 ├─ mod.json     // mod info. The structure is as follows.
 ├─ content/     // mod content. For example, an identifier "yourmod:font.ttf" refers to content/font.ttf.
 ├─ build/       // mod build dir. Your mod program DLL goes to here.
 └─ ...
```

### mod.json Configuration

```json
{
  "info": {
    "mod_id": "yourmod",
    "version": "1.0.0",
    "authors": "Your Name",
    "displayed_name": "Your Mod Name",
    "description": "Description of your mod"
  },
  "program": {
    "is_core_mod": false,
    "location": "build/YourMod.dll",
    "entrypoint": "YourMod.Main"
  },
  "dependencies": [
    {
      "mod_id": "dep1",
      "min_version": "1.0.0",
      "max_version": "2.0.0"
    },
    {
      "mod_id": "dep2",
      "min_version": "0.5.0"
    }
  ]
}
```

### Creating a Mod Class

```csharp
namespace YourMod;

public sealed class Main : Mod {
    // Define your mod's identifier scope
    public static readonly Identifier.ScopeRoot Root = 
        new Identifier.ScopeRoot("yourmod");
    
    // Define persistent data
    public static readonly PersistentData<int> SomeValue = 
        new PersistentData<int>(Root.Of("$a.b.c"), 42);
    
    public override void Initialize() {
        Log.Info($"Initializing {Info.DisplayedName} v{Info.Version}");
        
        // Your initialization code here
        RegisterContent();
    }
    
    private void RegisterContent() {
        // Register your mod's content
        var registry = new DeferredRegistry<YourItem>(Root);
        
        registry.Register(
            Root.Of("special_item"),
            () => new YourItem { /* properties */ }
        );
    }
    
    [SubscribeEvent]
    private static void OnUpdate(UpdateEvent e) {
        Log.Info("Game updated!");
    }
    
    [SubscribeEvent]
    private static void OnLoad(LoadEvent e) {
        Log.Info("Mod load!");
        AssetLoader subLoader = e.DominantLoader.CopyWithProcessors(Root);
        subLoader.Scan(Root.Of(""))
        e.DominantLoader.Enqueue(subLoader)
    }
}
```

### Identifier System

The `Identifier` system provides namespaced resource keys to avoid conflicts between mods.

```csharp
// Creating identifiers
var scope = new Identifier.ScopeRoot("yourmod");
var textureId = scope.Of("textures/player");  // "yourmod:textures/player" = "file://...mod/yourmod/textures/player"
var soundId = scope.Of("sounds/explosion");   // "yourmod:sounds/explosion" = "file://...mod/yourmod/sounds/explosion"

// Parsing identifiers
Identifier id = "othermod:some/resource";
string scope = id.Scope;  // "othermod"
string key = id.Key;      // "some/resource"

// Converting to URL (requires mod to be loaded)
Url resourceUrl = id.ToUrl();  // file://.../mod/othermod/some/resource

// Fallback to default scope
Identifier withFallback = Identifier.Fallback("defaultmod", "item");
// If "item" contains ':', uses that, otherwise "defaultmod:item"
```

### Deferred Registry

The deferred registry system solves initialization ordering problems.

```csharp
// Define a registerable type
public class MyItem : Registerable {
    public Identifier Id { get; set; }
    public int IntId { get; set; }
    public string Name { get; set; }
    public float Value { get; set; }
}

// Create registry
var registry = new DeferredRegistry<MyItem>("yourmod");

// Register items (these won't be created yet)
DeferredEntry<MyItem> swordEntry = registry.Register(
    "weapons/sword",  // Will be prefixed with "yourmod:"
    () => new MyItem { 
        Name = "Legendary Sword",
        Value = 100 
    }
);

DeferredEntry<MyItem> potionEntry = registry.Register(
    "items/potion",
    () => new MyItem {
        Name = "Health Potion",
        Value = 50
    }
);

// Items aren't created yet
Debug.Assert(!swordEntry.HasValue);

// Freeze the registry (creates all items)
registry.Freeze();

// Now items are available
MyItem sword = swordEntry;  // Implicit conversion
MyItem potion = potionEntry.Value;

// Access by identifier or index
MyItem item1 = registry["yourmod:weapons/sword"];
MyItem item2 = registry[0];  // By registration order
```

### URL System

The unified URL system handles multiple resource types.

```csharp
// Create URLs
Url fileUrl = Url.Local("mod/yourmod/texture/a.png");
Url httpUrl = new Url("https://example.com/data.json");
Url consoleUrl = new Url("console://out");  // Standard output

// Read from URL
ByteBuffer data = fileUrl.Read();  // Synchronous
ByteBuffer asyncData = await fileUrl.ReadAsync();  // Asynchronous

// Write to URL
ByteBuffer buffer = new ByteBuffer(Encoding.UTF8.GetBytes("Hello"));
fileUrl.Write(buffer);
await fileUrl.WriteAsync(buffer);

// URL operations
Url parent = ~fileUrl;  // Get parent directory
Url child = fileUrl / "subfolder" / "file.txt";  // Combine paths

// Get relative path
Url baseUrl = Url.Local("mod");
Url fullUrl = Url.Local("mod/yourmod/content/file.txt");
Url relative = Url.GetRelativeName(baseUrl, fullUrl);
// relative.Path == "yourmod/content/file.txt"
```

## Distribution Checklist

- [ ] Valid `mod.json` with correct metadata
- [ ] All dependencies listed
- [ ] Tested with target game version
- [ ] Documentation included
- [ ] Sample configuration if applicable
- [ ] License file included

---

**Happy modding!**