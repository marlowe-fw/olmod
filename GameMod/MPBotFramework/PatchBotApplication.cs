using Harmony;
using Overload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace GameMod.MPBotFramework
{

    

    public class BotControl
    {
        public static bool Enabled = false;
    }

    [HarmonyPatch(typeof(PlayerShip), "Awake")]
    class PMoveAndShrinkGameView
    {
        static void Postfix(PlayerShip __instance)
        {
            Camera shipCamera = __instance.GetComponentInChildren<Camera>(true);
            shipCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
    
    [HarmonyPatch(typeof(GameplayManager), "Initialize")]
    class PStartWindowed
    {

        static void Prefix() 
        {
            Screen.fullScreen = false;
        }

    }


        [HarmonyPatch(typeof(Controls), "UpdateDevice")]
        class PIgnoreControlsForGame1
        {
            private static readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));

            private static int a_count = 0;


            // Ignore all input, brutal - but works.
            static bool Prefix()
            {
                if (Input.anyKeyDown)
                {
                    foreach (KeyCode keyCode in keyCodes)
                    {
                        if (Input.GetKeyDown(keyCode))
                        {
                            PClearIntro.msg = "Key press: " + keyCode;

                            if (keyCode == KeyCode.A)
                            {
                                a_count++;
                                BotControl.Enabled = !BotControl.Enabled;
                                PClearIntro.msg1 = "BOT CONTROL: " + (BotControl.Enabled ? "ON" : "OFF") + a_count;
                            }

                            if (!BotControl.Enabled)
                            {
                                break;
                            }

                            if (keyCode == KeyCode.KeypadEnter)
                            {
                            //UIManager.DestroyAll(false);


                            //InternetMatch.Enabled = true;

                            // -- Switch to match setup (LAN)


                            /*
                            UIManager.DestroyAll(false);
                            MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);
                            MenuManager.m_mp_lan_match = true;
                            MenuManager.m_mp_private_match = true;
                            MenuManager.m_menu_micro_state = 4;
                            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);
                            MenuManager.PlaySelectSound(1f);
                            UIManager.CreateUIElement(UIManager.SCREEN_CENTER, 7000, UIElementType.MP_LOBBY MP_MATCH_SETUP, "LAN MATCH");
                            MenuManager.SetDefaultSelection(0);
                            MenuManager.m_menu_sub_state = MenuSubState.ACTIVE;
                            */

                            // --

                            // -- Create match outright (LAN)


                            MenuManager.m_mp_status = Loc.LS("CREATING LAN MATCH");
                            PrivateMatchDataMessage pmd = GetPrivateMatchDataMessage();
                            NetworkMatch.StartPrivateLobby(pmd);
                            UIManager.DestroyAll(false);



                            /*
                             // -----------------
                          

                            GameManager.m_game_state = GameManager.GameState.MENU;
                            //GameplayManager.GamePaused = true;


                            //UIManager.m_menu_selection = 1;
                            UIManager.DestroyAll(false);
                            MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);
                            MenuManager.m_menu_micro_state = 4; 
                            // create match
                                                                
                            //MenuManager.m_menu_sub_state = MenuSubState.INIT;


                            // TODO - menu is not loading correctly - are menu elements not populated?

                            //MenuManager.m_menu_micro_state = 0; // create match first step

                            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);

                            GameplayManager.IsMultiplayerActive = false;
                            MenuManager.m_mp_lan_match = true;
                            MenuManager.m_mp_private_match = true;

                            //MPInternet.Enabled
                            //MPInternet.MenuPassword = "127.0.0.1";
                            MenuManager.m_updating_pm_settings = false;


                            MenuManager.PlaySelectSound(1f);
                            UIManager.m_mouse_menu_selection = -1;


                            // -----------------
                            */


                            //UIManager.Update();

                            // ---

                            //var x = MenuState.MP_LOCAL_MATCH;
                            //UIManager.m_menu_selection = 7;
                            //MenuManager.m_mp_lan_match = true;
                            //(MenuManager.m_menu_micro_state == 0 && UIManager.m_menu_selection == 2) // create open match



                        }

                        //Debug.Log("KeyCode down: " + keyCode);
                        break;
                        }
                    }
                }




                return !BotControl.Enabled;
            }

            static PrivateMatchDataMessage GetPrivateMatchDataMessage() 
            {
                PrivateMatchDataMessage privateMatchDataMessage = new PrivateMatchDataMessage();
                privateMatchDataMessage.m_name = "spark"; // PilotManager.PilotName;
                privateMatchDataMessage.m_password = "127.0.0.1"; //MenuManager.mms_match_password;
                privateMatchDataMessage.m_match_mode = MatchMode.ANARCHY; //MenuManager.mms_mode;
                privateMatchDataMessage.m_num_players_to_start = 2;
                privateMatchDataMessage.m_max_players_for_match = 2; //MenuManager.mms_max_players;
                privateMatchDataMessage.m_addon_level_name_hash = "QUICK1.MP:9667D24A"; //string.Empty;

                privateMatchDataMessage.m_level_num = -1;

                            /*
                if (GameManager.MultiplayerMission.IsLevelAnAddon(MenuManager.mms_level_num))
                {
                if (isOnlineMatch)
                {
                    privateMatchDataMessage.m_level_num = 0;
                }
                else
                {
                    privateMatchDataMessage.m_level_num = -1;
                    privateMatchDataMessage.m_addon_level_name_hash = GameManager.MultiplayerMission.GetAddOnLevelIdStringHash(MenuManager.mms_level_num);
                }
                }
                else
                {
                privateMatchDataMessage.m_level_num = MenuManager.mms_level_num;
                }
                */
                privateMatchDataMessage.m_time_limit = MatchTimeLimit.NONE;//MenuManager.mms_time_limit;
                privateMatchDataMessage.m_score_limit = 0; //MenuManager.mms_score_limit;
                privateMatchDataMessage.m_respawn_time_seconds = 2; //MenuManager.mms_respawn_time;
                privateMatchDataMessage.m_respawn_shield_time_seconds = 2; //MenuManager.mms_respawn_invuln;
                privateMatchDataMessage.m_friendlyfire = false; //(MenuManager.mms_friendly_fire > 0);
                privateMatchDataMessage.m_show_enemy_names = MatchShowEnemyNames.NORMAL; //MenuManager.mms_show_names;
                privateMatchDataMessage.m_turn_speed_limit = 2; //MenuManager.mms_turn_speed_limit;
                privateMatchDataMessage.m_powerup_spawn = 2; // MenuManager.mms_powerup_spawn;
                privateMatchDataMessage.m_powerup_initial = 2; // MenuManager.mms_powerup_initial;
                privateMatchDataMessage.m_powerup_big_spawn = 1; //MenuManager.mms_powerup_big_spawn;
                privateMatchDataMessage.m_powerup_filter_bitmask = 0xffff; //RUtility.BoolArrayToBitmask(MenuManager.mms_powerup_filter);
                privateMatchDataMessage.m_force_loadout = 0;  // MenuManager.mms_force_loadout;
                privateMatchDataMessage.m_force_m1 = MissileType.FALCON;  //MenuManager.mms_force_m1;
                privateMatchDataMessage.m_force_m2 = MissileType.NUM; // MenuManager.mms_force_m2;
                privateMatchDataMessage.m_force_modifier1 = 4; //MenuManager.mms_force_modifier1;
                privateMatchDataMessage.m_force_modifier2 = 4; //MenuManager.mms_force_modifier2;
                privateMatchDataMessage.m_force_w1 = WeaponType.IMPULSE; //MenuManager.mms_force_w1;
                privateMatchDataMessage.m_force_w2 = WeaponType.NUM; //MenuManager.mms_force_w2;

                return privateMatchDataMessage;
            }

    }

        [HarmonyPatch(typeof(Controls), "MouseAim")]
        class PIgnoreControlsForGame3
        {
            // Ignore all input, brutal - but works.
            static bool Prefix()
            {
                return !BotControl.Enabled;
            }
        }

        [HarmonyPatch(typeof(Controls), "MouseAimCache")]
        class PIgnoreControlsForGame4
        {
            // Ignore all input, brutal - but works.
            static bool Prefix()
            {
                return !BotControl.Enabled;
            }
        }


        [HarmonyPatch(typeof(UIManager), "FindMousePosition")]
        class PIgnoreControlsForGame2
        {
            // Ignore all input brutal - but works.
            static bool Prefix()
            {
                return !BotControl.Enabled;
            }
        }



        [HarmonyPatch(typeof(GameManager), "OnGUI")]
        public class PClearIntro
        {
            private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
            private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

            private static readonly GUIStyle labelStyle = new GUIStyle(GUI.skin.label);

            private static bool initialized;



            public static string msg = "Key press: NONE";
            public static string msg1 = "BOT CONTROL: OFF";

            public static string msg2 = "STATE: NONE";

        private static Vector2 scrollViewVector = Vector2.zero;
            public  static string LogText = "";


        public static void DrawRect(Rect position, Color color, GUIContent content = null)
            {
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
                GUI.Box(position, content ?? GUIContent.none, textureStyle);
                GUI.backgroundColor = backgroundColor;
            }



            static void Postfix(GameManager __instance)
            {

                //GUI.DrawTexture(new Rect(-1f, -1f, Screen.width, Screen.height), Texture2D.blackTexture, ScaleMode.StretchToFill);
                if (!__instance.m_show_loading_screen)
                {
                    DrawRect(new Rect(0, 0, Screen.width / 2, Screen.height), Color.gray);
                    DrawRect(new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2), Color.gray);
                }

                if (!initialized)
                {
                    labelStyle.normal.textColor = Color.green;
                    initialized = true;
                }



                GUI.Label(new Rect(25, 25, 300, 30), msg, labelStyle);

                GUI.Label(new Rect(25, 55, 300, 30), msg1, labelStyle);

                msg2 = "";
                msg2 += $"UIManager.m_menu_selection: {UIManager.m_menu_selection}";
                msg2 += $"\nUIManager.m_mouse_menu_selection: {UIManager.m_mouse_menu_selection}";
                msg2 += $"\nMenuManager.m_menu_state: {MenuManager.m_menu_state}";
                msg2 += $"\nMenuManager.m_menu_sub_state: {MenuManager.m_menu_sub_state}";
                msg2 += $"\nMenuManager.m_menu_micro_state: {MenuManager.m_menu_micro_state}";
                msg2 += $"\nMenuManager.m_menu_next_menu_state: {MenuManager.m_next_menu_state}";
                msg2 += $"\nMenuManager.m_mp_lan_match: {MenuManager.m_mp_lan_match}";


                GUI.Label(new Rect(25, 85, 900, 350), msg2, labelStyle);

                scrollViewVector = GUI.BeginScrollView(new Rect(25, 485, 900, 400), scrollViewVector, new Rect(0, 0, 900, 900));

                // Put something inside the ScrollView
                LogText = GUI.TextArea(new Rect(0, 0, 900, 900), LogText);

                // End the ScrollView
                GUI.EndScrollView();


    }
        }

        /*
        [HarmonyPatch(typeof(GameManager), "Update")]
        class PIgnoreControlsForGame
        {
            static void Prefix()
            {
                Controls.ClearInputs();
            }

            static void Postfix()
            {
                Controls.ClearInputs();
            }

        }*/



    }