using Harmony;
using Overload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace GameMod.MPBotFramework
{

    public class BotApplication
    {
        public static bool BotControlEnabled = false;

        private static readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));
        private static int a_count = 0;


        public BotApplication() 
        {}


        public void HandleInput()
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
                            BotApplication.BotControlEnabled = !BotApplication.BotControlEnabled;
                            PClearIntro.msg1 = "BOT CONTROL: " + (BotApplication.BotControlEnabled ? "ON" : "OFF") + a_count;
                        }

                        if (!BotApplication.BotControlEnabled)
                        {
                            break;
                        }


                        if (keyCode == KeyCode.KeypadMinus)
                        {
                            Action<string, string> callback = delegate (string error, string player_id)
                            {
                                if (error != null)
                                {
                                    NetworkMatch.SetPlayerId("00000000-0000-0000-0000-000000000000");
                                }
                                else
                                {
                                    NetworkMatch.SetPlayerId(player_id);
                                }
                            };
                            NetworkMatch.GetMyPlayerId(PilotManager.PilotName, callback);
                        }


                        if (keyCode == KeyCode.KeypadPlus)
                        {
                            MenuManager.m_mp_ready_to_start = true;
                            //MenuManager.UIPulse(1f);
                            //MenuManager.PlayCycleSound(1f, 1f);
                            NetworkMatch.SetLobbyStartNowVote(MenuManager.m_mp_ready_to_start);
                            MenuManager.m_mp_ready_vote_timer = 1.25f;

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

                            // -- Create match outright (online)

                            /*
                            MPInternet.OldEnabled = true;
                            MenuManager.m_mp_lan_match = true;
                            MenuManager.m_mp_private_match = true;
                            UIManager.DestroyAll(false);
                            MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);
                            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);
                            MenuManager.PlaySelectSound(1f);*/

                            /*
                            MPInternet.Enabled = true;
                            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);
                            MPInternet.MenuPassword = "167.71.41.143"; // Frankfurt 1
                            MenuManager.m_mp_lan_match = true;
                            MenuManager.m_mp_private_match = true;
                            //MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);
                            MenuManager.m_game_paused = false;

                            //MenuManager.m_menu_sub_state = MenuSubState.ACTIVE;
                            MenuManager.m_menu_micro_state = 0;

                            MenuManager.m_mp_ready_to_start = false;
                            MenuManager.m_mp_level_vote = -1;
                            MenuManager.m_mp_cst_timer = 3f;
                            MenuManager.m_mp_ready_vote_timer = 0f;*/


                            MenuManager.m_mp_lan_match = true;
                            MenuManager.m_mp_private_match = true;
                            //UIManager.DestroyAll(false);
                            //MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);


                            MenuManager.m_updating_pm_settings = false;

                            //MenuManager.ChangeMenuState(MenuState.MP_PRE_MATCH_MENU, false);

                            MPInternet.Enabled = true;
                            MPInternet.MenuPassword = "167.71.41.143"; // Frankfurt 1
                            MPInternet.MenuIPAddress = "167.71.41.143";
                            MPInternet.ServerAddress = IPAddress.Parse("167.71.41.143");

                            InternetMatchAccessor.Enabled = true;
                            InternetMatchAccessor.ServerAddress = MPInternet.ServerAddress;

                            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);

                            MenuManager.m_mp_status = Loc.LS("CREATING INTERNET MATCH");

                            PrivateMatchDataMessage pmd = BotMultiplayerAccess.GetPrivateMatchDataMessage();
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




        }

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

        class InternetMatchAccessor
        {
            private static FieldInfo fld_ServerAddress = typeof(GameManager).Assembly.GetType("InternetMatch").GetField("ServerAddress", BindingFlags.Static | BindingFlags.Public);
            private static FieldInfo fld_Enabled = typeof(GameManager).Assembly.GetType("InternetMatch").GetField("Enabled", BindingFlags.Static | BindingFlags.Public);

            public static IPAddress ServerAddress
            {
                get { return (IPAddress)fld_ServerAddress.GetValue(null); }
                set { fld_ServerAddress.SetValue(null, value); }
            }

            public static bool Enabled
            {
                get { return (bool)fld_Enabled.GetValue(null); }
                set { fld_Enabled.SetValue(null, value); }
            }



        }


        [HarmonyPatch(typeof(Controls), "UpdateDevice")]
        class PIgnoreControlsForGame1
        {
            // Ignore all input, brutal - but works.
            static bool Prefix()
            {

                return !BotApplication.BotControlEnabled;
            }


        }

        [HarmonyPatch(typeof(Controls), "MouseAim")]
        class PIgnoreControlsForGame3
        {
            // Ignore all input, brutal - but works.
            static bool Prefix()
            {
                return !BotApplication.BotControlEnabled;
            }
        }

        [HarmonyPatch(typeof(Controls), "MouseAimCache")]
        class PIgnoreControlsForGame4
        {
            // Ignore all input, brutal - but works.
            static bool Prefix()
            {
                return !BotApplication.BotControlEnabled;
            }
        }


        [HarmonyPatch(typeof(UIManager), "FindMousePosition")]
        class PIgnoreControlsForGame2
        {
            // Ignore all input brutal - but works.
            static bool Prefix()
            {
                return !BotApplication.BotControlEnabled;
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

            private static Vector2 scrollViewVector = new Vector2(0, Mathf.Infinity); // Vector2.zero;

            public static string LogTextExternal = "";
            public static string LogText = "";


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
                msg2 += $"\nMenuManager.m_mp_statu: {MenuManager.m_mp_status}";


                GUI.Label(new Rect(25, 85, 900, 350), msg2, labelStyle);

                scrollViewVector = GUI.BeginScrollView(new Rect(25, 525, 900, 400), scrollViewVector, new Rect(0, 0, 900, 400));



                // Put something inside the ScrollView
                PCaptureNetworkLog.Log = GUI.TextArea(new Rect(0, 0, 900, 900), PCaptureNetworkLog.Log);

                // End the ScrollView
                GUI.EndScrollView();


            }
        }

        [HarmonyPatch(typeof(NetworkMatch), "TLog")]
        public class PCaptureNetworkLog
        {

            public static string Log = "";

            static void Postfix(string msg, params object[] args)
            {
                if (args?.Length > 0)
                {
                    msg = string.Format(msg, args);
                }
                Log += $"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}: {msg}\n";

                /*
                if (args == null || args.Length == 0)
                {
                    PClearIntro.LogTextExternal += "\n" +
                    string.Format("{0}: {1}", new object[]
                    {
                    DateTime.Now.ToString(),
                    msg
                    });
                }
                else
                {
                    PClearIntro.LogTextExternal += "\n" +
                    string.Format("{0}: {1}", new object[]
                    {
                    DateTime.Now.ToString(),
                    string.Format(msg, args)
                    });
                }*/

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