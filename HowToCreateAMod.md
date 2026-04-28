# How to Create a Mod

We will show this on JetBrains Rider since for convenience.

## I. New solution

Make a C# solution with ```.net>=9.0```.

## II. Clone Mino

Open the terminal of your solution, run shell
```bash
git clone https://github.com/licphel/Mino.git
```

## III. Create Your Mod Project

Finally, we create a Console Application project, which is actually your mod.

In aspect of coding, see ```Mino/examples/ExampleMod/```.

Assume that your mod project is simply named as ```Mod```, we need to put
```Mod/mod/mod.json```, which conforms to the form of ```Mino/examples/ExampleMod/mod/mod.json```.

- If you want to let your mod directly a game, set ```"is_core_mod" : true```.
- If you have no dependencies like other library mods, keep ```"dependencies" : []```.
- If you just want to override some assets, set ```"has_program": false```.
- Ensure ```"location"``` and ```"entrypoint"``` correct. They are essential for mod loading.

## IV. Launch

Now launch the Mod project with args ```--indev --debug --noexcept```

You are expected to see log outputs like:
```
[...] [Main/Info] Possible mod detected: file://.../Mod/mod
[...] [Main/Info] Mod '<your_mod_id>' successfully loaded. All subscribed
[...] [Main/Debug] Lazy initializing mod '<your_mod_id>'...
```
That's it.