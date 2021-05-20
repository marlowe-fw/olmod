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
                                UIManager.DestroyAll(false);
                                UIManager.DestroyAll(false);
                                NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);
                                MPInternet.MenuPassword = "127.0.0.1";
                                MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);
                                UIManager.m_menu_selection = 1;
                                MenuManager.m_menu_state = MenuState.MP_LOCAL_MATCH;
                                MenuManager.PlaySelectSound(1f);

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