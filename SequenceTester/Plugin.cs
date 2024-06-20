using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace SequenceTester
{
    [BepInPlugin("lammas123.SequenceTester", "SequenceTester", MyPluginInfo.PLUGIN_VERSION)]
    public class SequenceTester : BasePlugin
    {
        public override void Load()
        {
            SequencedDrop.Load();
            Harmony.CreateAndPatchAll(typeof(Patches));
            Log.LogInfo($"Loaded [{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION}]");
        }
    }
}