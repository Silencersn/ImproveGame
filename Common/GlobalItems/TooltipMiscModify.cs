﻿using ImproveGame.Common.Systems;
using System.Collections.Generic;
using Terraria.ModLoader.Default;

namespace ImproveGame.Common.GlobalItems
{
    internal class TooltipMiscModify : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!ModIntegrationsSystem.WMITFLoaded &&
                item.type != ModIntegrationsSystem.UnloadedItemType &&
                (item.type != ModIntegrationsSystem.AprilFoolsItemType || !AprilFools.CheckAprilFools()))
            {
                if (item.ModItem is not null && !item.Name.Contains("[" + item.ModItem.Mod.Name + "]") && !item.Name.Contains("[" + item.ModItem.Mod.DisplayName + "]"))
                {
                    string text = GetTextWith("Tips.FromMod", new { item.ModItem.Mod.DisplayName });
                    TooltipLine line = new(Mod, Mod.Name, "- " + text + " -")
                    {
                        OverrideColor = Colors.RarityBlue
                    };
                    tooltips.Add(line);
                }
            }
            if (item.DamageType == DamageClass.Summon && !item.sentry)
            {
                string key = "Tips.SummonSlot";
                if (Main.LocalPlayer.slotsMinions >= Main.LocalPlayer.maxMinions)
                {
                    key += "Full";
                }
                string text = GetTextWith(key, new {
                    Current = Main.LocalPlayer.slotsMinions,
                    Total = Main.LocalPlayer.maxMinions
                });
                TooltipLine line = new(Mod, Mod.Name, text);
                tooltips.Add(line);
            }
        }
    }
}