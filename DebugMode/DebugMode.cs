#define EN
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
using System.Xml;
using System.Xml.Serialization;
using DarkTonic.MasterAudio;
using Scrolls;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using UseItem;



namespace DebugMode
{
    [BepInPlugin(GUID, "DebugMode", version)]
    [BepInProcess("ChronoArk.exe")]
    public class DebugMode : BaseUnityPlugin
    {
        public const string GUID = "com.DRainw.ChArkMod.DebugMode";
        public const string version = "2.0.1";

        private static ConfigEntry<string> Language;


        private static readonly Harmony harmony = new Harmony(GUID);
        private bool windowShow = false;
        private static float Scale = 1.0f;


        void Awake()
        {
            Logger.LogInfo("BepInex: DebugMode loading.");
#if EN
            Language = Config.Bind("language", "Language", "en-us", "Change language, optional values: en-us, zh-cn.\n修改语言，可选值：en-us, zh-cn");
#endif
#if CH
            Language = Config.Bind("language", "Language", "zh-cn", "Change language, optional values: en-us, zh-cn.\n修改语言，可选值：en-us, zh-cn");
#endif
            DefineLang();
            harmony.PatchAll();
        }


        private static string Key_OK = "OK";
        private static string Key_Clear = "Clear";
        private static string ON_GUI = "Enter debug code";
        static void DefineLang()
        {
            if(Language.Value == "zh-cn")
            {
                Key_OK = "确认";
                Key_Clear = "清除";
                ON_GUI = "输入调试码";
            }
            else
            {
                Key_OK = "OK";
                Key_Clear = "Clear";
                ON_GUI = "Enter debug code";
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                windowShow = !windowShow;
            }
            //Debug.Log($"{PlayData.CheatChat}");
            if(windowShow && Input.GetKeyDown(KeyCode.Return))
            {
                PlayData.CheatChat = s;
            }
            if(Time.timeScale != Scale)
            {
                Time.timeScale = Scale;
            }
        }

        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        private Rect Wrect = new Rect(1032, 700, 500, 65);
        static string s = "";

        public void Wfunc(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                windowShow = false;
            }
            GUILayout.EndHorizontal();

