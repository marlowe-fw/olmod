﻿using Harmony;
using Overload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameMod.MPBotFramework
{
    // detect "frametime" "cheat code"
    [HarmonyPatch(typeof(PlayerShip), "Awake")]
    class PMoveAndShrinkGameView
    {
        static void Prefix(PlayerShip __instance)
        {
            Camera shipCamera = __instance.GetComponentInChildren<Camera>(true);
            shipCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }

    [HarmonyPatch(typeof(GameManager), "Awake")]
    class PStartWindowed
    {
        static void Prefix()
        {
            //Screen.SetResolution(Screen.width, Screen.height, false);
            Screen.fullScreen = false;
        }
    }


    [HarmonyPatch(typeof(Controls), "UpdateDevice")]
    class PIgnoreControlsForGame1
    {

        // Ignore all input, brutal - but works.
        static bool Prefix()
        {

            return false;
        }
    }

    [HarmonyPatch(typeof(Controls), "MouseAim")]
    class PIgnoreControlsForGame3
    {
        // Ignore all input, brutal - but works.
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(Controls), "MouseAimCache")]
    class PIgnoreControlsForGame4
    {
        // Ignore all input, brutal - but works.
        static bool Prefix()
        {
            return false;
        }
    }


    [HarmonyPatch(typeof(UIManager), "FindMousePosition")]
    class PIgnoreControlsForGame2
    {
        // Ignore all input brutal - but works.
        static bool Prefix()
        {
            return false;
        }
    }



    [HarmonyPatch(typeof(GameManager), "OnGUI")]
    class PClearIntro
    {
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
        private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

        private static readonly GUIStyle labelStyle = new GUIStyle(GUI.skin.label);

        private static bool initialized;

        private static string msg = "Key press: NONE";

        private static readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));

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
                DrawRect(new Rect(0, 0, Screen.width/2, Screen.height), Color.gray);
                DrawRect(new Rect(Screen.width / 2 , Screen.height / 2, Screen.width / 2, Screen.height / 2), Color.gray);
            }

            if (!initialized)
            {
                labelStyle.normal.textColor = Color.green;
                initialized = true;
            }

            

            if (Input.anyKeyDown)
            {
                // TODO: read own input
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        msg = "Key press: " + keyCode;
                        //Debug.Log("KeyCode down: " + keyCode);
                        break;
                    }
                }
            }


            GUI.Label(new Rect(25, 25, 100, 30), msg, labelStyle);


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