# ArkLib

As a dependency that make others mod can modify gdata.json



## How to use?

If you are a player, put the ArkLib.dll in plugins folder (should be `Chrono Ark\x64\Master\BepInEx\plugins`) , other mods using this mod will contain an ArkLib folder, put it in plugins folder and replace it.

If you are a modder:

1. Create a folder like this:

   - YourMod.dll

   - arklib_config.json

     the json like this:

     ```json
     {
       "Modname": "Yourmodname",
       "UseArkLib": true
     }     

   Put ArkLib.dll and ArklibAPI.dll in plugins folder
   Then start the game once

2. You can find the folder like that:

   - Yourmodname-ArkLib
   - YourMod.dll
   - arklib_config.json

   and the Yourmodname-ArkLib like that:

   - Additions
   - Replacements
   - Statchange

3. If you want to add new json to gdata.json, create a new json file in "Additions" folder, the format of the json file should be like that:

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
4. If your mod have "StatChange.txt", put it in `Statchange` folder
    About StatChange.txt, check `Chrono Ark\ChronoArk_Data\StreamingAssets\Mod`
5. When edit is completed, open `arklib_config.json, ` change "UseArkLib" to `true` to enable modify gdata function
6. When the mod needs to be released, it should be that:
    - Yourmodname-ArkLib
      - Additions
      - Replacements
      - Statchange
    - YourMod.dll
    - arklib_config.json (correct format)
    - icon.png
    - manifest.json
    - README.md

7. To ensure the correct dependency of mod, you can add ```[BepInDependency("com.DRainw.ChArkMod.ArkLib")]``` in your mod.dll, it is not necessary to reference ArkLib.dll in project reference.





1.1.2: Bugs fixed

2.0.0: Rewrite the folder struct



---
[*Manual install instructions*](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins#installation)  
**Manual install is the only option for 32-bit version**