            s = GUILayout.TextField(s, 25);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Key_OK) || (windowShow && Input.GetKeyDown(KeyCode.Return)))
            {
                PlayData.CheatChat = s;
            }
            if (GUILayout.Button(Key_Clear))
            {
                PlayData.CheatChat = string.Empty;
                s = string.Empty;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
        void OnGUI()
        {
            if (windowShow)
                Wrect = GUILayout.Window(122, Wrect, Wfunc, ON_GUI);
            //Debug.Log($"x: {Wrect.x}, y: {Wrect.y}");

        }


        //开启开发者模式
        [HarmonyPatch(typeof(SaveManager), "Awake")]
        class Debug_mode_Enable_Patch
        {
            private static void Postfix(ref bool ___DebugMode)
            {
                ___DebugMode = true;
                Debug.Log($"DebugMode: {___DebugMode}");
            }
        }
        //主要代码1
        [HarmonyPatch(typeof(StageSystem), "CheatChack")]
        class CheatPatch1
        {
            
            [HarmonyPrefix]
            static bool Prefix(StageSystem __instance)
            {
                //if (Input.GetKey(KeyCode.Return))
                //{
                //    PlayData.CheatChat = string.Empty;
                //}
                //foreach (char c in Input.inputString)
                //{
                //    if (c == "\n"[0] || c == "\r"[0])
                //    {
                //        PlayData.CheatChat = string.Empty;
                //    }
                //    else
                //    {
                //        PlayData.CheatChat += c;
                //    }
                //}
                

                string cheatChat = PlayData.CheatChat;
                switch (cheatChat)
                {
                    //转盘
                    case "roulette":
                        {
                            __instance.CheatEnabled();
                            //List<ItemBase> list = new List<ItemBase>();
                            //list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SmallBarrierMachine));
                            //list.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key));
                            //list.Add(ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter));
                            //list.Add(ItemBase.GetItem(GDEItemKeys.Item_Equip_GasMask));
                            //list.Add(ItemBase.GetItem(GDEItemKeys.Item_Active_PotLid));
                            UIManager.InstantiateActive(UIManager.inst.RouletteUI).GetComponent<UI_MiniGame_Roulette>().Init();
                            break;
                        }
                    //迷雾全开
                    case "fogout":
                        __instance.CheatEnabled();
                        __instance.Fogout(true);
                        break;
                    //迷雾全关
                    case "fogon":
                        __instance.CheatEnabled();
                        __instance.FogOn();
                        break;
                    //迷雾全开
                    case "fogwayout":
                        {
                            __instance.CheatEnabled();
                            List<Vector2> list2 = MapTile.MapRange(MapTile.VecToCube(__instance.PlayerPos), 30, __instance.Map.Size);
                            var SA = Traverse.Create(__instance).Field("SightAstar").GetValue<AStar>();
                            foreach (Vector2 end in list2)
                            {
                                SA.Start(__instance.PlayerPos, end, __instance.Map.MapObject);
                                SA.HiddenWall = true;
                                SA.Update(__instance.Map.MapObject);
                                if (SA.NowNode != null && SA.NowNode.Count() - 1 <= 30)
                                {
                                    __instance.Map.MapObject[(int)end.x, (int)end.y].HexTileComponent.SightOn();
                                    if (FieldSystem.instance.MiniMap.MapImages[(int)end.x, (int)end.y] != null)
                                    {
                                        FieldSystem.instance.MiniMap.MapImages[(int)end.x, (int)end.y].SightUpdate(true, false);
                                    }
                                }
                            }
                            break;
                        }
                    //修改游戏运行速度，待完善
                    case "scale":
                        Scale = 1f;
                        break;
                    case "scale2":
                        Scale = 2f;
                        break;
                    case "scale3":
                        Scale = 3f;
                        break;
                    //从当前关卡敌人中随机选择组进行战斗
                    case "testfield":
                        __instance.CheatEnabled();
                        FieldSystem.instance.BattleStart_MapEnemy(__instance.StageData.BattleMap.Key, __instance.Map.MapObject[(int)__instance.PlayerPos.x, (int)__instance.PlayerPos.y].Info.Cursed, string.Empty, string.Empty);
                        break;
                    //与教官进行战斗
                    case "testsandbox":
                        __instance.CheatEnabled();
                        FieldSystem.instance.BattleStart(new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_TestQueue), __instance.StageData.BattleMap.Key, false, false, string.Empty, string.Empty);
                        break;
                    //速度增加
                    case "fastrun":
                        __instance.CheatEnabled();
                        __instance.Player.GetComponent<PlayerController>().FastRun();
                        break;
                    //和平模式：遇敌不触发战斗
                    case "peace":
                        __instance.CheatEnabled();
                        //发掘私有属性
                        bool peace = Traverse.Create(__instance).Field("peacemode").GetValue<bool>();
                        Traverse.Create(__instance).Field("peacemode").SetValue(!peace);
                        break;
                    //飞行模式
                    case "fly":
                        __instance.CheatEnabled();
                        __instance.Player.GetComponent<PlayerController>().Fly();
                        break;
                    //跳至下一关篝火
                    case "nextstage":
                        __instance.CheatEnabled();
                        SaveManager.NowData.TutorialEnd = true;
                        SaveManager.savemanager.Save();
                        FieldSystem.instance.NextStage();
                        break;
                    //case "startani":
                    //    __instance.CheatEnabled();
                    //    __instance.StartCoroutine(__instance.StartAni());
                    //    break;
                    case "getkey":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Misc_Item_Key, 10)
                        });
                        break;
                    //case "restart":
                    //    __instance.CheatEnabled();
                    //    __instance.StageInit(GDEItemKeys.Stage_Stage1_2);
                    //    break;

                    //直接进入荒野
                    case "nextcrimson":
                        __instance.CheatEnabled();
                        __instance.StageInit(GDEItemKeys.Stage_Stage_Crimson);
                        break;
                    //case "tiledebug":
                    //    __instance.CheatEnabled();
                    //    __instance.Map.DebugSet();
                    //    break;
                    //进入吃知识的图书馆
                    //case "eventtest":
                    //    {
                    //        __instance.CheatEnabled();
                    //        GameObject gameObject = UIManager.InstantiateActive(UIManager.inst.RandomEventMainUI);
                    //        gameObject.GetComponent<RandomEventUI>().EventInit(GDEItemKeys.RandomEvent_RE_EatingKnowledge, null, null);
                    //        gameObject.transform.SetParent(FieldSystem.instance.RandomEventUITrans);
                    //        break;
                    //    }
                    //自选当前场景可用事件
                    case "eventstage":
                        {
                            __instance.CheatEnabled();
                            List<string> list3 = new List<string>();
                            foreach (GDERandomEventData gderandomEventData in StageSystem.instance.Map.StageData.RandomEventList)
                            {
                                list3.Add(gderandomEventData.Key);
                            }
                            FieldEventSelect.FieldEventSelectOpen(list3, null, Instantiate(__instance.RandomEventMainObject_S1), true);
                            break;
                        }
                    //自选事件
                    case "eventall":
                        {
                            __instance.CheatEnabled();
                            List<string> eventList = new List<string>();
                            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.RandomEvent, out eventList);
                            FieldEventSelect.FieldEventSelectOpen(eventList, null, Instantiate(__instance.RandomEventMainObject_S1), true);
                            break;
                        }
                    //强化
                    case "enforce":
                        __instance.CheatEnabled();
                        UIManager.InstantiateActive(UIManager.inst.EnforceUI).GetComponent<UI_Enforce>().Init(false, null, 2);
                        break;
                    //自选技能强化
                    case "allenforce":
                        {
                            __instance.CheatEnabled();
                            List<Skill_Extended> list4 = new List<Skill_Extended>();
                            List<string> list5 = new List<string>();
                            UI_Enforce component = UIManager.InstantiateActive(UIManager.inst.EnforceUI).GetComponent<UI_Enforce>();
                            GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.SkillExtended, out list5);
                            foreach (string key in list5)
                            {
                                GDESkillExtendedData gdeskillExtendedData = new GDESkillExtendedData(key);
                                if (gdeskillExtendedData.Drop)
                                {
                                    list4.Add(Skill_Extended.DataToExtended(gdeskillExtendedData));
                                }
                            }
                            for (int j = 0; j < list4.Count; j++)
                            {
                                EnforceButton component2 = UnityEngine.Object.Instantiate<GameObject>(component.EnforceButton, component.DebugAlign).GetComponent<EnforceButton>();
                                component2.MainExtended = list4[j];
                                if (component2.MainExtended.Data.Debuff)
                                {
                                    component2.Weak = true;
                                }
                                component2.MainUI = component;
                                component2.SelectChar = null;
                            }
                            break;
                        }
                    //清除周围敌人
                    case "cleansed":
                        __instance.CheatEnabled();
                        StageSystem.instance.Purfication();
                        break;
                    //case "text":
                    //    __instance.CheatEnabled();
                    //    break;
                    case "getbook":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter, 10),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 10),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookSuport, 5),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter_Rare, 5),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookLucy, 5),

                        });
                        break;
                    case "getbook1":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter, 20),

                        });
                        break;
                    case "getbook2":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookInfinity, 20),
                        });
                        break;
                    case "getbook3":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookSuport, 20),
                        });
                        break;
                    case "getbook4":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_SkillBookCharacter_Rare, 20),
                        });
                        break;

                    //学习露西稀有技能
                    case "learnlucy":
                        {
                            __instance.CheatEnabled();
                            List<string> list = new List<string>();
                            list.Add("S_Lucy_25");
                            foreach (GDESkillData s in PlayData.ALLSKILLLIST)
                            {
                                if (s.Category.Key == GDEItemKeys.SkillCategory_LucySkill && s.User == "Lucy")
                                {
                                    list.Add(s.Key);
                                    Debug.Log($"{s.Key}");
                                }
                            }
                            List<Skill> list2 = new List<Skill>();
                            foreach (string key in list)
                            {
                                list2.Add(Skill.TempSkill(key, PlayData.BattleLucy, PlayData.TempBattleTeam));
                            }
                            FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list2, new SkillButton.SkillClickDel(new SkillBookLucy().SkillAdd), ScriptLocalization.System_Item.SkillAdd, true, true, true, true, false));
                            break;
                        }
                    //case "learnlucymore":
                    //    {
                    //        __instance.CheatEnabled();


                    //    }
                    //    break;
                    //进入力量试炼
                    case "strength2":
                        __instance.CheatEnabled();
                        FieldSystem.instance.BattleStart(new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_Queue_TrialofStrength2), __instance.StageData.BattleMap.Key, false, false, string.Empty, string.Empty);
                        break;
                    //进入力量试炼
                    case "strength1":
                        __instance.CheatEnabled();
                        FieldSystem.instance.BattleStart(new GDEEnemyQueueData(GDEItemKeys.EnemyQueue_Queue_TrialofStrength), __instance.StageData.BattleMap.Key, false, false, string.Empty, string.Empty);
                        break;
                    case "getstone":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Misc_Soul, 100)
                        });
                        break;

                    case "geteq0":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Equip && (i as Item_Equip).ItemClassNum == 0)
                                {
                                    string id = (i as Item_Equip).MyData.Key;
                                    //if(id != "OldBilbe" && id != "Sunmoonstarcurse_Quest" && id != "Sunmoonstarcurse")
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "geteq1":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Equip && (i as Item_Equip).ItemClassNum == 1)
                                {
                                    string id = (i as Item_Equip).MyData.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "geteq2":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Equip && (i as Item_Equip).ItemClassNum == 2)
                                {
                                    string id = (i as Item_Equip).MyData.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "geteq3":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Equip && (i as Item_Equip).ItemClassNum == 3)
                                {
                                    string id = (i as Item_Equip).MyData.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "geteq4":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Equip && (i as Item_Equip).ItemClassNum == 4)
                                {
                                    string id = (i as Item_Equip).MyData.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "getactive":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Active)
                                {
                                    string id = (i as Item_Active).Data.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "getpotion":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Potions)
                                {
                                    string id = (i as Item_Potions).Data.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }

                    case "geteqrb":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_RabbitMask),
                        });
                        break;

                    case "getpa1":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            int count = 0;
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Passive)
                                {
                                    count++;
                                    string id = (i as Item_Passive).MyData.Key;
                                    if(count>=1 && count<= 20)
                                        list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;

                        }
                    case "getpa2":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            int count = 0;
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Passive)
                                {
                                    count++;
                                    string id = (i as Item_Passive).MyData.Key;
                                    if (count >= 21 && count <= 40)
                                        list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;

                        }
                    case "getpa3":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            int count = 0;
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Passive)
                                {
                                    count++;
                                    string id = (i as Item_Passive).MyData.Key;
                                    if (count >= 41)
                                        list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;

                        }
                    //case "reward1":
                    //    {
                    //        __instance.CheatEnabled();
                    //        List<ItemBase> list6 = new List<ItemBase>();
                    //        list6.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    //        list6.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Jar, false));
                    //        list6.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_SmallReward, false));
                    //        list6.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Soul, 10));
                    //        InventoryManager.Reward(list6);
                    //        break;
                    //    }
                    //case "reward2":
                    //    {
                    //        __instance.CheatEnabled();
                    //        Item_Equip item = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    //        Item_Equip item_Equip = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    //        Item_Equip item2 = InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest4, false)[0] as Item_Equip;
                    //        List<ItemBase> list7 = new List<ItemBase>();
                    //        item_Equip.Curse = EquipCurse.NewCurse(item_Equip, GDEItemKeys.CurseList_Curse_loss_of_will);
                    //        list7.Add(item);
                    //        list7.Add(item_Equip);
                    //        list7.Add(item2);
                    //        InventoryManager.Reward(list7);
                    //        break;
                    //    }
                    //case "reward3":
                    //    {
                    //        __instance.CheatEnabled();
                    //        List<ItemBase> list8 = new List<ItemBase>();
                    //        for (int k = 0; k < 10; k++)
                    //        {
                    //            list8.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                    //        }
                    //        InventoryManager.Reward(list8);
                    //        InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Consume_GoldenApple));
                    //        break;
                    //    }
                    //case "reward4":
                    //    __instance.CheatEnabled();
                    //    InventoryManager.Reward(new List<ItemBase>
                    //    {
                    //        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Lucy_17)),
                    //        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_7)),
                    //        ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_Public_36))
                    //    });
                    //    break;
                    //所有boss专属掉落
                    case "getboss":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Passive_WitchRelic),
                            ItemBase.GetItem(GDEItemKeys.Item_Passive_JokerCard),
                            ItemBase.GetItem(GDEItemKeys.Item_Passive_TankRelic),

                            ItemBase.GetItem(GDEItemKeys.Item_Passive_TwinsRelic),

                            ItemBase.GetItem(GDEItemKeys.Item_Consume_TimeRelic),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_TimeRelic),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_TimeRelic),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_TimeRelic),

                            ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_BombClown_DropSkill)),
                            ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_BombClown_DropSkill)),
                            ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_BombClown_DropSkill)),
                            ItemBase.GetItem(new GDESkillData(GDEItemKeys.Skill_S_BombClown_DropSkill))

                        });
                        break;
                    //case "reward7":
                    //    __instance.CheatEnabled();
                    //    InventoryManager.Reward(new List<ItemBase>
                    //    {
                    //        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_7)
                    //    });
                    //    break;
                    //case "reward8":
                    //    __instance.CheatEnabled();
                    //    InventoryManager.Reward(new List<ItemBase>
                    //    {
                    //        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_8)
                    //    });
                    //    break;
                    //case "reward9":
                    //    __instance.CheatEnabled();
                    //    InventoryManager.Reward(new List<ItemBase>
                    //    {
                    //        ItemBase.GetItem(GDEItemKeys.Item_Misc_Record_9)
                    //    });
                    //    break;

                    case "entest":
                        __instance.CheatEnabled();
                        Debug.Log("치트");
                        PlayData.TSavedata.Party[1].SkillDatas[2].SKillExtended = Skill_Extended.DataToExtended(GDEItemKeys.SkillExtended_SkillEn_Azar_0);
                        PlayData.TSavedata.Party[0].SkillDatas[3].SKillExtended = Skill_Extended.DataToExtended(GDEItemKeys.SkillExtended_SkillEn_BattleStartDraw);
                        break;
                    case "PotionTest":
                        {
                            __instance.CheatEnabled();
                            List<ItemTest> list9 = new List<ItemTest>();
                            for (int l = 0; l < 12000; l++)
                            {
                                List<ItemBase> list10 = InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetScroll, false);
                                if (list10.Count == 0)
                                {
                                    Debug.Log("뽑기실패");
                                }
                                else
                                {
                                    ItemBase TempPo = list10[0];
                                    (TempPo as Item_Scroll).Debug = true;
                                    if (list9.Find((ItemTest a) => a.Name == TempPo.GetName) != null)
                                    {
                                        list9.Find((ItemTest a) => a.Name == TempPo.GetName).Num++;
                                    }
                                    else
                                    {
                                        list9.Add(new ItemTest
                                        {
                                            Name = TempPo.GetName,
                                            Num = 1
                                        });
                                    }
                                }
                            }
                            foreach (ItemTest itemTest in list9)
                            {
                                Debug.Log(itemTest.Name + " : " + itemTest.Num);
                            }
                            break;
                        }
                    case "getstage":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list11 = new List<ItemBase>();
                            list11.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Soul, 20));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Boss, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_Boss, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest2, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_Ancient_Chest2, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                            list11.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_R_GetPotion, false));
                            list11.Add(ItemBase.GetItem(GDEItemKeys.Item_Misc_Gold, 1500));
                            InventoryManager.Reward(list11);
                            break;
                        }
                   
                    //获得每种卷轴
                    case "getscroll":
                        {
                            __instance.CheatEnabled();
                            List<string> list13 = new List<string>();
                            InventoryManager.Reward(new List<ItemBase>
                            {
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Enchant, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Identify, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Item, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Mapping, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Midas, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Purification, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Quick, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Teleport, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Transfer, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Uncurse, 5),
                                ItemBase.GetItem(GDEItemKeys.Item_Scroll_Scroll_Vitality, 5)
                            });

                            break;
                        }
                    //知晓敌方意图
                    case "enemycast":
                        __instance.CheatEnabled();
                        PlayData.EnemyCastView = true;
                        break;
                    case "enemycastoff":
                        __instance.CheatEnabled();
                        PlayData.EnemyCastView = false;
                        break;
                    //使我方全体生命值减半
                    case "myhphalf":
                        __instance.CheatEnabled();
                        foreach (Character character in PlayData.TSavedata.Party)
                        {
                            character.Hp /= 2;
                        }
                        break;
                    //回到标题界面（不建议使用）
                    case "backmain":
                        __instance.CheatEnabled();
                        __instance.StartCoroutine(LoadManager.Ins.LoadFade("Main", LoadSceneMode.Single));
                        break;
                    //可以进入下一关
                    case "clearfast":
                        {
                            __instance.CheatEnabled();
                            List<StageRewardAnalysis> list14 = new List<StageRewardAnalysis>();
                            StageRewardAnalysis stageRewardAnalysis = new StageRewardAnalysis();
                            for (int m = 0; m < 50; m++)
                            {
                                list14.Add(new StageRewardAnalysis());
                                List<ItemBase> list15 = new List<ItemBase>();
                                for (int n = 0; n < __instance.Map.EventTileList.Count; n++)
                                {
                                    if (__instance.Map.EventTileList[n].Info.Type is TileTypes.Event)
                                    {
                                        list15.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_Object_S, false));
                                    }
                                    if (__instance.Map.EventTileList[n].Info.Type is BlockEvent)
                                    {
                                        list15.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_Object_L, false));
                                    }
                                    if (__instance.Map.EventTileList[n].Info.Type is Monster)
                                    {
                                        list15.AddRange(InventoryManager.RewardKey(GDEItemKeys.Reward_Battle, false));
                                    }
                                }
                                for (int num2 = 0; num2 < list15.Count; num2++)
                                {
                                    if (list15[num2].itemkey == GDEItemKeys.Item_Misc_Soul)
                                    {
                                        list14[m].Soul += (float)list15[num2].StackCount;
                                    }
                                    if (list15[num2].itemkey == GDEItemKeys.Item_Misc_Item_Key)
                                    {
                                        list14[m].Key += (float)list15[num2].StackCount;
                                    }
                                    if (list15[num2].itemkey == GDEItemKeys.Item_Misc_Gold)
                                    {
                                        list14[m].Gold += (float)list15[num2].StackCount;
                                    }
                                    if (list15[num2].itemkey == GDEItemKeys.Item_Consume_Bread)
                                    {
                                        list14[m].Heal += (float)list15[num2].StackCount;
                                    }
                                    if (list15[num2] is Item_Equip)
                                    {
                                        list14[m].Equip += 1f;
                                    }
                                }
                                stageRewardAnalysis += list14[m];
                                Debug.Log(string.Concat(new object[]
                                {
                                    m,
                                    "번째루프 골드 : ",
                                    list14[m].Gold,
                                    " 소울 : ",
                                    list14[m].Soul,
                                    " 힐스 : ",
                                    list14[m].Heal,
                                    " 키 : ",
                                    list14[m].Key,
                                    " 장비 : ",
                                    list14[m].Equip
                                }));
                            }
                            stageRewardAnalysis /= 50;
                            Debug.Log(string.Concat(new object[]
                            {
                                "총합 평균 : 골드 : ",
                                stageRewardAnalysis.Gold,
                                " 소울 : ",
                                stageRewardAnalysis.Soul,
                                " 힐스 : ",
                                stageRewardAnalysis.Heal,
                                " 키 : ",
                                stageRewardAnalysis.Key,
                                " 장비 : ",
                                stageRewardAnalysis.Equip
                            }));
                            //私有2
                            __instance.GetObjective = Traverse.Create(__instance).Field("_MaxObjecive").GetValue<int>();
                            __instance.CanNextStage = true;
                            break;
                        }
                    //控制台打印字符
                    case "retiling":
                        __instance.CheatEnabled();
                        __instance.Retiling();
                        break;
                    //未知
                    //case "tuto":
                    //    __instance.CheatEnabled();
                    //    TutorialSystem.TutorialFlag(10);
                    //    break;
                    //与当前关卡的boss战斗（额外战斗，不清空地图）
                    case "testboss":
                        __instance.CheatEnabled();
                        __instance.BossEnterFunc();
                        break;
                    case "getmoney":
                        __instance.CheatEnabled();
                        PlayData.Gold += 3000;
                        break;
                    //可以直接前往下一关
                    case "clear":
                        __instance.CheatEnabled();
                        __instance.GetObjective = Traverse.Create(__instance).Field("_MaxObjecive").GetValue<int>();
                        __instance.CanNextStage = true;
                        break;
                    //可以直接前往下一关
                    case "clearboss":
                        __instance.CheatEnabled();
                        __instance.CanNextStage = true;
                        break;
                    //case "mapfadein":
                    //    __instance.CheatEnabled();
                    //    FieldSystem.instance.StartCoroutine(UIManager.inst.FadeBlack_In(1f));
                    //    break;
                    //case "mapfadeout":
                    //    __instance.CheatEnabled();
                    //    FieldSystem.instance.StartCoroutine(UIManager.inst.FadeBlack_Out(1f));
                    //    break;
                    //case "skip2":
                    //    __instance.CheatEnabled();
                    //    break;
                    //获得部分装备
                    //case "equipget":
                    //    __instance.CheatEnabled();
                    //    InventoryManager.Reward(new List<ItemBase>
                    //    {
                    //        ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato),
                    //        ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato_0),
                    //        ItemBase.GetItem(GDEItemKeys.Item_Equip_SweetPotato_1),
                    //        ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb),
                    //        ItemBase.GetItem(GDEItemKeys.Item_Equip_FoxOrb_0),
                    //        ItemBase.GetItem(GDEItemKeys.Item_Passive_EndlessSoul)
                    //    });
                    //    break;

                    case "save":
                        __instance.CheatEnabled();
                        SaveManager.savemanager.ProgressOneSaveDebug();
                        Debug.Log("保存");
                        break;
                    case "load":
                        __instance.CheatEnabled();
                        UIManager.InstantiateActive(UIManager.inst.DebugLoad);
                        break;
                    //获得技能最后一击
                    case "getStrike":
                        __instance.CheatEnabled();
                        PlayData.TSavedata.LucySkills.Add(GDEItemKeys.Skill_S_Lucy_25);
                        ChildClear.Clear(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().LucySkillAlign);
                        foreach (string key2 in PlayData.TSavedata.LucySkills)
                        {
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().SkillView);
                            gameObject2.transform.SetParent(UIManager.inst.CharstatUI.GetComponent<CharStatV3>().LucySkillAlign);
                            Misc.UIInit(gameObject2);
                            gameObject2.GetComponent<SkillButtonMain>().Skillbutton.InputData(Skill.TempSkill(key2, PlayData.BattleDummy, PlayData.TempBattleTeam), null, false);
                        }
                        break;

                    //新增命令
                    //获得荒野钥匙
                    case "crimsonkey":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Misc_RWEnterItem));
                        break;
                    case "getring":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_Taegeukring),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_StarRing),
                            ItemBase.GetItem(GDEItemKeys.Item_Equip_CrescentsReflex)
                        });
                        break;
                }

                return false;
            }
        }

        //主要代码2
        [HarmonyPatch(typeof(BattleSystem))]
        class CheatPatch2
        {
            [HarmonyPatch(nameof(BattleSystem.CheatChack))]
            [HarmonyPrefix]
            static bool Prefix(BattleSystem __instance)
            {
                if(__instance.timescale.TimeS != Scale)
                {
                    __instance.timescale.TimeS = Scale;
                }
                string cheatChat = PlayData.CheatChat;
                switch (cheatChat)
                {
                    //获得四个烟雾弹
                    case "redherb":
                        __instance.CheatEnabled();
                        InventoryManager.Reward(new List<ItemBase>
                        {
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_RedWing),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_RedWing),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_RedWing),
                            ItemBase.GetItem(GDEItemKeys.Item_Consume_RedWing)
                        });
                        break;
                    //在手牌中加一张命运
                    case "fate":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Add(Skill.TempSkill(GDEItemKeys.Skill_S_StraightFlush_0, __instance.AllyList[0], __instance.AllyTeam), true);
                        break;
                    //使最右边的友方生命值变为0
                    case "myhp0":
                        __instance.CheatEnabled();
                        __instance.AllyList[0].Info.Hp = 0;
                        __instance.AllyList[0].Recovery = 0;
                        break;
                    case "de010":
                        __instance.CheatEnabled();
                        __instance.AllyList[3].Damage(__instance.EnemyList[0], 10, false, false, false, 0, false, false, false);
                        break;
                    //使所有敌人生命值减半
                    case "deahalf":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy in __instance.EnemyList)
                        {
                            battleEnemy.Info.Hp = battleEnemy.Info.Hp / 2;
                        }
                        break;
                    //对所有友军造成15点物理伤害
                    case "daa15":
                        __instance.CheatEnabled();
                        __instance.AllyList[0].Damage(__instance.DummyChar, 15, false, false, false, 0, false, false, false);
                        __instance.AllyList[1].Damage(__instance.DummyChar, 15, false, false, false, 0, false, false, false);
                        __instance.AllyList[2].Damage(__instance.DummyChar, 15, false, false, false, 0, false, false, false);
                        __instance.AllyList[3].Damage(__instance.DummyChar, 15, false, false, false, 0, false, false, false);
                        break;
                    case "getmoney":
                        __instance.CheatEnabled();
                        PlayData.Gold += 3000;
                        break;
                    //dot测试
                    case "pain1":
                        __instance.CheatEnabled();
                        __instance.AllyList[0].BuffAdd(GDEItemKeys.Buff_B_Maid_T_1, __instance.AllyList[0], false, 100, false, -1, false);
                        break;
                    case "pain2":
                        __instance.CheatEnabled();
                        __instance.AllyList[1].BuffAdd(GDEItemKeys.Buff_B_Maid_T_1, __instance.AllyList[0], false, 100, false, -1, false);
                        break;

                    case "dodtest1":
                        __instance.CheatEnabled();
                        __instance.EnemyList[0].BuffAdd(GDEItemKeys.Buff_B_LastResistance, __instance.EnemyList[0], false, 0, false, -1, false);
                        break;
                    case "dodtest2":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Chars[0].BuffAdd(GDEItemKeys.Buff_B_DeepWound, __instance.AllyTeam.Chars[0], false, 0, false, -1, false);
                        break;
                    //使所有敌方生命值变为1
                    case "clear1":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy2 in __instance.EnemyList)
                        {
                            battleEnemy2.Info.Hp = 1;
                        }
                        break;
                    //击败所有敌人
                    case "clear":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy3 in __instance.EnemyList)
                        {
                            battleEnemy3.Info.Hp = 0;
                            battleEnemy3.Dead(false);
                        }
                        break;
                    //对所有友军造成999伤害
                    case "daa999":
                        __instance.CheatEnabled();
                        foreach (BattleAlly battleAlly in __instance.AllyList)
                        {
                            battleAlly.Damage(__instance.AllyTeam.DummyCharAlly, 999, false, false, false, 0, false, false, false);
                        }
                        break;
                    //给予调试技能
                    case "debugskill1":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Add(Skill.TempSkill(GDEItemKeys.Skill_S_DebugSkill, __instance.AllyList[0], __instance.AllyTeam), true);
                        break;
                    case "debugskill2":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Add(Skill.TempSkill(GDEItemKeys.Skill_S_DebugSkill, __instance.AllyList[1], __instance.AllyTeam), true);
                        break;
                    case "debugskill3":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Add(Skill.TempSkill(GDEItemKeys.Skill_S_DebugSkill, __instance.AllyList[2], __instance.AllyTeam), true);
                        break;
                    case "debugskill4":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Add(Skill.TempSkill(GDEItemKeys.Skill_S_DebugSkill, __instance.AllyList[3], __instance.AllyTeam), true);
                        break;
                    //启动战斗计时器
                    case "time":
                        __instance.CheatEnabled();
                        PlayData.TSavedata.TrialofTimeDifficulty = 0;
                        PlayData.TSavedata.Timer = 600f;
                        if (PlayData.TSavedata.TrialofTimetier1reward == null)
                        {
                            PlayData.TSavedata.TrialofTimetier1reward = PlayData.GetHighTierReward();
                        }
                        if (PlayData.TSavedata.TrialofTimetier2reward == null)
                        {
                            PlayData.TSavedata.TrialofTimetier2reward = PlayData.GetHighTierReward();
                        }
                        if (PlayData.TSavedata.TrialofTimetier3reward == null)
                        {
                            PlayData.TSavedata.TrialofTimetier3reward = PlayData.GetHighTierReward();
                        }
                        __instance.BattleExtended.Add(new EventBattle_TrialofTime());
                        (__instance.BattleExtended[0] as EventBattle_TrialofTime).BattleStart(BattleSystem.instance);
                        break;
                    //播放一段音乐
                    case "soundtest":
                        __instance.CheatEnabled();
                        MasterAudio.PlaySound("06 Show Time (Boss Front)", 1f, null, 0f, null, null, false, false);
                        break;
                    //here
                    //case "main":
                    //    __instance.CheatEnabled();
                    //    List<BattleChar> global = Traverse.Create(__instance).Property("Global").GetValue<List<BattleChar>>();
                    //    __instance.StartCoroutine(LoseBattle(__instance, global));
                    //    break;

                    //抽一张牌
                    case "draw":
                        __instance.CheatEnabled();
                        __instance.AllyTeam.Draw();
                        break;
                    //抽牌至拥有7张手牌
                    case "drawto7":
                        __instance.CheatEnabled();
                        for (int j = 0; j < __instance.AllyTeam.Skills.Count; j++)
                        {
                            __instance.AllyTeam.Skills[j].Delete(false);
                            j--;
                        }
                        __instance.AllyTeam.Draw(7);
                        break;
                    //治疗所有友军99点
                    case "heal":
                        __instance.CheatEnabled();
                        foreach (BattleAlly battleAlly2 in __instance.AllyList)
                        {
                            battleAlly2.Recovery = battleAlly2.GetStat.maxhp;
                            battleAlly2.Heal(battleAlly2, 99f, false, true, null);
                        }
                        break;
                    //治疗所有敌军99点
                    case "enemyheal":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy4 in __instance.EnemyList)
                        {
                            battleEnemy4.Heal(battleEnemy4, 99f, false, true, null);
                        }
                        break;
                    //丢弃最上方的手牌
                    case "discard":
                        __instance.CheatEnabled();
                        if (__instance.AllyTeam.Skills.Count >= 1)
                        {
                            __instance.AllyTeam.Skills[0].MyButton.Waste();
                        }
                        break;
                    //丢弃所有
                    case "discardall":
                        __instance.CheatEnabled();
                        if (__instance.AllyTeam.Skills.Count >= 1)
                        {
                            for (int i = 0; i < __instance.AllyTeam.Skills.Count; i++)
                            {
                                __instance.AllyTeam.Skills[i].MyButton.Waste();
                                i--;
                            }
                        }
                        break;
                    //显示敌方技能和攻击目标（下回合开始）
                    case "targetview":
                        __instance.CheatEnabled();
                        PlayData.EnemyCastView = true;
                        break;
                    case "targetviewoff":
                        __instance.CheatEnabled();
                        PlayData.EnemyCastView = false;
                        break;
                    //丢弃所有手牌，抽一张牌
                    case "lian":
                        __instance.CheatEnabled();
                        for (int k = 0; k < __instance.AllyTeam.Skills.Count; k++)
                        {
                            __instance.AllyTeam.Skills[k].Delete(false);
                            k--;
                        }
                        __instance.AllyTeam.Draw();
                        break;
                    //对最右边的敌人造成100点不无视防御的伤害
                    case "de0100":
                        __instance.CheatEnabled();
                        __instance.EnemyList[0].Damage(__instance.AllyList[0], 100, false, false, false, 0, false, false, false);
                        break;
                    //未知
                    case "turn":
                        __instance.CheatEnabled();
                        foreach (BattleAlly battleAlly3 in __instance.AllyList)
                        {
                            battleAlly3.ActionCount = 1;
                            battleAlly3.Overload = 0;
                        }
                        __instance.AllyTeam.AP = __instance.AllyTeam.MAXAP;
                        __instance.ActWindow.Window.GetSkillData(__instance.AllyTeam);
                        __instance.AllyTeam.DiscardCount = 1;
                        break;
                    //对最左边的友方造成10点物理伤害
                    case "da010":
                        __instance.CheatEnabled();
                        __instance.AllyList[0].Damage(__instance.EnemyList[0], 10, false, false, false, 0, false, false, false);
                        break;
                    //对所有敌方造成100点不无视防御的伤害
                    case "dea100":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy5 in __instance.EnemyList)
                        {
                            battleEnemy5.Damage(__instance.AllyList[0], 100, false, false, false, 0, false, false, false);
                        }
                        break;
                    //未知
                    case "on":
                        __instance.CheatEnabled();
                        __instance.ActWindow.On = true;
                        Debug.Log("ON");
                        break;
                    //对所有敌人施加一层弱点识破
                    case "enemydebuff":
                        __instance.CheatEnabled();
                        foreach (BattleEnemy battleEnemy5 in __instance.EnemyList)
                        {
                            battleEnemy5.BuffAdd(GDEItemKeys.Buff_B_Hein_T_2, __instance.AllyList[0], false, 100, false, -1, false);
                        }
                        break;


                    //新增命令
                    //获得药水 修改：获得每种药水
                    case "getpotion":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Potions)
                                {
                                    string id = (i as Item_Potions).Data.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                    case "getactive":
                        {
                            __instance.CheatEnabled();
                            List<ItemBase> list6 = new List<ItemBase>();
                            foreach (ItemBase i in PlayData.ALLITEMLIST)
                            {
                                if (i is Item_Active)
                                {
                                    string id = (i as Item_Active).Data.Key;
                                    list6.Add(ItemBase.GetItem(id));
                                    Debug.Log($"{id}");
                                }
                            }
                            InventoryManager.Reward(list6);
                            break;
                        }
                }

                return false;
            }

            [HarmonyPatch(nameof(BattleSystem.CheatEnabled))]
            [HarmonyPostfix]
            static void Postfix()
            {
                PlayData.CheatChat = string.Empty;
            }
        }

    }
}
