﻿using HarmonyLib;
using Overload;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameMod
{
    /// <summary>
    ///    adds a config file per pilot in a way that allows keeping up backward compatibility
    ///    for settings that dont belong into .xprefsmod due to their size.
    ///           
    ///  Author:  luponix 
    ///  Created: 2021-06-03
    /// </summary>
    class ExtendedConfig
    {
        
        private const string file_extension = ".extendedconfig";
        private static List<string> unknown_sections;

        // Legacy autoselect file.
        public static string textFile = Path.Combine(Application.persistentDataPath, "AutoSelect-Config.txt");

        // On Game Loading or when selecting a different PILOT read or generate PILOT.extendedconfig
        [HarmonyPatch(typeof(PilotManager), "Select", new Type[] { typeof(string) })]
        internal class ExtendedConfig_PilotManager_Select
        {
            public static void Prefix(string name)
            {
                if( Network.isServer )
                {
                    Debug.Log("ExtendedConfig_PilotManager_Select called on the server");
                    return;
                }

                SetDefaultConfig();

                var loaded = false;

                if (!string.IsNullOrEmpty(name))
                {
                    string filepath = Path.Combine(Application.persistentDataPath, name + file_extension);
                    if (File.Exists(filepath))
                    {
                        ReadConfigData(filepath);
                        loaded = true;
                    }
                }

                if (!loaded) {
                    // Attempt to use autoselect from pre 0.4.1.
                    if (File.Exists(textFile)) {
                        Debug.Log("Extended config does not exist for pilot, attempting to load pre-0.4.1 autoselect.");
                        using (StreamReader sr = File.OpenText(textFile)) {
                            MPAutoSelection.PrimaryPriorityArray[0] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[1] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[2] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[3] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[4] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[5] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[6] = sr.ReadLine();
                            MPAutoSelection.PrimaryPriorityArray[7] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[0] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[1] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[2] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[3] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[4] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[5] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[6] = sr.ReadLine();
                            MPAutoSelection.SecondaryPriorityArray[7] = sr.ReadLine();
                            MPAutoSelection.PrimaryNeverSelect[0] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[1] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[2] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[3] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[4] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[5] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[6] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.PrimaryNeverSelect[7] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[0] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[1] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[2] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[3] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[4] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[5] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[6] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.SecondaryNeverSelect[7] = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.primarySwapFlag = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.secondarySwapFlag = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.swapWhileFiring = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.dontAutoselectAfterFiring = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.zorc = sr.ReadLine().ToLower() == "true";
                            MPAutoSelection.miasmic = sr.ReadLine().ToLower() == "true";
                        }

                        Section_AutoSelect.Set(true);
                    }
                }

                ApplyConfigData();
            }
        }

        // reads all lines and passes them to their respective functions to process them 
        // unknown sections get stored in ExtendedConfig.unknown_lines to reattach them to the end when saving
        private static void ReadConfigData(string filepath)
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                unknown_sections = new List<string>();
                List<string> current_section = new List<string>();
                string line = String.Empty;
                string current_section_id = "unknown";

                while ((line = sr.ReadLine()) != null)
                {

                    if (line.StartsWith("[SECTION:"))
                    {
                        if (known_sections.Contains(line))
                        {
                            current_section_id = line;
                        }
                        else
                        {
                            current_section_id = "unknown";
                            unknown_sections.Add(line);
                        }
                    }
                    else if (line.Equals("[/END]"))
                    {
                        if (!current_section_id.Equals("unknown"))
                        {
                            PassSectionToFunction(current_section, current_section_id);
                            current_section_id = "unknown";
                            current_section.Clear();
                        }
                        else
                        {
                            unknown_sections.Add(line);
                        }
                    }
                    else
                    {
                        if (current_section_id.Equals("unknown"))
                        {
                            unknown_sections.Add(line);
                        }
                        else
                        {
                            current_section.Add(line);
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Controls), "SaveControlData")]
        internal class ExtendedConfig_Controls_SaveControlData
        {
            public static void Postfix()
            {
                if (Network.isServer)
                {
                    Debug.Log("ExtendedConfig_Controls_SaveControlData called on the server");
                    return;
                }
                SaveActivePilot();
            }
        }

        [HarmonyPatch(typeof(PilotManager), "SavePreferences")]
        internal class ExtendedConfig_PilotManager_SavePreferences
        {
            public static void Postfix()
            {
                if (Network.isServer)
                {
                    Debug.Log("ExtendedConfig_Controls_SavePreferences called on the server");
                    return;
                }
                SaveActivePilot();
            }
        }

        [HarmonyPatch(typeof(PilotManager), "Create")]
        internal class ExtendedConfig_PilotManager_Create
        {
            public static void Prefix()
            {
                if (Network.isServer)
                {
                    Debug.Log("ExtendedConfig_PilotManager_Create called on the server");
                    return;
                }
                if ( string.IsNullOrEmpty(PilotManager.ActivePilot) )
                {
                    SetDefaultConfig();
                }
                SaveActivePilot();
            }

            public static void Postfix(string name, bool copy_prefs, bool copy_config)
            {
                if (Network.isServer)
                {
                    Debug.Log(" called on the server");
                    return;
                }
                if (!copy_prefs && !copy_config)
                {
                    SetDefaultConfig();
                    unknown_sections = new List<string>();
                }
                SaveActivePilot();
            }
        }

        [HarmonyPatch(typeof(Platform), "DeleteUserData")]
        internal class ExtendedConfig_Platform_DeleteUserData
        {
            static void Postfix(string filename)
            {
                if (Network.isServer)
                {
                    Debug.Log("ExtendedConfig_Platform_DeleteUserData called on the server");
                    return;
                }
                if (filename.EndsWith(".xprefs"))
                {
                    Platform.DeleteUserData(filename.Replace(".xprefs", file_extension));
                }
            }
        }

        [HarmonyPatch(typeof(Controls), "OnControllerConnected")]
        internal class ExtendedConfig_Controls_OnControllerConnected
        {
            static void Prefix()
            {
                if (!Network.isServer)
                {
                    PilotManager.Select(PilotManager.ActivePilot);
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        ///                           Add new Sections here:                                  ///
        /////////////////////////////////////////////////////////////////////////////////////////

        private static List<string> known_sections = new List<string> {
            "[SECTION: AUTOSELECT]",
            "[SECTION: JOYSTICKCURVE]",

        };


        private static void PassSectionToFunction(List<string> section, string section_name)
        {
            if (section_name.Equals(known_sections[0]))
            {
                Section_AutoSelect.Load(section);
            }
            if (section_name.Equals(known_sections[1]))
            {
                Section_JoystickCurve.Load(section);
            }


        }

        public static void SaveActivePilot()
        {
            if (!PilotManager.Exists(PilotManager.ActivePilot))
            {
                return;
            }
            try
            {
                string filepath = Path.Combine(Application.persistentDataPath, PilotManager.ActivePilot + file_extension);

                using (StreamWriter w = File.CreateText(filepath))
                {
                    w.WriteLine("[SECTION: AUTOSELECT]");
                    Section_AutoSelect.Save(w);
                    w.WriteLine("[/END]");

                    w.WriteLine("[SECTION: JOYSTICKCURVE]");
                    Section_JoystickCurve.Save(w);
                    w.WriteLine("[/END]");


                    if (unknown_sections != null)
                    {
                        foreach (string line in unknown_sections)
                        {
                            w.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error in ExtendedConfig.SaveActivePilot(): " + ex);
            }
        }

        public static void SetDefaultConfig()
        {
            Section_AutoSelect.Set();
            Section_JoystickCurve.SetDefault();

        }

        public static void ApplyConfigData()
        {
            Section_AutoSelect.ApplySettings();

        }



        /////////////////////////////////////////////////////////////////////////////////////////
        ///                           SECTION SPECIFIC FUNCTIONS:                             ///
        /////////////////////////////////////////////////////////////////////////////////////////

        public static string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        internal class Section_AutoSelect
        {
            public static Dictionary<string, string> settings;

            public static void Load(List<string> section)
            {
                settings = new Dictionary<string, string>();
                string l;
                foreach (string line in section)
                {
                    l = RemoveWhitespace(line);
                    string[] res = l.Split(':');
                    if (res.Length == 2)
                    {
                        settings.Add(res[0], res[1]);
                    }
                    else
                    {
                        Debug.Log("Error in ExtendedConfig.ProcessAutoSelectSection: unexpected line split: " + line + ", Setting Default Values.");
                        Set();
                        return;
                    }
                }
                ApplySettings();
            }

            public static void Save(StreamWriter w)
            {
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        if (setting.Key != null && setting.Value != null)
                        {
                            w.WriteLine("   " + setting.Key + ": " + setting.Value);
                        }
                    }
                }
            }

            // sets the values of the AutoSelect dictionary
            //  mirror = false   sets the default values
            //  mirror = true    sets the current MPAutoSelection values
            public static void Set(bool mirror = false)
            {
                settings = new Dictionary<string, string>();
                settings.Add("p_priority_0", mirror ? MPAutoSelection.PrimaryPriorityArray[0] : "THUNDERBOLT");
                settings.Add("p_priority_1", mirror ? MPAutoSelection.PrimaryPriorityArray[1] : "CYCLONE");
                settings.Add("p_priority_2", mirror ? MPAutoSelection.PrimaryPriorityArray[2] : "DRILLER");
                settings.Add("p_priority_3", mirror ? MPAutoSelection.PrimaryPriorityArray[3] : "IMPULSE");
                settings.Add("p_priority_4", mirror ? MPAutoSelection.PrimaryPriorityArray[4] : "FLAK");
                settings.Add("p_priority_5", mirror ? MPAutoSelection.PrimaryPriorityArray[5] : "CRUSHER");
                settings.Add("p_priority_6", mirror ? MPAutoSelection.PrimaryPriorityArray[6] : "LANCER");
                settings.Add("p_priority_7", mirror ? MPAutoSelection.PrimaryPriorityArray[7] : "REFLEX");
                settings.Add("s_priority_0", mirror ? MPAutoSelection.SecondaryPriorityArray[0] : "DEVASTATOR");
                settings.Add("s_priority_1", mirror ? MPAutoSelection.SecondaryPriorityArray[1] : "NOVA");
                settings.Add("s_priority_2", mirror ? MPAutoSelection.SecondaryPriorityArray[2] : "TIMEBOMB");
                settings.Add("s_priority_3", mirror ? MPAutoSelection.SecondaryPriorityArray[3] : "VORTEX");
                settings.Add("s_priority_4", mirror ? MPAutoSelection.SecondaryPriorityArray[4] : "HUNTER");
                settings.Add("s_priority_5", mirror ? MPAutoSelection.SecondaryPriorityArray[5] : "FALCON");
                settings.Add("s_priority_6", mirror ? MPAutoSelection.SecondaryPriorityArray[6] : "MISSILE_POD");
                settings.Add("s_priority_7", mirror ? MPAutoSelection.SecondaryPriorityArray[7] : "CREEPER");
                settings.Add("p_neverselect_0", mirror ? MPAutoSelection.PrimaryNeverSelect[0].ToString() : "false");
                settings.Add("p_neverselect_1", mirror ? MPAutoSelection.PrimaryNeverSelect[1].ToString() : "false");
                settings.Add("p_neverselect_2", mirror ? MPAutoSelection.PrimaryNeverSelect[2].ToString() : "false");
                settings.Add("p_neverselect_3", mirror ? MPAutoSelection.PrimaryNeverSelect[3].ToString() : "false");
                settings.Add("p_neverselect_4", mirror ? MPAutoSelection.PrimaryNeverSelect[4].ToString() : "false");
                settings.Add("p_neverselect_5", mirror ? MPAutoSelection.PrimaryNeverSelect[5].ToString() : "false");
                settings.Add("p_neverselect_6", mirror ? MPAutoSelection.PrimaryNeverSelect[6].ToString() : "false");
                settings.Add("p_neverselect_7", mirror ? MPAutoSelection.PrimaryNeverSelect[7].ToString() : "false");
                settings.Add("s_neverselect_0", mirror ? MPAutoSelection.SecondaryNeverSelect[0].ToString() : "false");
                settings.Add("s_neverselect_1", mirror ? MPAutoSelection.SecondaryNeverSelect[1].ToString() : "false");
                settings.Add("s_neverselect_2", mirror ? MPAutoSelection.SecondaryNeverSelect[2].ToString() : "false");
                settings.Add("s_neverselect_3", mirror ? MPAutoSelection.SecondaryNeverSelect[3].ToString() : "false");
                settings.Add("s_neverselect_4", mirror ? MPAutoSelection.SecondaryNeverSelect[4].ToString() : "false");
                settings.Add("s_neverselect_5", mirror ? MPAutoSelection.SecondaryNeverSelect[5].ToString() : "false");
                settings.Add("s_neverselect_6", mirror ? MPAutoSelection.SecondaryNeverSelect[6].ToString() : "false");
                settings.Add("s_neverselect_7", mirror ? MPAutoSelection.SecondaryNeverSelect[7].ToString() : "false");
                settings.Add("p_swap", mirror ? MPAutoSelection.primarySwapFlag.ToString() : "false");
                settings.Add("s_swap", mirror ? MPAutoSelection.secondarySwapFlag.ToString() : "false");
                settings.Add("dev_alert", mirror ? MPAutoSelection.zorc.ToString() : "true");
                settings.Add("reduced_hud", mirror ? MPAutoSelection.miasmic.ToString() : "false");
                settings.Add("swap_while_firing", mirror ? MPAutoSelection.swapWhileFiring.ToString() : "false");
                settings.Add("dont_autoselect_after_firing", mirror ? MPAutoSelection.dontAutoselectAfterFiring.ToString() : "false");
            }

            public static void ApplySettings()
            {
                try
                {
                    MPAutoSelection.PrimaryPriorityArray[0] = settings["p_priority_0"];
                    MPAutoSelection.PrimaryPriorityArray[1] = settings["p_priority_1"];
                    MPAutoSelection.PrimaryPriorityArray[2] = settings["p_priority_2"];
                    MPAutoSelection.PrimaryPriorityArray[3] = settings["p_priority_3"];
                    MPAutoSelection.PrimaryPriorityArray[4] = settings["p_priority_4"];
                    MPAutoSelection.PrimaryPriorityArray[5] = settings["p_priority_5"];
                    MPAutoSelection.PrimaryPriorityArray[6] = settings["p_priority_6"];
                    MPAutoSelection.PrimaryPriorityArray[7] = settings["p_priority_7"];
                    MPAutoSelection.SecondaryPriorityArray[0] = settings["s_priority_0"];
                    MPAutoSelection.SecondaryPriorityArray[1] = settings["s_priority_1"];
                    MPAutoSelection.SecondaryPriorityArray[2] = settings["s_priority_2"];
                    MPAutoSelection.SecondaryPriorityArray[3] = settings["s_priority_3"];
                    MPAutoSelection.SecondaryPriorityArray[4] = settings["s_priority_4"];
                    MPAutoSelection.SecondaryPriorityArray[5] = settings["s_priority_5"];
                    MPAutoSelection.SecondaryPriorityArray[6] = settings["s_priority_6"];
                    MPAutoSelection.SecondaryPriorityArray[7] = settings["s_priority_7"];
                    MPAutoSelection.PrimaryNeverSelect[0] = Convert.ToBoolean(settings["p_neverselect_0"]);
                    MPAutoSelection.PrimaryNeverSelect[1] = Convert.ToBoolean(settings["p_neverselect_1"]);
                    MPAutoSelection.PrimaryNeverSelect[2] = Convert.ToBoolean(settings["p_neverselect_2"]);
                    MPAutoSelection.PrimaryNeverSelect[3] = Convert.ToBoolean(settings["p_neverselect_3"]);
                    MPAutoSelection.PrimaryNeverSelect[4] = Convert.ToBoolean(settings["p_neverselect_4"]);
                    MPAutoSelection.PrimaryNeverSelect[5] = Convert.ToBoolean(settings["p_neverselect_5"]);
                    MPAutoSelection.PrimaryNeverSelect[6] = Convert.ToBoolean(settings["p_neverselect_6"]);
                    MPAutoSelection.PrimaryNeverSelect[7] = Convert.ToBoolean(settings["p_neverselect_7"]);
                    MPAutoSelection.SecondaryNeverSelect[0] = Convert.ToBoolean(settings["s_neverselect_0"]);
                    MPAutoSelection.SecondaryNeverSelect[1] = Convert.ToBoolean(settings["s_neverselect_1"]);
                    MPAutoSelection.SecondaryNeverSelect[2] = Convert.ToBoolean(settings["s_neverselect_2"]);
                    MPAutoSelection.SecondaryNeverSelect[3] = Convert.ToBoolean(settings["s_neverselect_3"]);
                    MPAutoSelection.SecondaryNeverSelect[4] = Convert.ToBoolean(settings["s_neverselect_4"]);
                    MPAutoSelection.SecondaryNeverSelect[5] = Convert.ToBoolean(settings["s_neverselect_5"]);
                    MPAutoSelection.SecondaryNeverSelect[6] = Convert.ToBoolean(settings["s_neverselect_6"]);
                    MPAutoSelection.SecondaryNeverSelect[7] = Convert.ToBoolean(settings["s_neverselect_7"]);
                    MPAutoSelection.primarySwapFlag = Convert.ToBoolean(settings["p_swap"]);
                    MPAutoSelection.secondarySwapFlag = Convert.ToBoolean(settings["s_swap"]);
                    MPAutoSelection.zorc = Convert.ToBoolean(settings["dev_alert"]);
                    MPAutoSelection.miasmic = Convert.ToBoolean(settings["reduced_hud"]);
                    MPAutoSelection.swapWhileFiring = Convert.ToBoolean(settings["swap_while_firing"]);
                    MPAutoSelection.dontAutoselectAfterFiring = Convert.ToBoolean(settings["dont_autoselect_after_firing"]);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error while parsing AutoSelect settings. missing entry " + ex + "\nSetting Default values");
                    Set();
                    ApplySettings();
                }
            }

        }


        internal class Section_JoystickCurve
        {
            public static List<Controller> controllers = new List<Controller>();

            public class Controller
            {
                public string name = "";
                public List<Axis> axes = new List<Axis>();

                public class Axis
                {
                    public Vector2[] curve_points = new Vector2[4];
                    public float[] curve_lookup = new float[200];

                    public Vector2[] CloneCurvePoints()
                    {
                        return new Vector2[] { 
                            new Vector2(curve_points[0].x,curve_points[0].y), 
                            new Vector2(curve_points[1].x,curve_points[1].y), 
                            new Vector2(curve_points[2].x,curve_points[2].y), 
                            new Vector2(curve_points[3].x,curve_points[3].y)
                        };

                    }
                }
            }

            public static void Load(List<string> section)
            {
                for (int i = 0; i < section.Count; i++)
                {
                    if (!string.IsNullOrEmpty(section[i]))
                    {
                        section[i] = RemoveWhitespace(section[i]);
                    }
                }

                try
                {
                    SetDefault();
                    int index = -1;
                    int.TryParse(section[++index], out int val);
                    int numControllers = val;
                    for (int i = 0; i < numControllers; i++)
                    {
                        string controllerName = section[++index];
                        int.TryParse(section[++index], out int val2);
                        int numAxes = val2;
                        if( i >= controllers.Count )
                        {
                            Controller c = new Controller();
                            c.name = controllerName;
                            for (int g = 0; g < numAxes; g++) c.axes.Add(new Controller.Axis());
                            controllers.Add(c);
                        }
                        for (int j = 0; j < numAxes; j++)
                        {
                            float value = 0f;
                            controllers[i].axes[j].curve_points = DefaultCurvePoints();
                            controllers[i].axes[j].curve_points[0].y = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 0f;
                            controllers[i].axes[j].curve_points[1].x = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 0.25f;
                            controllers[i].axes[j].curve_points[1].y = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 0.25f;
                            controllers[i].axes[j].curve_points[2].x = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 0.75f;
                            controllers[i].axes[j].curve_points[2].y = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 0.75f;
                            controllers[i].axes[j].curve_points[3].y = float.TryParse(section[++index], out value) && value >= 0f && value <= 1f ? value : 1f;

                            controllers[i].axes[j].curve_lookup = ExtendedConfig.Section_JoystickCurve.GenerateCurveLookupTable(controllers[i].axes[j].curve_points);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Error in ExtendedConfig.Section_JoystickCurve.Load:  " + ex + ", Setting Default Values.");
                    SetDefault();

                }
            }

            public static void Save(StreamWriter w)
            {
                try
                {
                    w.WriteLine(controllers.Count);
                    for (int i = 0; i < controllers.Count; i++)
                    {
                        w.WriteLine("   " + controllers[i].name);
                        w.WriteLine("   " + controllers[i].axes.Count);
                        for (int j = 0; j < controllers[i].axes.Count; j++)
                        {
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[0].y);
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[1].x);
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[1].y);
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[2].x);
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[2].y);
                            w.WriteLine("      " + controllers[i].axes[j].curve_points[3].y);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Error in ExtendedConfig.Section_JoystickCurve.Save(): \n" + ex);
                }
            }

            public static void SetDefault()
            {
                controllers.Clear();
                for (int i = 0; i < Controls.m_controllers.Count; i++)
                {
                    controllers.Add(new Controller
                    {
                        name = Controls.m_controllers[i].name,
                        axes = new List<Controller.Axis>()
                    });
                    for (int j = 0; j < Controls.m_controllers[i].m_axis_count; j++)
                    {
                        controllers[i].axes.Add(new Controller.Axis()
                        {
                            curve_points = DefaultCurvePoints(),
                            curve_lookup = GenerateCurveLookupTable(DefaultCurvePoints())
                        });
                    }
                }
            }

            public static Vector2[] DefaultCurvePoints()
            {
                return new Vector2[] { Vector2.zero, new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f), Vector2.one };
            }

            public static float[] GenerateCurveLookupTable(Vector2[] points)
            {
                if (points.Length != 4 || points[0] == null || points[1] == null || points[2] == null || points[3] == null)
                {
                    Debug.Log("ExtendedConfig.GenerateCurveLookupTable: invalid argument");
                    points = DefaultCurvePoints();
                }


                // generate initial curve
                Vector2[] curve = new Vector2[200];
                float t;
                for (int i = 0; i < 200; i++)
                {
                    t = 1f / 200f * i;
                    curve[i] = new Vector2(
                       JoystickCurveEditor.CubicBezierAxisForT(t, points[0].x, points[1].x, points[2].x, points[3].x),
                       JoystickCurveEditor.CubicBezierAxisForT(t, points[0].y, points[1].y, points[2].y, points[3].y)
                       );
                }

                // normalize the initial curve for slightly faster lookup and constant distribution
                float[] normalized = new float[200];
                for (int i = 0; i < 200; i++)
                {
                    float x = i * 0.005f;
                    int k = 1;
                    while (curve[k].x <= x && k < 199) k++;

                    normalized[i] = curve[k - 1].y + (x - curve[k - 1].x) / (curve[k].x - curve[k - 1].x) * (curve[k].y - curve[k - 1].y);
                }
                //string debug = "";
                //foreach (float f in normalized) debug += "," + f.ToString();
                //Debug.Log("NORMALIZED AXIS: \n" + debug);
                return normalized;
            }
        }


        
    }
}

