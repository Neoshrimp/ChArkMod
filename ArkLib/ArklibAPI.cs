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
                                if (this.Prefix)
                                {
                                    FolderPreprocessing(temppath);
                                }
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
    }

}
