﻿using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Helpers
{
    internal static class Utils
    {
        public static unsafe bool IsHostileMemory(BattleNpc npc)
        {
            if (npc == null)
            {
                return false;
            }

            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1)
                && *(byte*)(npc.Address + 0x1980) != 0
                && *(byte*)(npc.Address + 0x193C) != 1;
        }

        public static unsafe float ActorShieldValue(Actor actor)
        {
            if (actor == null)
            {
                return 0f;
            }

            return Math.Min(*(int*)(actor.Address + 0x1997), 100) / 100f;
        }

        public static string DurationToString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            var t = TimeSpan.FromSeconds(duration);

            if (t.Hours > 1)
            {
                return t.Hours + "h";
            }

            if (t.Minutes >= 5)
            {
                return t.Minutes + "m";
            }

            if (t.Minutes >= 1)
            {
                return $"{t.Minutes}:{t.Seconds:00}";
            }

            return t.Seconds.ToString();
        }

        public static PluginConfigColor ColorForActor(Chara actor)
        {
            switch (actor.ObjectKind)
            {
                // Still need to figure out the "orange" state; aggroed but not yet attacked.
                case ObjectKind.Player:
                    return GlobalColors.Instance.SafeColorForJobId(actor.ClassJob.Id);

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    return GlobalColors.Instance.NPCHostileColor;

                case ObjectKind.BattleNpc:
                    if (!IsHostileMemory((BattleNpc)actor))
                    {
                        return GlobalColors.Instance.NPCFriendlyColor;
                    }
                    break;
            }

            return GlobalColors.Instance.NPCNeutralColor;
        }

        public static bool HasTankInvulnerability(Actor actor)
        {
            var tankInvulnBuff = actor.StatusEffects.Where(o => o.EffectId is 810 or 1302 or 409 or 1836);
            return tankInvulnBuff.Count() > 0;
        }

        public static Actor FindTargetOfTarget(Actor target, Actor player, ActorTable actors)
        {
            if (target == null)
            {
                return null;
            }

            if (target.TargetActorID == 0 && player.TargetActorID == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (var i = 0; i < 200; i += 2)
            {
                var actor = actors[i];
                if (actor?.ActorId == target.TargetActorID)
                {
                    return actor;
                }
            }

            return null;
        }

        public static Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            switch (anchor)
            {
                case DrawAnchor.Center: return position - size / 2f;
                case DrawAnchor.Left: return position + new Vector2(0, -size.Y / 2f);
                case DrawAnchor.Right: return position + new Vector2(-size.X, -size.Y / 2f);
                case DrawAnchor.Top: return position + new Vector2(-size.X / 2f, 0);
                case DrawAnchor.TopLeft: return position;
                case DrawAnchor.TopRight: return position + new Vector2(-size.X, 0);
                case DrawAnchor.Bottom: return position + new Vector2(-size.X / 2f, -size.Y);
                case DrawAnchor.BottomLeft: return position + new Vector2(0, -size.Y);
                case DrawAnchor.BottomRight: return position + new Vector2(-size.X, -size.Y);
            }

            return position;
        }
    }
}
