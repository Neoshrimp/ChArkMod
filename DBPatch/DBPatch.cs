using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using GameDataEditor;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using I2.Loc;
using ArklibAPI;
using ModF = ArklibAPI.ModtheFolder;
using BepInEx.Bootstrap;
using System.Collections;
using TileTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrolls;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;


namespace DBPatch
{
    [BepInPlugin(GUID, "DBPatch", version)]
    [BepInProcess("ChronoArk.exe")]
    [BepInDependency("com.DRainw.ChArkMod.ArkLib", "2.0.0")]
    public class DBPatch : BaseUnityPlugin
    {
        public const string GUID = "com.DRainw.ChArkMod.DBPatch";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);
        private static Dictionary<string, DirectoryInfo> DIR = new Dictionary<string, DirectoryInfo>();
        private static readonly string root = new FileInfo(Chainloader.PluginInfos[GUID].Location).Directory.FullName;
        private static readonly string path_0 = Application.streamingAssetsPath + "/LangDataDB.csv";
        private static readonly string path_1 = Application.streamingAssetsPath + "/LangDialogueDB.csv";
        private static readonly string path_2 = Application.streamingAssetsPath + "/LangRecordsDB.csv";
        private static readonly string path_3 = Application.streamingAssetsPath + "/LangSystemDB.csv";


