using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System.Reflection.Emit;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using GameDataEditor;
using TileTypes;
using System.Reflection;
using I2.Loc;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using DarkTonic.MasterAudio;
using Scrolls;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using ArklibAPI;
using ModF = ArklibAPI.ModtheFolder;
using BepInEx.Bootstrap;

namespace ArkLib
{
    [BepInPlugin(GUID, "ArkLib", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ArkLib : BaseUnityPlugin
    {
        public const string GUID = "com.DRainw.ChArkMod.ArkLib";
        public const string version = "2.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);
        private static readonly string root = new FileInfo(Chainloader.PluginInfos[GUID].Location).Directory.FullName;
        private static readonly string path_g = Application.streamingAssetsPath + "/gdata.json";

        private static readonly List<string> Preprocessing = new List<string>
        {
            "Additions",
            "Replacements",
            "StatChange"
        };
        private static Dictionary<string, DirectoryInfo> DIR = new Dictionary<string, DirectoryInfo>();

        void Awake()
        {
            Logger.LogInfo("BepInex: ArkLib loading.");

            try
            {
                ModF API = new ModF(Preprocessing);
                DIR = API.GetModRoots("ArkLib");

                CheckFolder();
                //CheckRoot();
                Merge(CheckHash());

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            harmony.PatchAll();
        }

        void Start()
        {

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
            if (!File.Exists(root + "\\_data\\new_gdata.json"))
            {
                change = true;
            }
            if (File.Exists(root + "\\_data\\_jsondata.json"))
            {
                //read json
                StreamReader reader = File.OpenText(root + "\\_data\\_jsondata.json");
                JsonTextReader jsonTextReader = new JsonTextReader(reader);
                JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                reader.Close();

                //检查gdata是否有更新
                int _gdataHash = new FileInfo(path_g).LastWriteTimeUtc.GetHashCode();
                if (config["Hash_gdata"].ToString() != _gdataHash.ToString())
                {
                    change = true;
                    config["Hash_gdata"] = _gdataHash;
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
                            int Additions_Hash = FigureHash(new DirectoryInfo(dir.Value.FullName + "\\Additions"));
                            if (config[modname]["Additions_Hash"].ToString() != Additions_Hash.ToString())
                            {
                                change = true;
                                config[modname]["Additions_Hash"] = Additions_Hash;
                            }
                            int Replacements_Hash = FigureHash(new DirectoryInfo(dir.Value.FullName + "\\Replacements"));
                            if (config[modname]["Replacements_Hash"].ToString() != Replacements_Hash.ToString())
                            {
                                change = true;
                                config[modname]["Replacements_Hash"] = Replacements_Hash;
                            }
                            int StatChange_Hash = FigureHash(new DirectoryInfo(dir.Value.FullName + "\\StatChange"));
                            if (config[modname]["StatChange_Hash"].ToString() != StatChange_Hash.ToString())
                            {
                                change = true;
                                config[modname]["StatChange_Hash"] = StatChange_Hash;
                            }
                        }
                        else
                        {
                            Hashdata new_data = new Hashdata
                            {
                                Additions_Hash = 0,
                                Replacements_Hash = 0,
                                StatChange_Hash = 0
                            };
                            config.Add(modname, JObject.Parse(JsonConvert.SerializeObject(new_data)));
                            File.WriteAllText(root + "\\_data\\_jsondata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                            return CheckHash();
                        }
                    }

                    int total_hash = totalmod.GetHashCode();
                    if(config["Hash_modtotal"].ToString() != total_hash.ToString())
                    {
                        config["Hash_modtotal"] = total_hash;
                    }

                    if (change)
                    {
                        File.WriteAllText(root + "\\_data\\_jsondata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                    }
                }
            }
            else
            {
                NewHashdata new_data = new NewHashdata
                {
                    Hash_gdata = 0,
                    Hash_modtotal = 0
                };
                JObject config = JObject.Parse(JsonConvert.SerializeObject(new_data));
                File.WriteAllText(root  + "\\_data\\_jsondata.json", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                return CheckHash();
            }
            return change;
        }

        #region ori
        //static bool CheckHash()
        //{
        //    bool change = false;
        //    DirectoryInfo Root = new DirectoryInfo(root);
        //    //fix
        //    //if (!File.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\StatChange.xml") || !File.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\new_gdata.json"))
        //    //{
        //    //    return true;
        //    //}

        //    //计算文件夹是否有变动
        //    //Whether the calculation folder has changed
        //    string _check = string.Empty;

        //    foreach (DirectoryInfo nf in Root.GetDirectories())
        //    {
        //        //遍历所有mod数据文件夹
        //        //Traverse all mod folders
        //        if (nf.Name != "_data")
        //        {
        //            //进入子文件夹
        //            //Enter Subfolder
        //            foreach (FileInfo nf1 in nf.GetFiles())
        //            {
        //                if (nf1.Name == "config.json")
        //                {

        //                    //read json
        //                    StreamReader reader = File.OpenText(nf1.FullName);
        //                    JsonTextReader jsonTextReader = new JsonTextReader(reader);
        //                    JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
        //                    reader.Close();

        //                    if (!config["UseLib"].ToString().Equals("True"))
        //                        break;


        //                    Directory.CreateDirectory(nf.FullName + "\\Additions");
        //                    string CodeAdd = FigureHash(nf.FullName + "\\Additions");
        //                    if (CodeAdd != config["Hash"]["Add_Hash"].ToString())
        //                    {
        //                        config["Hash"]["Add_Hash"] = CodeAdd;
        //                        change = true;

        //                    }
        //                    Directory.CreateDirectory(nf.FullName + "\\Additions\\statchange");
        //                    string CodeStat = FigureHash(nf.FullName + "\\Additions\\statchange");
        //                    if (CodeStat != config["Hash"]["Statchange_Hash"].ToString())
        //                    {
        //                        config["Hash"]["Statchange_Hash"] = CodeStat;
        //                        change = true;

        //                    }
        //                    Directory.CreateDirectory(nf.FullName + "\\Replacements");
        //                    string CodeRep = FigureHash(nf.FullName + "\\Replacements");
        //                    if (CodeRep != config["Hash"]["Rep_Hash"].ToString())
        //                    {
        //                        config["Hash"]["Rep_Hash"] = CodeRep;
        //                        change = true;

        //                    }

        //                    //如果hash值有变化，写入新config文件
        //                    //If the hash value changes, write a new config file
        //                    if (change)
        //                    {
        //                        string json_output = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        //                        File.WriteAllText(nf1.FullName, json_output);

        //                    }

        //                    //检查mod是否有卸载或增加，将文件夹名字加入字符串
        //                    //Check whether the mod is uninstalled or added, and add the folder name to the string
        //                    _check += nf.FullName;

        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    int _checkCode = 0;
        //    //若为空，说明没有使用gdata的mod
        //    //If it is empty, the mod of gdata is not used
        //    if (_check != null)
        //    {
        //        _checkCode = _check.GetHashCode();
        //    }

        //    //检查gdata源文件是否发生更新，首先计算目前gdata的hash值
        //    //Check whether the gdata source file is updated. First, calculate the hash value of the current gdata
        //    string _gdataHash = new FileInfo(path_g).LastWriteTimeUtc.GetHashCode().ToString();

        //    //读取_check.json文件，对比哈希值
        //    //read _check.json file, Contrast Hash
        //    {
        //        StreamReader reader = File.OpenText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json");
        //        JsonTextReader jsonTextReader = new JsonTextReader(reader);
        //        JObject check_json = (JObject)JToken.ReadFrom(jsonTextReader);
        //        reader.Close();
        //        //Arklib版本更新，重写check
        //        //arklib updata, rewrite cheak
        //        if (!check_json["Arklib_ver"].ToString().Equals(version.ToString()))
        //        {
        //            LibDocument lib = new LibDocument();
        //            JObject jsonCheck = (JObject)JsonConvert.DeserializeObject(lib.Get_cheak());
        //            string check1 = JsonConvert.SerializeObject(jsonCheck, Newtonsoft.Json.Formatting.Indented);
        //            File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json", check1);
        //            return CheckHash();
        //        }
        //        //gdata更新
        //        //gdata updata
        //        if (!check_json["Gdata_Hash"].ToString().Equals(_gdataHash.ToString()))
        //        {
        //            check_json["Gdata_Hash"] = _gdataHash;
        //            change = true;
        //        }
        //        //mod文件状态更新
        //        //mod's state updata
        //        if (!check_json["Modfileinfo_Hash"].ToString().Equals(_checkCode.ToString()))
        //        {
        //            check_json["Modfileinfo_Hash"] = _checkCode.ToString();
        //            change = true;
        //        }
        //        if (change)
        //        {
        //            string json_output = JsonConvert.SerializeObject(check_json, Newtonsoft.Json.Formatting.Indented);
        //            File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json", json_output);
        //        }

        //    }
        //    return change;

        //}
        #endregion

        //根据文件夹内文件名和文件最后修改时间，计算哈希值
        //Calculate the hash value according to the file name in the folder and the last modification time of the file
        static int FigureHash(DirectoryInfo path)
        {
            string target = string.Empty;
            foreach (FileInfo file in path.GetFiles())
            {
                target += file.Name + file.LastWriteTimeUtc.ToString();
            }
            return target.GetHashCode();
        }

        static void Merge(bool change)
        {
            if (!change)
                return;

            //列出所有mod需要合并的文件目录
            //List the file directories to be merged for all mods
            Queue<FileInfo> ListAdditions = new Queue<FileInfo>();
            Queue<FileInfo> ListReplacements = new Queue<FileInfo>();
            Queue<FileInfo> ListStatChange = new Queue<FileInfo>();
            //查找并添加
            //search and add
            foreach (var modlist in DIR)
            {

                ListAdditions = SearchFile(ListAdditions, new DirectoryInfo(modlist.Value.FullName + "\\Additions"), "json");
                ListReplacements = SearchFile(ListReplacements, new DirectoryInfo(modlist.Value.FullName + "\\Replacements"), "json");
                ListStatChange = SearchFile(ListStatChange, new DirectoryInfo(modlist.Value.FullName + "\\StatChange"), "xml");

            }

            //Read game orginal gdata
            StreamReader reader_ = File.OpenText(path_g);
            JsonTextReader jsonTextReader_ = new JsonTextReader(reader_);
            JObject pg = (JObject)JToken.ReadFrom(jsonTextReader_);
            reader_.Close();

            //json添加
            //add
            if (ListAdditions.Count > 0)
            {
                pg.Merge(JsonAdd(ListAdditions), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge
                });
            }
            //替换
            //replace
            if (ListReplacements.Count > 0)
            {
                pg.Merge(JsonAdd(ListReplacements), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            //导出gdata
            //Export gdata
            string json_output = JsonConvert.SerializeObject(pg, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(root + "\\_data\\new_gdata.json", json_output);

            //xml合并
            if (ListStatChange.Count > 0)
            {
                //xml合并
                //merge xml

                //首文件，用于后续添加操作
                JObject firstJson = new JObject();
                JArray firstJA = new JArray();
                bool flag1 = true;
                bool flag2 = true;

                //储存唯一UseKey： Mod_StatChange_Script_Item的字典
                Dictionary<string, JObject> modItem = new Dictionary<string, JObject>();
                while (ListStatChange.Count > 0)
                {
                    //逐个读取文件
                    FileInfo temp = ListStatChange.Dequeue();
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(File.OpenText(temp.FullName));

                    Debug.Log($"Merging file: {temp.Name}");

                    //转化为json
                    JObject xml_json = (JObject)JToken.Parse(JsonConvert.SerializeXmlNode(xmld));

                    if (flag1)
                    {
                        firstJson = xml_json;
                        flag1 = false;
                    }

                    //读取Mod_StatChange_Script_Item中的信息
                    JArray xml_ja = (JArray)xml_json["Mod_StatChange_Script_Main"]["Main"]["Mod_StatChange_Script_Item"];

                    if (flag2)
                    {
                        firstJA = xml_ja;
                        flag2 = false;
                    }

                    //唯一添加到字典中
                    for (int i = 0; i < xml_ja.Count; i++)
                    {
                        if (!modItem.ContainsKey(xml_ja[i]["UseKey"].ToString()))
                        {
                            modItem.Add(xml_ja[i]["UseKey"].ToString(), (JObject)xml_ja[i]);
                        }
                    }


                    //将字典中的值添加到原数据中
                    foreach (KeyValuePair<string, JObject> kvp in modItem)
                    {
                        bool add__instance = true;
                        for (int i = 0; i < firstJA.Count; i++)
                        {

                            if (kvp.Key.Equals(firstJA[i]["UseKey"].ToString()))
                            {
                                add__instance = false;
                            }
                        }
                        if (add__instance)
                        {
                            firstJA.Add(kvp.Value);
                        }
                    }
                    firstJson["Mod_StatChange_Script_Main"]["Main"]["Mod_StatChange_Script_Item"] = firstJA;
                    XmlDocument doc = JsonConvert.DeserializeXmlNode(firstJson.ToString());
                    doc.Save(root + "\\_data\\StatChange.xml");
                }
            }
        }

        private static Queue<FileInfo> SearchFile(Queue<FileInfo> target, DirectoryInfo dir, string extension)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                if (extension == "json")
                {
                    if (file.Extension.Equals(".json"))
                    {
                        target.Enqueue(file);
                        
                    }
                }
                else if (extension == "xml")
                {
                    if (file.Extension.Equals(".xml") || file.Extension.Equals(".txt"))
                    {
                        target.Enqueue(file);
                    }
                }
            }
            return target;
        }

        static JObject JsonAdd(Queue<FileInfo> list)
        {
            //首文件，用于后续添加操作
            JObject firstJson = new JObject();
            bool flag1 = true;
            while (list.Count > 0)
            {
                FileInfo temp = list.Dequeue();
                Debug.Log($"Merging file: {temp.Name}");

                //根据出队的文件名，创建JO类型的对象
                JObject json = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(temp.FullName)));

                if (flag1)
                {
                    firstJson = json;
                    flag1 = false;
                }
                else
                {
                    firstJson.Merge(json, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    });
                }

            }
            return firstJson;
        }

        [HarmonyPatch(typeof(SaveManager), "Awake")]
        class StatChangeModPatch
        {
            static void Prefix()
            {
                Mod_StatChangeMod.Path = root + "\\_data\\StatChange.xml";
                Mod_StatChangeMod.LoadFile();
            }
        }

        //Change gdata path
        [HarmonyPatch(typeof(GDEDataManager))]
        class ModifyGdata
        {

            static string NewMasterDataPath()
            {
                var path = root + "\\_data\\new_gdata.json";
                Debug.Log($"Working in Init[1], data is {path}");
                return path;
            }

            [HarmonyPatch(nameof(GDEDataManager.Init), new Type[] { typeof(bool) })]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Stsfld, AccessTools.Field(typeof(GDEDataManager), "masterDataPath")))
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModifyGdata), nameof(ModifyGdata.NewMasterDataPath)));
                        yield return ci;
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }
        }
    }

    public class Hashdata
    {
        public int Additions_Hash;
        public int Replacements_Hash;
        public int StatChange_Hash;
    }

    public class NewHashdata
    {
        public int Hash_gdata;
        public int Hash_modtotal;
    }
}



