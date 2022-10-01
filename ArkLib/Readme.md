# ArkLib

As a dependency that make others mod can modify gdata.json



## How to use?

If you are a player, put the ArkLib.dll in plugins folder (should be `Chrono Ark\x64\Master\BepInEx\plugins`) , other mods using this mod will contain an ArkLib folder, put it in plugins folder and replace it.

If you are a modder:

1. Put the ArkLib.dll in plugins folder, and then run the game once, then you will see a folder named "Sample"  at ` Chrono Ark\x64\Master\BepInEx\plugins\ArkLib` (if the Sample folder already exists, ignore this step)

2. If you want to add new json to gdata.json, create a new json file in "Additions" folder, the format of the json file should be like that:

```json
{		//start
    "B_Sample": {
        "_gdeType_Debuff": "Bool",
        "Debuff": false,
        "_gdeType_Name": "String",
       	......
        //depends on what you want to add, refer to game original gdata.json
        //the path of the gdata.json is 
        //Chrono Ark\ChronoArk_Data\StreamingAssets\gdata.json
        ......
    },
    "S_Sample2": {
       	......
        //depends on what you want to add, refer to game original gdata.json
        ......
    },
	......
}		//end
```
The file name of this json file can be any, but internal json must be correct.

3. If you want to replace a json which in gdata.json, put new json file in "Replacements" folder
    This function is not recommended unless there are special requirements.

4. If your mod have "StatChange.txt", put it in `ArkLib\Sample\Additions\statchange`
    About StatChange.txt, check `Chrono Ark\ChronoArk_Data\StreamingAssets\Mod`

5. When edit is completed, open `ArkLib\Sample\config.json , `change "UseLib" to `true` to enable modify gdata function, change "Modname" to your mod's name (optional), don't change "Hash" key and its value, then rename the Sample folder to your mod's name.

6. The `ArkLib\_data` folder is the cache of the plug-in runtime. Do not modify or delete it. When mod is released, the "_data" folder should not be included.

7. When the mod needs to be released, it should be that:
In plugins folder: Mod.dll, ArkLib folder;
In ArkLib folder: Modname folder;
In Modname folder: include Additions and Replacements folders, include config.json and its format is correct.

8. To ensure the correct dependency of mod, you can add ```[BepInDependency("com.DRainw.ChArkMod.ArkLib")]``` in your mod.dll, it is not necessary to reference ArkLib.dll in project reference.
