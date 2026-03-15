## Example Mod

This is a demo project (remember! this is a C# project, just no .csproj file here)

This is optional, but for better dev experience, creating a wrapper is highly recommended.

Basic flow:
```
YourModProject /mod/
 ↑ build        ↑ load as valid mod
=== SourceMod  ===
 ↑ call         ↑ refer
=== RMLWrapper ===
 ↓ call
=== RMLCore    ===
```

**NOTE: Launching a RMLWrapper through IDE, you'd better add the '--indev' arg.
This will raise the run/ dir from exe-based to project-based, which is much more convenient.**
