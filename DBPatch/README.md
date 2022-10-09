# DBPatch

Rewrite LanguageDB file to support localization of mod



## How to use

For players:

Unzip zip file, put the `TeamCrop-DBPatch-version` folder in plugins folder

Or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) installation is recommended

For modder: 

1. Create a folder like that: (at `plugins`)
- YourModname
	- YourMod.dll
  - arklib_config.json
	The json like that:
```json
{
  "Modname": "YourModname",
  "UseDBPatch": true
}
```
Change `"UseDBPatch"` to true

2. Run the game once

3. Create a csv file named one of the following four:

   - LangDataDB.csv
   - LangDialogueDB.csv
   - LangRecordsDB.csv
   - LangSystemDB.csv

   Depends on the target who you want to modify, check the DB file in 
   `Chrono Ark\ChronoArk_Data\StreamingAssets`

   put the new file in `YourModname-DBPatch`

4. (CAUTION!) The csv file format must be correct! 

   First, the format must be consistent with the game original file , then, the file must like that:

   `start`

   ```csv
   Skill/S_Azar_11_Name,Text,,빛나는 검기,Shining Aura,光る剣気,光辉剑气,光輝劍氣
   Skill/S_Azar_12_Name,Text,,이기어검,Dancing Sword,飛剣術,以气御剑,以氣御劍
   ...
   ...
   Skill/S_Azar_13_Description,Text,,환영검을 2개를 손으로 가져옵니다. ,Add 2 Illusion Swords to your hand.,幻影の刃を2枚手札に加える。,将 2 个幻影剑拿到手中。,將 2 個幻影劍拿到手中。
   Skill/S_Azar_13_Name,Text,,환영검 생성,Illusion Sword's Calling,幻影の刃召喚,生成幻影剑,生成幻影劍
   
   ```

   `end`

   Does not include the first row that defines each column's language type:

   `Key,Type,Desc,*Korean,*English,Japanese,Chinese,Chinese-TW [zh-tw]`

   This mod cannot add a new language.

   A blank line must be left on the last line, that is related to whether the next mod can be read correctly.

   If the modified entry is repeated with the original entry, the modified entry will be read and the original entry will be ignored.

   Recommended to use [Visual Studio Code](https://code.visualstudio.com/download) to modify csv files.

5. When the mod need release, the zip should contains:

- YourModname-DBPatch folder
  - (LangDBfile.csv)
  - (LangDBfile.csv)
  - ...
  
- arklib_config.json

- ...

  Make sure run the game once before upload. 





---
[*Manual install instructions*](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins#installation)  
**Manual install is the only option for 32-bit version**