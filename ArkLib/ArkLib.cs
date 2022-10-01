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

namespace ArkLib
{
    [BepInPlugin(GUID, "ArkLib", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ArkLib : BaseUnityPlugin
    {
        public const string GUID = "com.DRainw.ChArkMod.ArkLib";
        public const string version = "1.1.0";

        private static readonly Harmony harmony = new Harmony(GUID);
        private static readonly string root = BepInEx.Paths.PluginPath + "\\ArkLib";
        private static readonly string path_g = Application.streamingAssetsPath + "/gdata.json";


        void Awake()
        {
            try
            {
                CheckRoot();
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
                harmony.UnpatchAll(GUID);
        }

        static void CheckRoot()
        {
            //初始化
            if (!Directory.Exists(root) || !Directory.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data") || !File.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json"))
            {
                PatchInit();
            }
            //if(配置文件中生成模板 == true)
            //PatchInit();
        }

        //创建模板
        static void PatchInit()
        {

            //创建数据文件夹
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib");
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib\\_data");

            //创建内部文件夹
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib\\Sample");
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib\\Sample\\Additions");
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib\\Sample\\Additions\\statchange");
            Directory.CreateDirectory(BepInEx.Paths.PluginPath + "\\ArkLib\\Sample\\Replacements");

            //生成config.json
            LibDocument lib = new LibDocument();
            JObject jsonData = (JObject)JsonConvert.DeserializeObject(lib.GetConfig());
            string config = JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\Sample\\config.json", config);
            

            //生成_check.json
            JObject jsonCheck = (JObject)JsonConvert.DeserializeObject(lib.Get_cheak());
            string _check = JsonConvert.SerializeObject(jsonCheck, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json", _check);
        }

        static bool CheckHash()
        {
            bool change = false;
            DirectoryInfo Root = new DirectoryInfo(root);
            if (!File.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\StatChange.xml") || !File.Exists(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\new_gdata.json"))
            {
                return true;
            }

            //计算文件夹是否有变动
            string _check = string.Empty;

            foreach (DirectoryInfo nf in Root.GetDirectories())
            {
                //遍历所有mod数据文件夹
                if (nf.Name != "_data")
                {
                    //进入子文件夹
                    foreach (FileInfo nf1 in nf.GetFiles())
                    {
                        if (nf1.Name == "config.json")
                        {

                            //读取json
                            StreamReader reader = File.OpenText(nf1.FullName);
                            JsonTextReader jsonTextReader = new JsonTextReader(reader);
                            JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                            reader.Close();

                            if (!config["UseLib"].ToString().Equals("True"))
                                break;


                            Directory.CreateDirectory(nf.FullName + "\\Additions");
                            string CodeAdd = FigureHash(nf.FullName + "\\Additions");
                            if (CodeAdd != config["Hash"]["Add_Hash"].ToString())
                            {
                                config["Hash"]["Add_Hash"] = CodeAdd;
                                change = true;

                            }
                            Directory.CreateDirectory(nf.FullName + "\\Additions\\statchange");
                            string CodeStat = FigureHash(nf.FullName + "\\Additions\\statchange");
                            if (CodeStat != config["Hash"]["Statchange_Hash"].ToString())
                            {
                                config["Hash"]["Statchange_Hash"] = CodeStat;
                                change = true;

                            }
                            Directory.CreateDirectory(nf.FullName + "\\Replacements");
                            string CodeRep = FigureHash(nf.FullName + "\\Replacements");
                            if (CodeRep != config["Hash"]["Rep_Hash"].ToString())
                            {
                                config["Hash"]["Rep_Hash"] = CodeRep;
                                change = true;

                            }

                            //如果hash值有变化，写入新config文件
                            if (change)
                            {
                                string json_output = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                                File.WriteAllText(nf1.FullName, json_output);
                                
                            }

                            //检查mod是否有卸载或增加，将文件夹名字加入字符串
                            _check += nf.FullName;

                            break;
                        }
                    }
                }
            }

            int _checkCode = 0;
            //若为空，说明没有使用gdata的mod
            if (_check != null)
            {
                _checkCode = _check.GetHashCode();
            }

            //检查gdata源文件是否发生更新，首先计算目前gdata的hash值
            string _gdataHash = new FileInfo(path_g).LastWriteTimeUtc.GetHashCode().ToString();

            //读取_check.json文件，对比哈希值
            {
                StreamReader reader = File.OpenText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json");
                JsonTextReader jsonTextReader = new JsonTextReader(reader);
                JObject check_json = (JObject)JToken.ReadFrom(jsonTextReader);
                reader.Close();
                //Arklib版本更新，重写check
                if (!check_json["Arklib_ver"].ToString().Equals(version.ToString()))
                {
                    LibDocument lib = new LibDocument();
                    JObject jsonCheck = (JObject)JsonConvert.DeserializeObject(lib.Get_cheak());
                    string check1 = JsonConvert.SerializeObject(jsonCheck, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json", check1);
                }
                //gdata更新
                if (!check_json["Gdata_Hash"].ToString().Equals(_gdataHash))
                {
                    check_json["Gdata_Hash"] = _gdataHash;
                    change = true;
                }
                //mod文件状态更新
                if (!check_json["Modfileinfo_Hash"].ToString().Equals(_checkCode))
                {
                    check_json["Modfileinfo_Hash"] = _checkCode.ToString();
                    change = true;
                }
                if (change)
                {
                    string json_output = JsonConvert.SerializeObject(check_json, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\_check.json", json_output);
                }

            }
            return change;

        }

        //根据文件夹内文件名和文件最后修改时间，计算哈希值
        static string FigureHash(string path)
        {
            string target = string.Empty;
            DirectoryInfo Path = new DirectoryInfo(path);
            foreach (FileInfo file in Path.GetFiles())
            {
                target += file.Name + file.LastWriteTimeUtc.ToString();
            }
            return target.GetHashCode().ToString();
        }

        static void Merge(bool change)
        {
            if (!change)
                return;

            //列出所有mod需要合并的文件夹目录
            Queue<string> ListStatChange = new Queue<string>();
            Queue<string> ListAdditions = new Queue<string>();
            Queue<string> ListReplacements = new Queue<string>();

            //添加目录
            DirectoryInfo Root = new DirectoryInfo(root);
            foreach (DirectoryInfo nf in Root.GetDirectories())
            {
                if (nf.Name != "_data")
                {
                    //进入子文件夹
                    foreach (FileInfo nf1 in nf.GetFiles())
                    {
                        if (nf1.Name == "config.json")
                        {
                            //读取json
                            StreamReader reader = File.OpenText(nf1.FullName);
                            JsonTextReader jsonTextReader = new JsonTextReader(reader);
                            JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                            reader.Close();

                            if (!config["UseLib"].ToString().Equals("True"))
                                break;

                            ListAdditions.Enqueue(nf.FullName + "\\Additions");
                            ListReplacements.Enqueue(nf.FullName + "\\Replacements");
                            ListStatChange.Enqueue(nf.FullName + "\\Additions\\statchange");

                            break;
                        }
                    }
                }
            }

            //json合并

            //Read game orginal gdata
            StreamReader reader_ = File.OpenText(path_g);
            JsonTextReader jsonTextReader_ = new JsonTextReader(reader_);
            JObject pg = (JObject)JToken.ReadFrom(jsonTextReader_);
            reader_.Close();

            //json添加
            if (ListAdditions.Count > 0)
            {
                pg.Merge(JsonAdd(ListAdditions), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge
                });
            }
            //替换
            if (ListReplacements.Count > 0)
            {
                pg.Merge(JsonAdd(ListAdditions), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }
            //导出gdata
            string json_output = JsonConvert.SerializeObject(pg, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\new_gdata.json", json_output);


            //xml合并
            {
                Queue<FileInfo> XmlList = new Queue<FileInfo>();
                while (ListStatChange.Count > 0)
                {
                    DirectoryInfo folder = new DirectoryInfo(ListStatChange.Dequeue());
                    foreach (FileInfo nfolder in folder.GetFiles())
                    {
                        if (nfolder.Extension.Equals(".xml") || nfolder.Extension.Equals(".txt"))
                        {
                            XmlList.Enqueue(nfolder);
                        }
                    }
                }

                //如果无，则终止
                if (XmlList.Count == 0)
                    return;

                //首文件，用于后续添加操作
                JObject firstJson = new JObject();
                JArray firstJA = new JArray();
                bool flag1 = true;
                bool flag2 = true;

                //储存唯一UseKey： Mod_StatChange_Script_Item的字典
                Dictionary<string, JObject> modItem = new Dictionary<string, JObject>();
                while (XmlList.Count > 0)
                {
                    //逐个读取文件
                    FileInfo temp = XmlList.Dequeue();
                    XmlDocument xmld = new XmlDocument();
                    xmld.Load(File.OpenText(temp.FullName));

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

                doc.Save(BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\StatChange.xml");
            }
        }


        static JObject JsonAdd(Queue<string> list)
        {
            Queue<FileInfo> JsonList = new Queue<FileInfo>();
            while (list.Count > 0)
            {
                DirectoryInfo folder = new DirectoryInfo(list.Dequeue());
                foreach (FileInfo nfolder in folder.GetFiles())
                {
                    if (nfolder.Extension.Equals(".json"))
                    {
                        JsonList.Enqueue(nfolder);
                    }
                }

            }
            //首文件，用于后续添加操作
            JObject firstJson = new JObject();
            bool flag1 = true;
            while (JsonList.Count > 0)
            {
                FileInfo temp = JsonList.Dequeue();
                Debug.Log($"正在合并文件{temp.Name}");

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


        //Change StatChange patch
        [HarmonyPatch(typeof(Mod_StatChangeMod))]
        class StatChangeModPatch
        {
            [HarmonyPatch(nameof(Mod_StatChangeMod.LoadFile))]
            static bool Prefix()
            {
                string path = BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\StatChange.xml";

                try
                {
                    if (File.Exists(path))
                    {
                        FileStream fileStream = null;
                        Mod_StatChangeMod.serializer = new XmlSerializer(typeof(Mod_StatChange_Script_Main));
                        fileStream = new FileStream(path, FileMode.Open);
                        try
                        {
                            Mod_StatChangeMod.Main = (Mod_StatChange_Script_Main)Mod_StatChangeMod.serializer.Deserialize(fileStream);
                            fileStream.Close();
                        }
                        catch
                        {
                            fileStream.Close();
                        }
                    }
                }
                catch
                {
                }
                return false;
            }
        }

        //Change gdata path
        [HarmonyPatch(typeof(GDEDataManager))]
        class ModifyGdata
        {

            [HarmonyPatch(nameof(GDEDataManager.Init), new Type[] { typeof(bool) })]
            [HarmonyPrefix]
            static bool Prefix(ref bool __result, bool encrypted = false)
            {

                //Define the gdata.json path as masterDataPath.
                string masterDataPath = BepInEx.Paths.PluginPath + "\\ArkLib\\_data\\new_gdata.json";
                bool result = true;
                GDEDataManager.GDEResourcesData = (GDESpriteResources)Resources.Load("GDESpriteResources");
                try
                {
                    TextAsset dataAsset = new TextAsset(File.ReadAllText(masterDataPath));
                    result = GDEDataManager.Init(dataAsset, encrypted);
                }
                catch (Exception message)
                {
                    Debug.LogError(message);
                    result = false;
                }
                __result = result;
                UnityEngine.Debug.Log($"Working in Init[1], data is {masterDataPath}");

                return false;
            }


        }
    }

    public class LibDocument
    {
        public string GetConfig()
        {
            string str = @"{
                                'UseLib': false,
                                'Modname': '',
                                'GUID': '',
                                'Hash': {
                                    'Add_Hash': '',
                                    'Rep_Hash': '',
                                    'Statchange_Hash': ''
                                }
                            }";
            return str;
        }

        public string Get_cheak()
        {
            string str = @"{
                                'Arklib_ver': '1.1.0',
                                'Gdata_Hash': '',
                                'Modfileinfo_Hash': ''
                            }";

            return str;
        }
    }
}
