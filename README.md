# FO4Patchers

A collection of various Synthesis patchers for FO4. Currently a very lonely collection.

## HO3 Patcher

[Mod Page](https://www.nexusmods.com/fallout4/mods/82318?tab=description). This is what this patch is for. Please read the page to familiarise yourself with the mod and its properties.

### Manual Patching

Currently, this is not very practical. Use this if you want to generate a patch file for a mod and post it somewhere.

 1. Select the mods you want to patch. If you leave this field empty, the patcher will do your entire modlist.
 1. Add one or more `Rules`. The Rules with the lowest `Priority` will be applied first; in other words, the more important the `Rule`, the higher you should set the `Priority`.
 1. Select the `Armor` records to patch using the various options. Most of them use [Regex](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) - it's very flexible and I highly recommend learning it.
	1. Of note is the `Nif Regex` field. This is the one you'd want to use if converting from old HHS. It searches through all `Armor` records for those that use any matching `.nif` files and patches them. You can match an exact filename with the Regex `^FILENAME$`; for example, `^randomheel$` would only match `randomheel.nif`.
 1. Set the HHS height as you would normally
 1. Set the Ground Clip Allowance as you would normally. This is a new feature of HO3: it's how much the framework will allow the shoe to sink into the ground during animations before resorting to shrinking and so on. Useful on platform shoes.

### Automatic Patching

If you're an end user, this section will probably be the most applicable to you.

The patcher will parse the game's folder structure for existing HHS settings files (i.e. a `.txt` with `Height=[something]`) and apply them to the patch. It'll then move all of these files into a new `HeelsBackup` directory in the game folder, preserving all folder structure. If you need the old HHS files back, simply select everything in `HeelsBackup` and drag it back to the game folder. If you're struggling to find `HeelsBackup` after running this patch, open the `Data` tab on the right half of Mod Organiser 2, find the moved `.txt` files there, and right-click to open them in Explorer.