using BepInEx;
using System;
using System.Reflection;
using UnityEngine;
using MonoMod.RuntimeDetour;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace ControllerAimer
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class AimerModule : BaseUnityPlugin
    {
        public const string GUID = "kleirof.etg.controlleraimer";
        public const string NAME = "Controller Aimer";
        public const string VERSION = "1.1.2";
        public const string TEXT_COLOR = "#FF7F50";

		public class AimerPatches
		{
			[HarmonyILManipulator, HarmonyPatch(typeof(GuidedBulletsPassiveItem), nameof(GuidedBulletsPassiveItem.PreMoveProjectileModifier))]
			public static void HandleKnockbackPatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.Before,
					x => x.MatchLdloc(3),
					x => x.MatchLdloc(2),
					x => x.MatchLdloc(4),
					x => x.MatchCall("BraveTime", "get_DeltaTime"),
					x => x.MatchMul()
					))
				{
					crs.Index += 2;
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.Emit(OpCodes.Ldarg_1);
                    crs.Emit(OpCodes.Ldloc_0);
                    crs.Emit(OpCodes.Ldloc_1);
                    crs.EmitDelegate<Func<float, GuidedBulletsPassiveItem, Projectile, BraveInput, Vector2, float>>
                        ((target, self, p, instanceForPlayer, vector) =>
                    {
                        if (!(instanceForPlayer.IsKeyboardAndMouse(false)
                                || instanceForPlayer.ActiveActions == null)
                                && vector == Vector2.zero)
                        {
                            float num1 = 0f;
                            AIActor nearestEnemy = self.Owner.CurrentRoom.GetNearestEnemy(self.Owner.CenterPosition, out num1, true, false);
							if (nearestEnemy)
							{
								vector = nearestEnemy.CenterPosition - p.specRigidbody.UnitCenter;
								return vector.ToAngle();
							}
							else
								return self.Owner.m_currentGunAngle;
                        }
                        else
                            return target;
                    });
				}
			}

			[HarmonyILManipulator, HarmonyPatch(typeof(InputGuidedProjectile), nameof(InputGuidedProjectile.Move))]
			public static void MovePatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.Before,
					x => x.MatchLdloc(4),
					x => x.MatchLdloc(3),
					x => x.MatchLdarg(0),
					x => x.MatchLdfld<InputGuidedProjectile>("trackingSpeed"),
					x => x.MatchCall("BraveTime", "get_DeltaTime"),
					x => x.MatchMul()
					))
				{
					crs.Index += 2;
					crs.Emit(OpCodes.Ldarg_0);
					crs.Emit(OpCodes.Ldloc_1);
					crs.Emit(OpCodes.Ldloc_2);
					crs.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<float, InputGuidedProjectile, BraveInput, Vector2, float>>
						((target, self, instanceForPlayer, vector) =>
						{
							if (!(instanceForPlayer.IsKeyboardAndMouse(false)
									|| instanceForPlayer.ActiveActions == null)
									&& vector == Vector2.zero)
							{
								float num1 = 0f;
								AIActor nearestEnemy = (self.Owner as PlayerController).CurrentRoom.GetNearestEnemy(self.Owner.CenterPosition, out num1, true, false);
								if (nearestEnemy)
								{
									vector = nearestEnemy.CenterPosition - self.specRigidbody.UnitCenter;
									return vector.ToAngle();
								}
								else
									return (self.Owner as PlayerController).m_currentGunAngle;
							}
							else
								return target;
						});
				}
			}
		}

		public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			Harmony.CreateAndPatchAll(typeof(AimerPatches));
		}

        public static void Log(string text, string color = "FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
	}
}
