﻿using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.Players;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ImproveGame.Common.Systems
{
    /// <summary>
    /// 为了方便管理，这里主要放一些不成体系的小修改，比如一些单独的On, IL
    /// </summary>
    public class MinorModifySystem : ModSystem
    {
        public override void Load() {
            // 还原哥布林重铸槽中物品的重铸次数
            On.Terraria.Player.dropItemCheck += SaveReforgePrefix;
            // 死亡是否掉落墓碑
            On.Terraria.Player.DropTombstone += DisableDropTombstone;
            // 抓取距离修改
            On.Terraria.Player.PullItem_Common += Player_PullItem_Common;
            // 晚上刷新 NPC
            IL.Terraria.Main.UpdateTime += TweakNPCNightSpawn;
            // 城镇NPC入住速度修改
            IL.Terraria.Main.UpdateTime_SpawnTownNPCs += SpeedUpNPCSpawn;
            // 修改空间法杖显示平台剩余数量
            IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += TweakDrawCountInventory;
            // 伤害波动
            On.Terraria.Main.DamageVar += DisableDamageVar;
            // 使存钱罐中物品生效，如同放入背包一样
            On.Terraria.Player.VanillaPreUpdateInventory += TweakExtraUpdateInventory;
            // 旗帜更新
            On.Terraria.SceneMetrics.ScanAndExportToMain += TweakBannerSceneMetrics;
            // 拾取物品处理方法
            On.Terraria.Player.PickupItem += Player_PickupItem;
            // 摇树总是掉落水果
            IL.Terraria.WorldGen.ShakeTree += TweakShakeTree;
        }

        /// <summary>
        /// 拾取物品的时候
        /// </summary>
        private Item Player_PickupItem(On.Terraria.Player.orig_PickupItem orig, Player player, int playerIndex, int worldItemArrayIndex, Item itemToPickUp) {
            ImprovePlayer improvePlayer = player.GetModPlayer<ImprovePlayer>();
            // 智能虚空保险库
            if (MyUtils.Config.SmartVoidVault) {
                if (!itemToPickUp.IsACoin) {
                    // 大背包
                    if (!itemToPickUp.IsAir && MyUtils.Config.SuperVault && MyUtils.HasItem(player.GetModPlayer<DataPlayer>().SuperVault, itemToPickUp)) {
                        itemToPickUp = MyUtils.ItemStackToInventory(player.GetModPlayer<DataPlayer>().SuperVault, itemToPickUp);
                    }
                    // 虚空保险库
                    if (!itemToPickUp.IsAir && player.IsVoidVaultEnabled && MyUtils.HasItem(player.bank4.item, itemToPickUp)) {
                        itemToPickUp = MyUtils.ItemStackToInventory(player.bank4.item, itemToPickUp);
                    }
                    // 其他
                    if (MyUtils.Config.SuperVoidVault) {
                        if (!itemToPickUp.IsAir && improvePlayer.PiggyBank && MyUtils.HasItem(player.bank.item, itemToPickUp)) {
                            itemToPickUp = MyUtils.ItemStackToInventory(player.bank.item, itemToPickUp);
                        }
                        if (!itemToPickUp.IsAir && improvePlayer.Safe && MyUtils.HasItem(player.bank2.item, itemToPickUp)) {
                            itemToPickUp = MyUtils.ItemStackToInventory(player.bank2.item, itemToPickUp);
                        }
                        if (!itemToPickUp.IsAir && improvePlayer.DefendersForge && MyUtils.HasItem(player.bank3.item, itemToPickUp)) {
                            itemToPickUp = MyUtils.ItemStackToInventory(player.bank3.item, itemToPickUp);
                        }
                    }
                }
            }
            if (!itemToPickUp.IsAir) {
                itemToPickUp = orig(player, playerIndex, worldItemArrayIndex, itemToPickUp);
            }
            // 大背包
            if (!itemToPickUp.IsAir && MyUtils.Config.SuperVault && itemToPickUp.type != ItemID.None && itemToPickUp.stack > 0 && !itemToPickUp.IsACoin) {
                itemToPickUp = MyUtils.ItemStackToInventory(player.GetModPlayer<DataPlayer>().SuperVault, itemToPickUp);
            }
            // 超级虚空保险库
            if (MyUtils.Config.SuperVoidVault) {
                if (itemToPickUp.type != ItemID.None && itemToPickUp.stack > 0 && !itemToPickUp.IsACoin) {
                    if (!itemToPickUp.IsAir && improvePlayer.PiggyBank) {
                        itemToPickUp = MyUtils.ItemStackToInventory(player.bank.item, itemToPickUp);
                    }
                    if (!itemToPickUp.IsAir && improvePlayer.Safe && itemToPickUp.type != ItemID.None && itemToPickUp.stack > 0) {
                        itemToPickUp = MyUtils.ItemStackToInventory(player.bank2.item, itemToPickUp);
                    }
                    if (!itemToPickUp.IsAir && improvePlayer.DefendersForge && itemToPickUp.type != ItemID.None && itemToPickUp.stack > 0) {
                        itemToPickUp = MyUtils.ItemStackToInventory(player.bank3.item, itemToPickUp);
                    }
                }
            }
            Main.item[worldItemArrayIndex] = itemToPickUp;
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, worldItemArrayIndex);
            }
            return itemToPickUp;
        }

        /// <summary>
        /// 旗帜BUFF在背包生效
        /// </summary>
        private static void AddBannerBuff(SceneMetrics self, Player player, Item item) {
            if (item.createTile == TileID.Banners) {
                int style = item.placeStyle;
                int frameX = style * 18;
                int frameY = 0;
                if (style >= 90) {
                    frameX -= 1620;
                    frameY += 54;
                }
                if (frameX >= 396 || frameY >= 54) {
                    int styleX = frameX / 18 - 21;
                    for (int num4 = frameY; num4 >= 54; num4 -= 54) {
                        styleX += 90;
                    }
                    self.NPCBannerBuff[styleX] = true;
                    self.hasBanner = true;
                }
            }
        }

        /// <summary>
        /// 旗帜增益
        /// </summary>
        private void TweakBannerSceneMetrics(On.Terraria.SceneMetrics.orig_ScanAndExportToMain orig, SceneMetrics self, SceneMetricsScanSettings settings) {
            orig(self, settings);
            // 随身旗帜（增益站）
            if (MyUtils.Config.NoPlace_BUFFTile_Banner) {
                Player player = Main.LocalPlayer;
                for (int i = 0; i < player.inventory.Length; i++) {
                    Item item = player.inventory[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank.item.Length; i++) {
                    Item item = player.bank.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank2.item.Length; i++) {
                    Item item = player.bank2.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank3.item.Length; i++) {
                    Item item = player.bank3.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank4.item.Length; i++) {
                    Item item = player.bank4.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                if (MyUtils.Config.SuperVault) {
                    for (int i = 0; i < player.GetModPlayer<DataPlayer>().SuperVault.Length; i++) {
                        Item item = player.GetModPlayer<DataPlayer>().SuperVault[i];
                        if (item.type == ItemID.None)
                            continue;
                        AddBannerBuff(self, player, item);
                    }
                }
            }
        }

        /// <summary>
        /// 使存钱罐中物品如同放在背包
        /// </summary>
        private void TweakExtraUpdateInventory(On.Terraria.Player.orig_VanillaPreUpdateInventory orig, Player self) {
            orig(self);
            var items = MyUtils.GetAllInventoryItemsList(self, true);
            foreach (var item in items) {
                self.VanillaUpdateInventory(item);
            }
        }

        /// <summary>
        /// 伤害波动
        /// </summary>
        private int DisableDamageVar(On.Terraria.Main.orig_DamageVar orig, float dmg, float luck) {
            if (MyUtils.Config.BanDamageVar)
                return (int)Math.Round(dmg);
            else
                return orig(dmg, luck);
        }

        /// <summary>
        // NPC 晚上刷新
        /// </summary>
        private void TweakNPCNightSpawn(ILContext il) {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchCall(typeof(Main), "UpdateTime_StartDay"),
                i => i.MatchCall(typeof(Main), "HandleMeteorFall")))
                return;
            c.EmitDelegate(() => {
                if (MyUtils.Config.TownNPCSpawnInNight) {
                    MethodInfo methodInfo = typeof(Main).GetMethod("UpdateTime_SpawnTownNPCs", BindingFlags.Static | BindingFlags.NonPublic);
                    methodInfo.Invoke(null, null);
                }
            });
        }

        /// <summary>
        // NPC 刷新速度
        /// </summary>
        private void SpeedUpNPCSpawn(ILContext il) {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld(typeof(Main), nameof(Main.checkForSpawns)),
                i => i.Match(OpCodes.Ldc_I4_1)))
                return;
            c.EmitDelegate<Func<int, int>>((JiaJi) => {
                return (int)Math.Pow(2, MyUtils.Config.TownNPCSpawnSpeed);
            });
        }

        /// <summary>
        // 空间法杖计算剩余平台数
        /// </summary>
        private void TweakDrawCountInventory(ILContext il) {
            // 计算剩余平台
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Pop),
                i => i.Match(OpCodes.Ldc_I4_M1)))
                return;
            c.Emit(OpCodes.Ldarg_1); // 玩家物品槽
            c.Emit(OpCodes.Ldarg_2); // content
            c.Emit(OpCodes.Ldarg_3); // 物品在物品槽的位置
            c.EmitDelegate<Func<int, Item[], int, int, int>>((num11, inv, content, slot) => {
                if (content == 13) {
                    if (inv[slot].type == ModContent.ItemType<Content.Items.SpaceWand>()) {
                        int count = 0;
                        MyUtils.GetPlatformCount(inv, ref count);
                        return count;
                    }
                    else if (inv[slot].type == ModContent.ItemType<Content.Items.WallPlace>()) {
                        int count = 0;
                        MyUtils.GetWallCount(inv, ref count);
                        return count;
                    }
                    return -1;
                }
                else {
                    return -1;
                }
            });
        }

        /// <summary>
        /// 物品吸取速度
        /// </summary>
        private void Player_PullItem_Common(On.Terraria.Player.orig_PullItem_Common orig, Player player, Item item, float xPullSpeed) {
            if (MyUtils.Config.GrabDistance > 0) {
                Vector2 velocity = (player.Center - item.Center).SafeNormalize(Vector2.Zero);
                if (item.velocity.Length() + velocity.Length() > 15f) {
                    item.velocity = velocity * 15f;
                }
                else {
                    item.velocity = velocity * (item.velocity.Length() + 1);
                }
            }
            else {
                orig(player, item, xPullSpeed);
            }
        }

        /// <summary>
        /// 墓碑掉落
        /// </summary>
        private void DisableDropTombstone(On.Terraria.Player.orig_DropTombstone orig, Player self, int coinsOwned, Terraria.Localization.NetworkText deathText, int hitDirection) {
            if (!MyUtils.Config.BanTombstone) {
                orig(self, coinsOwned, deathText, hitDirection);
            }
        }

        /// <summary>
        /// 前缀保存
        /// </summary>
        private void SaveReforgePrefix(On.Terraria.Player.orig_dropItemCheck orig, Player self) {
            if (Main.reforgeItem.type > ItemID.None && self.GetModPlayer<DataPlayer>().ReforgeItemPrefix > 0) {
                Main.reforgeItem.GetGlobalItem<GlobalItemData>().recastCount =
                    self.GetModPlayer<DataPlayer>().ReforgeItemPrefix;
                self.GetModPlayer<DataPlayer>().ReforgeItemPrefix = 0;
            }
            orig(self);
        }

        /// <summary>
        /// 摇树总掉水果
        /// </summary>
        private void TweakShakeTree(ILContext il) {
            try {
                // 源码，在最后：
                // if (flag) {
                //     [摇树有物品出现，执行一些特效]
                // }
                // 搞到这个flag, 如果为false(没东西)就加水果, 然后让他读到true
                // IL_0DAF: ldloc.s   flag
                // IL_0DB1: brfalse.s IL_0E12
                // 这两行就可以精确找到, 因为其他地方没有相同的
                // 值得注意的是，代码开始之前有这个：
                // treeShakeX[numTreeShakes] = x;
                // treeShakeY[numTreeShakes] = y;
                // numTreeShakes++;
                // 所以我们可以直接用了，都不需要委托获得x, y

                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.Before,
                                   i => i.Match(OpCodes.Ldloc_S),
                                   i => i.Match(OpCodes.Brfalse_S))) {
                    ErrorTweak();
                    return;
                }

                c.Index++;
                c.EmitDelegate<Func<bool, bool>>((shackSucceed) => {
                    if (!shackSucceed && MyUtils.Config.ShakeTreeFruit) {
                        int x = WorldGen.treeShakeX[WorldGen.numTreeShakes - 1];
                        int y = WorldGen.treeShakeY[WorldGen.numTreeShakes - 1];
                        int tileType = Main.tile[x, y].TileType;
                        TreeTypes treeType = WorldGen.GetTreeType(tileType);

                        // 获取到顶部
                        y--;
                        while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType]) {
                            y--;
                        }
                        y++;

                        int fruit = MyUtils.GetShakeTreeFruit(treeType);
                        if (fruit > -1) {
                            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, fruit);
                            shackSucceed = true;
                        }
                    }
                    return shackSucceed;
                });

            }
            catch {
                ErrorTweak();
                return;
            }
        }

        private static void ErrorTweak() {
            string exception = "Something went wrong in TweakShakeTree(), please contact with the mod developers.\nYou can still use the mod, but the \"Always drop fruit when shaking the tree\" option will not work";
            if (GameCulture.FromCultureName(GameCulture.CultureName.Chinese).IsActive)
                exception = "TweakShakeTree()发生错误，请联系Mod制作者\n你仍然可以使用Mod，但是“摇树总掉水果”选项不会起作用";
            ImproveGame.Instance.Logger.Warn(exception);
        }
    }
}
