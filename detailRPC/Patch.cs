using Discord;
using HarmonyLib;
using RDTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace detailRPC
{
    public static class Patch
    {
        public static bool isdeath, isoverload, isclear, auto = false;
        [HarmonyPatch(typeof(DiscordController),"UpdatePresence")]
        public static class RPPatch
        {
            private static String Validate(String s)
            {
                if (s.Length <= 60)
                {
                    return s;
                }
                return s.Substring(0, 57) + "...";
            }
            public static bool Prefix(DiscordController __instance, Discord.Discord ___discord)
            {
                //Main.Logger.Log("RPPatch Working");
                if (Main.isplaying && ___discord != null)
                {
                    if (ADOBase.sceneName == GCNS.sceneLevelSelect)
                        return true;
                    if (ADOBase.sceneName == "scnCLS")
                        return true;
                    String text = String.Empty;
                    String text2 = String.Empty;
                    String text3 = String.Empty;

                    bool isLevelEditor = Main.ReleaseNumber >= 94 ? (bool)Main.latestisLevelEditorProperty.GetValue(null) : (bool)Main.isLevelEditorProperty.GetValue(null);
                    scnEditor editor = Main.ReleaseNumber >= 94 ? (scnEditor)Main.latesteditorProperty.GetValue(null) : (scnEditor)Main.editorProperty.GetValue(null);
                    bool isEditingLevel = Main.ReleaseNumber >= 94 ? (bool)Main.latestisEditingLevelProperty.GetValue(null) : (bool)Main.isEditingLevelProperty.GetValue(null);

                    if (scrController.instance != null && isLevelEditor)
                    {
                        string text4 = editor.levelData.fullCaption;
                        if (GCS.standaloneLevelMode)
                        {
                            text2 = RDString.Get("discord.playing", null);
                            if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (isEditingLevel ? editor.playbackSpeed : 1f)), 1.0))
                            {
                                string str = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                                {
                                    {
                                        "multiplier",
                                        scrConductor.instance.song.pitch.ToString("0.0#")
                                    }
                                });
                                text4 = text4 + " (" + str + ")";
                            }
                            text3 = text4;
                        }
                        else
                        {
                            text2 = RDString.Get("discord.inLevelEditor", null);
                            if (!editor.customLevel.levelPath.IsNullOrEmpty())
                            {
                                text3 = RDString.Get("discord.editedLevel", new Dictionary<string, object>
                                {
                                    {
                                        "level",
                                        text4
                                    }
                                });
                            }
                        }
                    }
                    else if (scrController.instance != null && scrController.instance.gameworld)
                    {
                        string text5 = ADOBase.GetLocalizedLevelName(ADOBase.sceneName);
                        if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (isEditingLevel ? editor.playbackSpeed : 1f)), 1.0))
                        {
                            string str2 = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                            {
                                {
                                    "multiplier",
                                    scrConductor.instance.song.pitch.ToString("0.0#")
                                }
                            });
                            text5 = text5 + " (" + str2 + ")";
                        }
                        text2 = RDString.Get("discord.playing", null);
                        text3 = text5;
                        text = text5;
                    }
                    text = Validate(text);
                    text3 = Validate(text3);
                    text2 = Validate(text2);
                    Activity activity = default(Activity);
                    if (text2.IsNullOrEmpty())
                    {
                        return true;
                    }
                    if (!scrController.instance.paused && !RDC.auto && (!(Patch.isdeath || Patch.isoverload) || scrController.instance.noFail))
                    {
                        if (!scrController.instance.noFail)
                        {
                            if (GCS.difficulty == Difficulty.Lenient)
                                activity.Details = text2 + " / (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨" : "Lenient") + ")";
                            else if (GCS.difficulty == Difficulty.Normal)
                                activity.Details = text2 + " / (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통" : "Normal") + ")";
                            else if (GCS.difficulty == Difficulty.Strict)
                                activity.Details = text2 + " / (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격" : "Strict") + ")";
                        }
                        else
                        {
                            if (GCS.difficulty == Difficulty.Lenient)
                                activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨-실패 방지" : "Lenient-No Fail") + ")";
                            else if (GCS.difficulty == Difficulty.Normal)
                                activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통-실패 방지" : "Normal-No Fail") + ")";
                            else if (GCS.difficulty == Difficulty.Strict)
                                activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격-실패 방지" : "Strict-No Fail") + ")";
                            if (!isEditingLevel)
                            {
                                text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                            }
                        }
                    }
                    else if (scrController.instance.paused)
                        activity.Details = text2;
                    else if (RDC.auto)
                    {
                        activity.Details = text2 + " / (Auto)";
                        auto = true;
                    }
                    else if (Patch.isdeath)
                        activity.Details = text2 + " / (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "죽음" : "Death") + ")";
                    else if (Patch.isoverload)
                        activity.Details = text2 + " / (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "과부하" : "Overload") + ")";
                    if (!RDC.auto)
                        auto = false;

                    activity.State = text3;
                    activity.Assets.LargeImage = "planets_icon_stars";
                    activity.Assets.LargeText = text;
                    Activity activity2 = activity;
                    ___discord.GetActivityManager().UpdateActivity(activity2, delegate (Result result)
                    {
                        if (result != Result.Ok)
                        {
                            RDBaseDll.printem(result.ToString());
                        }
                    });
                    DiscordController.shouldUpdatePresence = false;
                    return false;
                }
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"PlayerControl_Update")]
    internal static class Death
    {
        [HarmonyPatch(typeof(scrController),"FailAction")]
        private static void Prefix(bool overload = false)
        {
            if (!overload)
                Patch.isdeath = true;
            else
                Patch.isoverload = true;
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scrCountdown), "ShowGetReady")]
    public static class StartLoadingPatcher
    {
        public static void Postfix()
        {
            Patch.isdeath = false;
            Patch.isoverload= false;
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scrController),"Countdown_Update")]
    public class EditorStartLoadingPatcher
    {
        public static void Prefix()
        {
            if (ADOBase.sceneName == GCNS.sceneEditor && (Patch.isdeath || Patch.isoverload))
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(scnEditor),"Play")]
    public static class PlayPatch
    {
        public static void Prefix()
        {
            Patch.isdeath = false;
            Patch.isoverload = false;
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scnEditor),"TogglePause")]
    public static class EditorPausePatch
    {
        public static void Prefix()
        {
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scnEditor),"ToggleAuto")]
    public static class EditorAutoPatch
    {
        public static void Prefix()
        {
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scrController),"Checkpoint_Enter")]
    public static class CheckpointEnter
    {
        public static void Postfix()
        {
            Patch.isdeath = false;
            Patch.isoverload = false;
            DiscordController.shouldUpdatePresence = true;
        }
    }

    [HarmonyPatch(typeof(scrController),"OnLandOnPortal")]
    public static class ClearPatch
    {
        public static void Postfix(scrController __instance)
        {
            if (__instance.gameworld)
            {
                Patch.isclear = true;
            }
        }
    }
}
