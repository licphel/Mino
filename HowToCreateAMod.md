# How to Create a Mod

We will show this on JetBrains Rider since for convenience.

## I. New solution

Make a C# solution with ```.net>=9.0```.

## II. Clone Mino

Open the terminal of your solution, run shell

```bash
git clone https://github.com/licphel/Mino.git
```

## III. RMLWrapper

A RMLWrapper automates your source-to-mod procedure. You needn't build or publish
your mod, copy DLLs to the target directory.

Just to launch the RMLWrapper will build your source as a mod.

### Create a project

Console Application or Windows Application projects are both OK.
This project is expected to have a Main function.

You can get a template RMLWrapper at ```Mino/examples/ExampleRMLWrapper```.
And remember to replace the paths in the template to your own ones.

## IV. Your Mod Project

Finally, we create a Class Library project, which is actually your mod.

Assume that your mod project is simply named as ```Mod```, we need to put
```Mod/mod/mod.json```, which conforms to the form of ```Mino/examples/ExampleMod/mod/mod.json```.

- If you want to let your mod directly a game, set ```"is_core_mod" : true```.
- If you have no dependencies like other library mods, keep ```"dependencies" : []```.
- If you just want to override some assets, set ```"has_program": false```.
- Ensure ```"location"``` and ```"entrypoint"``` correct. They are essential for mod loading.

## V. Launch

Now launch the RMLWrapper project with args ```--indev --debug --noexcept```

You are expected to see log outputs like:

```
[...] [Main/Info] Possible mod detected: file://.../Mod/mod
[...] [Main/Info] Mod '<your_mod_id>' successfully loaded. All subscribed
[...] [Main/Debug] Lazy initializing mod '<your_mod_id>'...
```

That's it.