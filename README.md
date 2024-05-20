# FO4Patchers

## HO3 Patcher

[Mod Page](https://www.nexusmods.com/fallout4/mods/82318?tab=description). This is what this patch is for. Please read the page to familiarise yourself with the mod and its properties.

Here's how to use this patcher:

 1. Select the mods you want to patch. If you leave this field empty, the patcher will do your entire modlist.
 1. Add one or more `Rules`. The Rules with the lowest `Priority` will be applied first; in other words, the more important the `Rule`, the higher you should set the `Priority`.
 1. Select the `Armor` records to patch using the various options. Most of them use [Regex](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) - it's very flexible and I highly recommend learning it.
	1. Of note is the `Nif Regex` field. This is the one you'd want to use if converting from old HHS. It searches through all `Armor` records for those that use any matching `.nif` files and patches them. You can match an exact filename with the Regex `^FILENAME$`; for example, `^randomheel$` would only match `randomheel.nif`.
 1. Set the HHS height as you would normally
 1. Set the Ground Clip Allowance as you would normally. This is a new feature of HO3: it's how much the framework will allow the shoe to sink into the ground during animations before resorting to shrinking and so on. Useful on platform shoes.