using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using System.Reflection.Emit;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using Debug = UnityEngine.Debug;



namespace ArklibAPI
{
    public class ModtheFolder
    {
        public ModtheFolder()
        {
            this.Prefix = false;
        }

        public ModtheFolder(List<string> Preprocessing)
        {
            this.Prefix = true;
            this.Preprocessing = Preprocessing;
        }

        private bool Prefix;
        private List<string> Preprocessing = new List<string>();
        private string tempname = string.Empty;
        private DirectoryInfo temppath;

        public Dictionary<string, DirectoryInfo> GetModRoots(string GetType)
        {
            Dictionary<string, DirectoryInfo> path = new Dictionary<string, DirectoryInfo>();
            try
            {
                foreach (DirectoryInfo fd in new DirectoryInfo(BepInEx.Paths.PluginPath).GetDirectories())
                {
                    foreach (FileInfo file in fd.GetFiles())
                    {
                        if (file.Name == "arklib_config.json")
                        {
                            if (Check(GetType, file))
                            {
                                path.Add(tempname, temppath);
                                Debug.Log($"ArklibAPI: Loading mod's folder {tempname}: {fd.FullName}, operator: {GetType}, return path: {temppath.FullName}.");
                                tempname = string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return path;
        }

        private bool Check(string GetType, FileInfo file)
        {
            bool flag = false;

            StreamReader reader = File.OpenText(file.FullName);
            JsonTextReader jsonTextReader = new JsonTextReader(reader);
            JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
            reader.Close();



            if (config.ContainsKey("Modname") && config["Modname"].ToString().Length > 2)
            {
                tempname = config["Modname"].ToString();
                temppath = Directory.CreateDirectory(file.Directory.FullName + "\\" + tempname + "-" + GetType);
                //Debug.Log($"{tempname}, {temppath.FullName}");

                if (config.ContainsKey("Use" + GetType))
                {
                    if (config["Use" + GetType].ToString().ToLower() == "true")
                    {
                        flag = true;
                        if (this.Prefix)
                        {
                            FolderPreprocessing(temppath);
                        }
                        ModManagerFix(tempname, file.Directory.FullName, temppath);
                    }
                }
                else
                {
                    config.Add("Use" + GetType, false);

                    string json_output = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(file.FullName, json_output);
                }

            }
            else
            {
                Debug.LogWarning($"Config file format is not correct in {file.FullName}");
                return false;
            }

            return flag;
        }

        private void FolderPreprocessing(DirectoryInfo dir)
        {
            if (this.Preprocessing.Count > 0)
            {
                foreach (string str in this.Preprocessing)
                {
                    //Debug.Log("str:" + str);
                    Directory.CreateDirectory(dir.FullName + "\\" + str);
                }
            }
        }

        private void ModManagerFix(string name, string rootpath, DirectoryInfo targetpath)
        {
            FileInfo[] infos = targetpath.GetFiles("*", SearchOption.AllDirectories);
            //还原
            if (infos.Length == 0)
            {
                StreamReader reader = File.OpenText(rootpath + "\\_managerfix");
                JsonTextReader jsonTextReader = new JsonTextReader(reader);
                JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                reader.Close();

                foreach (FileInfo f in new DirectoryInfo(rootpath).GetFiles())
                {
                    if (config.ContainsKey(f.Name))
                    {
                        Debug.Log("Assembling  file: " + f.Name);
                        File.Move(f.FullName, rootpath + config[f.Name].ToString());
                    }
                }

                Managerfix new_data = new Managerfix
                {
                    Modname = name,
                    isReplace = true
                };
                JObject config_ = JObject.Parse(JsonConvert.SerializeObject(new_data));
                File.WriteAllText(rootpath + "\\_managerfix", JsonConvert.SerializeObject(config_, Newtonsoft.Json.Formatting.Indented));
            }
            else    //记录
            {
            FT:

                if (File.Exists(rootpath + "\\_managerfix"))
                {
                    //read json
                    StreamReader reader = File.OpenText(rootpath + "\\_managerfix");
                    JsonTextReader jsonTextReader = new JsonTextReader(reader);
                    JObject config = (JObject)JToken.ReadFrom(jsonTextReader);
                    reader.Close();

                    if (config["isReplace"].ToString().ToLower() == "true")
                        return;

                    foreach (FileInfo f in infos)
                    {
                        string relative_path = f.FullName.Replace(rootpath, "");
                        if (config.ContainsKey(f.Name))
                        {
                            config[f.Name] = relative_path;
                        }
                        else
                        {
                            config.Add(f.Name, relative_path);
                        }
                    }

                    File.WriteAllText(rootpath + "\\_managerfix", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                }
                else
                {
                    Managerfix new_data = new Managerfix
                    {
                        Modname = name,
                        isReplace = false
                    };
                    JObject config = JObject.Parse(JsonConvert.SerializeObject(new_data));
                    File.WriteAllText(rootpath + "\\_managerfix", JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                    goto FT;
                }
            }
        }

        private class Managerfix
        {
            public string Modname;
            public bool isReplace;
        }
    }

}