        void Awake()
        {
            Logger.LogInfo("BepInex: DBPatch loading.");

            try
            {
                ModF API = new ModF();
                DIR = API.GetModRoots("DBPatch");
                flag0 = flag1 = flag2 = flag3 = false;
                CheckFolder();
                Merge(CheckHash());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            harmony.PatchAll();
        }

        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        static void CheckFolder()
        {
            Directory.CreateDirectory(root + "\\_data");
        }


        static bool CheckHash()
        {
            bool change = false;

            if (File.Exists(root + "\\_data\\_dbfiledata.json"))
            {
                //read json
                StreamReader reader = File.OpenText(root + "\\_data\\_dbfiledata.json");
                JsonTextReader jsonTextReader = new JsonTextReader(reader);
                JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                reader.Close();

                //检查源文件是否有更新
                int _Hash0 = new FileInfo(path_0).LastWriteTimeUtc.GetHashCode();
                if (config["Hash_Data"].ToString() != _Hash0.ToString())
                {
                    change = true;
                    config["Hash_Data"] = _Hash0;
                }
                int _Hash1 = new FileInfo(path_1).LastWriteTimeUtc.GetHashCode();
                if (config["Hash_Dialogue"].ToString() != _Hash1.ToString())
                {
                    change = true;
                    config["Hash_Dialogue"] = _Hash1;
                }
                int _Hash2 = new FileInfo(path_2).LastWriteTimeUtc.GetHashCode();
                if (config["Hash_Records"].ToString() != _Hash2.ToString())
                {
                    change = true;
                    config["Hash_Records"] = _Hash2;
                }
                int _Hash3 = new FileInfo(path_3).LastWriteTimeUtc.GetHashCode();
                if (config["Hash_System"].ToString() != _Hash3.ToString())
                {
                    change = true;
                    config["Hash_System"] = _Hash3;
                }

                if (DIR.Count > 0)
                {
                    //mod文件夹的改变
                    string totalmod = string.Empty;

                    //逐个mod读取
                    foreach (var dir in DIR)
                    {
                        string modname = dir.Key;
                        totalmod += modname;

                        if (config.ContainsKey(modname))
                        {
                            if (File.Exists(dir.Value.FullName + "\\LangDataDB.csv"))
                            {
                                flag0 = true;
                                int DataHah = FigureHash(new FileInfo(dir.Value.FullName + "\\LangDataDB.csv"));
                                if (config[modname]["Hash_Data"].ToString() != DataHah.ToString())
                                {
                                    change = true;
                                    config[modname]["Hash_Data"] = DataHah;
                                }
                            }
                            if (File.Exists(dir.Value.FullName + "\\LangDialogueDB.csv"))
                            {
                                flag1 = true;
                                int DialogueHash = FigureHash(new FileInfo(dir.Value.FullName + "\\LangDialogueDB.csv"));
                                if (config[modname]["Hash_Dialogue"].ToString() != DialogueHash.ToString())
                                {
                                    change = true;
                                    config[modname]["Hash_Dialogue"] = DialogueHash;
                                }
                            }
                            if (File.Exists(dir.Value.FullName + "\\LangRecordsDB.csv"))
                            {
                                flag2 = true;
                                int RecordsHash = FigureHash(new FileInfo(dir.Value.FullName + "\\LangRecordsDB.csv"));
                                if (config[modname]["Hash_Records"].ToString() != RecordsHash.ToString())
                                {
                                    change = true;
                                    config[modname]["Hash_Records"] = RecordsHash;
                                }
                            }
                            if (File.Exists(dir.Value.FullName + "\\LangSystemDB.csv"))
                            {
                                flag3 = true;
                                int SystemHash = FigureHash(new FileInfo(dir.Value.FullName + "\\LangSystemDB.csv"));
                                if (config[modname]["Hash_System"].ToString() != SystemHash.ToString())
                                {
                                    change = true;
                                    config[modname]["Hash_System"] = SystemHash;
                                }
                            }
                        }
                        else
                        {
                            Hashdata new_data = new Hashdata
                            {
                                Hash_Data = 0,
                                Hash_Dialogue = 0,
                                Hash_Records = 0,
                                Hash_System = 0
                            };
                            config.Add(modname, JObject.Parse(JsonConvert.SerializeObject(new_data)));
                            File.WriteAllText(root + "\\_data\\_dbfiledata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                            return CheckHash();
                        }
                    }

                    int total_hash = totalmod.GetHashCode();
                    if (config["Hash_modtotal"].ToString() != total_hash.ToString())
                    {
                        config["Hash_modtotal"] = total_hash;
                    }

                    if (change)
                    {
                        File.WriteAllText(root + "\\_data\\_dbfiledata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                    }
                }
            }
            else
            {
                NewHashdata new_data = new NewHashdata
                {
                    Hash_Data = 0,
                    Hash_Dialogue = 0,
                    Hash_Records = 0,
                    Hash_System = 0,
                    Hash_modtotal = 0
                };
                JObject config = JObject.Parse(JsonConvert.SerializeObject(new_data));
                File.WriteAllText(root + "\\_data\\_dbfiledata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                return CheckHash();
            }
            return change;
        }

        private static bool flag0;
        private static bool flag1;
        private static bool flag2;
        private static bool flag3;

        static void Merge(bool change)
        {
            if (!change)
                return;

            if (flag0)
            {
                string path0 = root + "\\_data\\LangDataDB.csv";
                File.Copy(path_0, path0, true);
                using (StreamReader sr = File.OpenText(path0))
                {
                    string ori = sr.ReadToEnd();
                    foreach (var dir in DIR)
                    {
                        if (!File.Exists(dir.Value.FullName + "\\LangDataDB.csv"))
                        {
                            continue;
                        }
                        using (StreamReader sr1 = File.OpenText(dir.Value.FullName + "\\LangDataDB.csv"))
                        {
                            string temp = sr1.ReadToEnd();
                            ori += temp;
                        }
                    }
                    sr.Close();
                    File.WriteAllText(path0, ori);
                }
            }
            if (flag1)
            {
                string path0 = root + "\\_data\\LangDialogueDB.csv";
                File.Copy(path_0, path0, true);
                using (StreamReader sr = File.OpenText(path0))
                {
                    string ori = sr.ReadToEnd();
                    foreach (var dir in DIR)
                    {
                        if (!File.Exists(dir.Value.FullName + "\\LangDialogueDB.csv"))
                            continue;
                        using (StreamReader sr1 = File.OpenText(dir.Value.FullName + "\\LangDialogueDB.csv"))
                        {
                            string temp = sr1.ReadToEnd();
                            ori += temp;
                        }
                    }
                    sr.Close();
                    File.WriteAllText(path0, ori);
                }
            }
            if (flag2)
            {
                string path0 = root + "\\_data\\LangRecordsDB.csv";
                File.Copy(path_0, path0, true);
                using (StreamReader sr = File.OpenText(path0))
                {
                    string ori = sr.ReadToEnd();
                    foreach (var dir in DIR)
                    {
                        if (File.Exists(dir.Value.FullName + "\\LangRecordsDB.csv"))
                            continue;
                            using (StreamReader sr1 = File.OpenText(dir.Value.FullName + "\\LangRecordsDB.csv"))
                        {
                            string temp = sr1.ReadToEnd();
                            ori += temp;
                        }
                    }
                    sr.Close();
                    File.WriteAllText(path0, ori);
                }
            }
            if (flag3)
            {
                string path0 = root + "\\_data\\LangSystemDB.csv";
                File.Copy(path_0, path0, true);
                using (StreamReader sr = File.OpenText(path0))
                {
                    string ori = sr.ReadToEnd();
                    foreach (var dir in DIR)
                    {
                        if (File.Exists(dir.Value.FullName + "\\LangSystemDB.csv"))
                            continue;
                            using (StreamReader sr1 = File.OpenText(dir.Value.FullName + "\\LangSystemDB.csv"))
                        {
                            string temp = sr1.ReadToEnd();
                            ori += temp;
                        }
                    }
                    sr.Close();
                    File.WriteAllText(path0, ori);
                }
            }

        }


        static int FigureHash(FileInfo file)
        {
            return file.LastWriteTimeUtc.ToString().GetHashCode();
        }

        [HarmonyPatch(typeof(LocalizeManager))]
        class ChangeDBpatch
        {
            [HarmonyPatch(nameof(LocalizeManager.Awake))]
            static bool Prefix(LocalizeManager __instance)
            {
                if (LocalizeManager.Ins == null)
                {
                    LocalizeManager.Ins = __instance;
                    LocalizeManager.MainFile = Resources.Load<LanguageSourceAsset>("LangSystemDB").SourceData;
                    LocalizeManager.DBFile = Resources.Load<LanguageSourceAsset>("LangDataDB").SourceData;
                    LocalizeManager.DialogueFile = Resources.Load<LanguageSourceAsset>("LangDialogueDB").SourceData;
                    LocalizeManager.RecordFile = Resources.Load<LanguageSourceAsset>("LangRecordsDB").SourceData;

                    string csvstring2;
                    if (flag0)
                        csvstring2 = LocalizationReader.ReadCSVfile(root + "\\_data\\LangDataDB.csv", Encoding.UTF8);
                    else
                        csvstring2 = LocalizationReader.ReadCSVfile(Application.dataPath + "/StreamingAssets/LangDataDB.csv", Encoding.UTF8);
                    LocalizeManager.DBFile.Import_CSV(string.Empty, csvstring2, eSpreadsheetUpdateMode.Replace, ',');

                    string csvstring3;
                    if (flag1)
                        csvstring3 = LocalizationReader.ReadCSVfile(root + "\\_data\\LangDialogueDB.csv", Encoding.UTF8);
                    else
                        csvstring3 = LocalizationReader.ReadCSVfile(Application.dataPath + "/StreamingAssets/LangDialogueDB.csv", Encoding.UTF8);
                    LocalizeManager.DialogueFile.Import_CSV(string.Empty, csvstring3, eSpreadsheetUpdateMode.Replace, ',');

                    string csvstring4;
                    if (flag2)
                        csvstring4 = LocalizationReader.ReadCSVfile(root + "\\_data\\LangRecordsDB.csv", Encoding.UTF8);
                    else
                        csvstring4 = LocalizationReader.ReadCSVfile(Application.dataPath + "/StreamingAssets/LangRecordsDB.csv", Encoding.UTF8);
                    LocalizeManager.RecordFile.Import_CSV(string.Empty, csvstring4, eSpreadsheetUpdateMode.Replace, ',');

                    string csvstring;
                    if (flag3)
                        csvstring = LocalizationReader.ReadCSVfile(root + "\\_data\\LangSystemDB.csv", Encoding.UTF8);
                    else
                        csvstring = LocalizationReader.ReadCSVfile(Application.dataPath + "/StreamingAssets/LangSystemDB.csv", Encoding.UTF8);
                    LocalizeManager.MainFile.Import_CSV(string.Empty, csvstring, eSpreadsheetUpdateMode.Merge, ',');


                    return false;
                }
                UnityEngine.Object.Destroy(__instance);

                return false;
            }
        }


    }

    public class NewHashdata
    {
        public int Hash_Data;
        public int Hash_Dialogue;
        public int Hash_Records;
        public int Hash_System;

        public int Hash_modtotal;
    }

    public class Hashdata
    {
        public int Hash_Data;
        public int Hash_Dialogue;
        public int Hash_Records;
        public int Hash_System;
    }
}
