using BepInEx;
using HarmonyLib;
using Gunfiguration;
using UnityEngine;

namespace ControllerAimer
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInDependency("pretzel.etg.gunfig")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class AimerModule : BaseUnityPlugin
    {
        public const string GUID = "kleirof.etg.controlleraimer";
        public const string NAME = "Controller Aimer";
        public const string VERSION = "1.2.0";
        public const string TEXT_COLOR = "#FF7F50";

        internal static Gunfig gunfig = null;

        internal const string remoteBulletsFixStr = "Remote Bullets Fix";
        internal const string noAimingBarrelsStr = "No Aiming Barrels";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			Harmony harmony = new Harmony(GUID);
			harmony.PatchAll();

            InitializeGunfig();
        }

        public static void Log(string text, string color = "FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }

        internal static void InitializeGunfig()
        {
            gunfig = Gunfig.Get("Controller Aimer".WithColor(Color.white));

            gunfig.AddToggle(key: remoteBulletsFixStr, label: GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE ?
                "远程子弹修复" :
                remoteBulletsFixStr, enabled: true);
            gunfig.AddToggle(key: noAimingBarrelsStr, label: GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE ?
                "不瞄准桶" :
                noAimingBarrelsStr, enabled: true);
        }
    }
}
