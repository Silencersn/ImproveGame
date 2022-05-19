﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ImproveGame.Content.Items
{
    public class WallPlace : ModItem
    {
        public override void SetDefaults()
        {
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Lime;
            Item.value = Terraria.Item.sellPrice(0, 1, 0, 0);
            Item.mana = 50;
            Item.UseSound = SoundID.Item1;
        }

        /*public static List<Point> Walls = new();
        public static void PlaceWallXY(Point position, int range = 10)
        {
            Tile tile = Main.tile[position.X, position.Y];
            if (tile.HasTile && (TileID.Sets.Platforms[tile.TileType] || Main.tileSolid[tile.TileType] ||
                tile.TileType == 10 || tile.TileType == 11))
            {
                return;
            }
            // 向右算
            for (int i = 0; i <= range; i++)
            {
                if (HasTile(position + new Point(i, 0)))
                {
                    break;
                }
                PlaceWallY(position, i, range);
            }
            // 向左算
            for (int i = -1; i >= -range; i--)
            {
                if (HasTile(position + new Point(i, 0)))
                {
                    break;
                }
                PlaceWallY(position, i, range);
            }
        }

        public static void PlaceWallY(Point position, int i, int range)
        {
            // 向下算
            for (int j = 1; j <= range; j++)
            {
                if (HasTile(position + new Point(i, j)))
                {
                    break;
                }
            }
            // 向上算
            for (int j = -1; j >= -range; j--)
            {
                if (HasTile(position + new Point(i, j)))
                {
                    break;
                }
            }
        }*/

        public static int HasTile(Point position, ref List<Point> Walls, ref List<Point> Walls2)
        {
            Tile tile = Main.tile[position.X, position.Y];
            // 有墙体，是平台，门，实体块
            if (tile.HasTile && (TileID.Sets.Platforms[tile.TileType] || Main.tileSolid[tile.TileType] ||
                tile.TileType == 10 || tile.TileType == 11))
            {
                return 1; // 有方块或者墙体
            }
            if (tile.WallType == 0)
            {
                return 0; // 没有方块没有墙体
            }
            return 2; // 有墙体没有方块
        }

        public static void ABC(Point position, ref List<Point> Walls, ref List<Point> Walls2)
        {
            // 不在世界内，在Wall内，大于500
            if (position.X >= Main.maxTilesX || position.Y >= Main.maxTilesY || position.X <= 0 ||
                position.Y <= 0 || Walls.Contains(position) || Walls2.Contains(position) || Walls.Count > 1000 ||
                Walls2.Count > 2000)
            {
                return;
            }
            int hasTile = HasTile(position, ref Walls, ref Walls2);
            if (hasTile == 1)
            {
                return;
            }
            if (hasTile == 0)
            {
                Walls.Add(position);
            }
            if (hasTile == 2)
            {
                Walls2.Add(position);
            }
            ABC(position + new Point(0, -1), ref Walls, ref Walls2);
            ABC(position + new Point(1, 0), ref Walls, ref Walls2);
            ABC(position + new Point(0, 1), ref Walls, ref Walls2);
            ABC(position + new Point(-1, 0), ref Walls, ref Walls2);
        }

        public override bool? UseItem(Player player)
        {
            Point point = Main.MouseWorld.ToTileCoordinates();
            if (Main.myPlayer == player.whoAmI && player.itemAnimation == player.itemAnimationMax)
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.owner == player.whoAmI && proj.active &&
                        proj.type == ModContent.ProjectileType<Projectiles.PlaceWall>())
                    {
                        proj.Kill();
                        CombatText.NewText(proj.getRect(), new Color(225, 0, 0), Language.GetTextValue($"Mods.ImproveGame.CombatText_Item.WallPlace_Kill"));
                        return base.UseItem(player);
                    }
                }
                List<Point> Walls = new();
                List<Point> Walls2 = new();
                ABC(point, ref Walls, ref Walls2);
                if (Walls.Count > 1000 || (Walls.Count == 0 && Walls2.Count > 2000))
                {
                    CombatText.NewText(player.getRect(), new Color(225, 0, 0), Language.GetTextValue($"Mods.ImproveGame.CombatText_Item.WallPlace_Limit"));
                    return base.UseItem(player);
                }
                else if (Walls.Count > 0)
                {
                    CombatText.NewText(player.getRect(), new Color(0, 155, 255), Language.GetTextValue($"Mods.ImproveGame.CombatText_Item.WallPlace_Consume") + Walls.Count);
                    Projectile proj = Projectile.NewProjectileDirect(null, Main.MouseWorld, Vector2.Zero,
                        ModContent.ProjectileType<Projectiles.PlaceWall>(), 0, 0, player.whoAmI);
                    ((Projectiles.PlaceWall)proj.ModProjectile).Walls = Walls;
                }
            }
            return base.UseItem(player);
        }
    }
}