﻿using ImproveGame.Entitys;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImproveGame.Content.Items
{
    /// <summary>
    /// 选区物品做成一个基类，方便批量生产
    /// </summary>
    public abstract class SelectorItem : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => MyUtils.Config.LoadModItems;

        public override void SetStaticDefaults() {
            Item.staff[Type] = true;
        }

        private Point Start;
        private Point End;
        protected Point SelectRange;
        protected Rectangle TileRect => new((int)MathF.Min(Start.X, End.X), (int)MathF.Min(Start.Y, End.Y), (int)MathF.Abs(Start.X - End.X) + 1, (int)MathF.Abs(Start.Y - End.Y) + 1);

        /// <summary>
        /// 封装了原来的SetDefaults()，现在用这个，在里面应该设置SelectRange
        /// </summary>
        public virtual void SetItemDefaults() { }
        public sealed override void SetDefaults() {
            // 基本属性
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item1;

            SetItemDefaults();
        }

        /// <summary>
        /// CanUseItem的替代
        /// </summary>
        public virtual bool StartUseItem(Player player) => true;

        /// <summary>
        /// 是否应该使用自由选区功能，比如说某个模式开了才能自由选区之类的
        /// </summary>
        public virtual bool CanUseSelector(Player player) => true;

        public override bool CanUseItem(Player player) {
            bool flag = StartUseItem(player);
            if (flag && CanUseSelector(player)) {
                MyUtils.ItemRotation(player);
                _unCancelled = true;
                Start = Main.MouseWorld.ToTileCoordinates();
            }
            return flag;
        }

        /// <summary>
        /// 右键取消选区的实现
        /// </summary>
        private bool _unCancelled;

        /// <summary>
        /// 对被框选的物块进行修改的方法，如果返回值为false，则会立刻终止后面的操作，默认为true
        /// </summary>
        public virtual bool ModifySelectedTiles(Player player, int i, int j) => true;

        public override bool? UseItem(Player player) {
            if (CanUseSelector(player)&& !Main.dedServ && player.whoAmI == Main.myPlayer) {
                if (Main.mouseRight && _unCancelled) {
                    _unCancelled = false;
                }
                End = MyUtils.LimitRect(Start, Main.MouseWorld.ToTileCoordinates(), SelectRange.X, SelectRange.Y);
                Color color;
                if (_unCancelled)
                    color = new(255, 0, 0);
                else
                    color = Color.GreenYellow;
                int boxIndex = Box.NewBox(Start, End, color * 0.35f, color);
                if (boxIndex is not -1) {
                    Box box = DrawSystem.boxs[Box.NewBox(Start, End, color * 0.35f, color)];
                    box.ShowWidth = true;
                    box.ShowHeight = true;
                }
                if (Main.mouseLeft) {
                    player.itemAnimation = 8;
                    MyUtils.ItemRotation(player);
                }
                else {
                    player.itemAnimation = 0;
                    if (_unCancelled) {
                        Rectangle tileRect = TileRect;
                        int minI = tileRect.X;
                        int maxI = tileRect.X + tileRect.Width - 1;
                        int minJ = tileRect.Y;
                        int maxJ = tileRect.Y + tileRect.Height - 1;
                        for (int j = minJ; j <= maxJ; j++) {
                            for (int i = minI; i <= maxI; i++) {
                                if (!ModifySelectedTiles(player, i, j)) {
                                    return base.UseItem(player);
                                }
                            }
                        }
                    }
                }
            }
            return base.UseItem(player);
        }
    }
}