using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using static SequenceTester.SequencedDrop;

namespace SequenceTester
{
    internal static class Patches
    {
        // Anti Bepinex detection (thanks o7Moon https://github.com/o7Moon/CrabGame.AntiAntiBepinex)
        [HarmonyPatch(typeof(EffectManager), nameof(EffectManager.Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0))] // Ensures effectSeed is never set to 4200069 (if it is, modding has been detected)
        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.Method_Private_Void_0))] // Ensures connectedToSteam stays false (true means modding has been detected)
        //[HarmonyPatch(typeof(SnowSpeedModdingDetector), nameof(SnowSpeedModdingDetector.Method_Private_Void_0))] // Would ensure snowSpeed is never set to Vector3.zero, but it is immediately set back to Vector3.one due to an accident on Dani's part lol
        [HarmonyPrefix]
        public static bool PreBepinexDetection() => false;

        [HarmonyPatch(typeof(GameModePractice), nameof(GameModePractice.Init))]
        [HarmonyPostfix]
        internal static void PostGameModePracticeInit() => playing = false;

        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPostfix]
        internal static void PostServerSendSendChatMessage(ulong param_0, string param_1)
        {
            if (!SteamManager.Instance.IsLobbyOwner() || param_0 <= 1) return;
            if (param_1.StartsWith("!seq"))
            {
                if (LobbyManager.Instance.gameMode != GameModeManager.Instance.practiceMode)
                {
                    ServerSend.SendChatMessage(1, "You can only play sequences in practice mode.");
                    return;
                }
                if (BlockDropBlockManager.Instance == null)
                {
                    ServerSend.SendChatMessage(1, "You cannot play a sequence on this map.");
                    return;
                }
                if (playing)
                {
                    ServerSend.SendChatMessage(1, "A sequence is already being played.");
                    return;
                }

                int split = param_1.IndexOf(' ');
                if (split == -1) return;
                string args = param_1[(split + 1)..];

                foreach (Sequence sequence in sequences)
                    if (sequence.name.ToLower().StartsWith(args))
                    {
                        BlockDropBlockManager.Instance.StartCoroutine(ProcessSequence(sequence));
                        return;
                    }
                ServerSend.SendChatMessage(1, $"No sequence matched: {args}");
            }
            else if (param_1 == "!rel")
            {
                Load();
                ServerSend.SendChatMessage(1, "Reloaded sequences.");
            }
            else if (param_1 == "!res")
                ServerSend.StartGame();
        }
    }
}