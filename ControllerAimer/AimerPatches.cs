using UnityEngine;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using System;
using System.Reflection;

namespace ControllerAimer
{
    public static class AimerPatches
    {
        public static void EmitCall<T>(this ILCursor iLCursor, string methodName, Type[] parameters = null, Type[] generics = null)
        {
            MethodInfo methodInfo = AccessTools.Method(typeof(T), methodName, parameters, generics);
            iLCursor.Emit(OpCodes.Call, methodInfo);
        }

        [HarmonyPatch(typeof(GuidedBulletsPassiveItem), nameof(GuidedBulletsPassiveItem.PreMoveProjectileModifier))]
        public class PreMoveProjectileModifierPatchClass
        {
            [HarmonyILManipulator]
            public static void PreMoveProjectileModifierPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.Before,
                    x => x.MatchStloc(2)))
                {
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.Emit(OpCodes.Ldarg_1);
                    crs.Emit(OpCodes.Ldloc_0);
                    crs.Emit(OpCodes.Ldloc_1);
                    crs.EmitCall<PreMoveProjectileModifierPatchClass>(nameof(PreMoveProjectileModifierPatchClass.PreMoveProjectileModifierPatchCall));
                }
            }

            private static float PreMoveProjectileModifierPatchCall(float target, GuidedBulletsPassiveItem self, Projectile p, BraveInput instanceForPlayer, Vector2 vector)
            {
                if (!(instanceForPlayer.IsKeyboardAndMouse(false)
                            || instanceForPlayer.ActiveActions == null)
                            && vector == Vector2.zero)
                {
                    AIActor nearestEnemy = self.Owner.CurrentRoom.GetNearestEnemy(self.Owner.CenterPosition, out _, true, false);
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
            }
        }

        [HarmonyPatch(typeof(InputGuidedProjectile), nameof(InputGuidedProjectile.Move))]
        public class MovePatchClass
        {
            [HarmonyILManipulator]
            public static void MovePatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.Before,
                    x => x.MatchStloc(3)))
                {
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.Emit(OpCodes.Ldloc_1);
                    crs.Emit(OpCodes.Ldloc_2);
                    crs.EmitCall<MovePatchClass>(nameof(MovePatchClass.MovePatchCall));
                }
            }

            private static float MovePatchCall(float target, InputGuidedProjectile self, BraveInput instanceForPlayer, Vector2 vector)
            {
                if (!(instanceForPlayer.IsKeyboardAndMouse(false)
                        || instanceForPlayer.ActiveActions == null)
                        && vector == Vector2.zero)
                {
                    AIActor nearestEnemy = (self.Owner as PlayerController).CurrentRoom.GetNearestEnemy(self.Owner.CenterPosition, out _, true, false);
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
            }
        }
    }
}
