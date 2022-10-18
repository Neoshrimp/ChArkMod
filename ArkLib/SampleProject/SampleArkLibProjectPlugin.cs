using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SampleArkLibProject
{
    [BepInPlugin(GUID, "Sample ArkLib Project", version)]
    [BepInDependency("com.DRainw.ChArkMod.ArkLib")]
    [BepInProcess("ChronoArk.exe")]
    public class SampleArkLibProjectPlugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.tools.sampleArkLib";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        public static BepInEx.Logging.ManualLogSource logger;

        public class MyExtend : Skill_Extended
        {
            public override string DescExtended(string desc)
            {
                return "THIS IS MY EXTEND";
            }
        }

        void Awake()
        {
            logger = Logger;

            logger.LogInfo(typeof(MyExtend).AssemblyQualifiedName);


            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }




    }
}
