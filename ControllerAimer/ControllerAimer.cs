using BepInEx;
using HarmonyLib;

namespace ControllerAimer
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class AimerModule : BaseUnityPlugin
    {
        public const string GUID = "kleirof.etg.controlleraimer";
        public const string NAME = "Controller Aimer";
        public const string VERSION = "1.1.3";
        public const string TEXT_COLOR = "#FF7F50";

		public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			Harmony harmony = new Harmony(GUID);
			harmony.PatchAll();
		}

        public static void Log(string text, string color = "FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
	}
}
