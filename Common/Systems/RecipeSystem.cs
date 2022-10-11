﻿using ImproveGame.Common.Players;
using ImproveGame.Interface.Common;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;

namespace ImproveGame.Common.Systems
{
    public class RecipeSystem : ModSystem
    {
        public static RecipeGroup GoldGroup;
        public static RecipeGroup SilverGroup;
        public static RecipeGroup IronGroup;
        public static RecipeGroup CopperGroup;
        public static RecipeGroup DemoniteGroup;
        public static RecipeGroup ShadowGroup;

        public override void Unload()
        {
            GoldGroup = null;
            SilverGroup = null;
            IronGroup = null;
            CopperGroup = null;
            DemoniteGroup = null;
            ShadowGroup = null;
        }

        public override void AddRecipeGroups()
        {
            GoldGroup = new(() => GetText("RecipeGroup.GoldGroup"), 19, 706);
            SilverGroup = new(() => GetText("RecipeGroup.SilverGroup"), 21, 705);
            IronGroup = new(() => GetText("RecipeGroup.IronGroup"), 22, 704);
            CopperGroup = new(() => GetText("RecipeGroup.CopperGroup"), 20, 703);
            ShadowGroup = new(() => GetText("RecipeGroup.ShadowGroup"), 86, 1329);
            DemoniteGroup = new(() => GetText("RecipeGroup.DemoniteGroup"), 57, 1257);

            RecipeGroup.RegisterGroup("ImproveGame:GoldGroup", GoldGroup);
            RecipeGroup.RegisterGroup("ImproveGame:SilverGroup", SilverGroup);
            RecipeGroup.RegisterGroup("ImproveGame:IronGroup", IronGroup);
            RecipeGroup.RegisterGroup("ImproveGame:CopperGroup", CopperGroup);
            RecipeGroup.RegisterGroup("ImproveGame:ShadowGroup", ShadowGroup);
            RecipeGroup.RegisterGroup("ImproveGame:DemoniteGroup", DemoniteGroup);
        }

        private static bool _loadedSuperVault; // 本次运行是否加载了大背包，全局字段避免出问题

        public override void Load()
        {
            // 配方列表
            IL.Terraria.Recipe.FindRecipes += AllowBigBagAsMeterial;
            // 物品消耗
            IL.Terraria.Recipe.Create += ConsumeBigBagMaterial;
        }

        private void AllowBigBagAsMeterial(ILContext il)
        {
            var c = new ILCursor(il);

            /* IL_0268: ldsfld    class Terraria.Player[] Terraria.Main::player
             * IL_026D: ldsfld    int32 Terraria.Main::myPlayer
             * IL_0272: ldelem.ref
             * IL_0273: ldfld     class Terraria.Item[] Terraria.Player::inventory
             * IL_0278: stloc.s   'array'
             */
            if (!c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.player)),
                i => i.MatchLdsfld<Main>(nameof(Main.myPlayer)),
                i => i.Match(OpCodes.Ldelem_Ref),
                i => i.MatchLdfld<Player>(nameof(Player.inventory))
            ))
                return;

            c.EmitDelegate(GetWholeInventory);

            /* IL_02E4: ldloc.s   l
             * IL_02E6: ldc.i4.s  58
             * IL_02E8: blt.s     IL_027F
             */
            if (!c.TryGotoNext(
                MoveType.After,
                i => i.Match(OpCodes.Ldloc_S),
                i => i.Match(OpCodes.Ldc_I4_S, (sbyte)58)
                //i => i.Match(OpCodes.Blt_S) // FindRecipes是Blt_S, Create是Blt, 加上这句就不方便偷懒了
            ))
                return;

            var label = c.DefineLabel(); // 记录位置
            c.Emit(OpCodes.Ldsfld, typeof(RecipeSystem).GetField(nameof(_loadedSuperVault), BindingFlags.Static | BindingFlags.NonPublic));
            c.Emit(OpCodes.Brfalse, label); // 如果没加载，跳出
            c.Emit(OpCodes.Pop); // 直接丢弃，因为sbyte不支持127以上
            c.Emit(OpCodes.Ldc_I4, 59 + 100); // 玩家背包 + 大背包
            c.MarkLabel(label); // pop和ldc_i4之后，直接跳到这里就没那两句
        }

        private void ConsumeBigBagMaterial(ILContext il)
        {
            AllowBigBagAsMeterial(il);

            var c = new ILCursor(il);
            /* IL_01A8: ldloc.0
             * IL_01A9: ldloc.s   k
             * IL_01AB: newobj    instance void Terraria.Item::.ctor()
             * IL_01B0: stelem.ref
             */
            if (!c.TryGotoNext(
                MoveType.After,
                i => i.Match(OpCodes.Ldloc_0),
                i => i.Match(OpCodes.Ldloc_S),
                i => i.Match(OpCodes.Newobj),
                i => i.Match(OpCodes.Stelem_Ref)
            ))
                return;

            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Call, typeof(Item).GetMethod(nameof(Item.TurnToAir), BindingFlags.Instance | BindingFlags.Public));
        }

        // 两边代码异常相似，所以我封装成一个方法了
        private Item[] GetWholeInventory(Item[] inventory)
        {
            _loadedSuperVault = false;
            if (Config.SuperVault && Main.LocalPlayer.GetModPlayer<UIPlayerSetting>().SuperVault_HeCheng && DataPlayer.TryGet(Main.LocalPlayer, out var modPlayer) && modPlayer.SuperVault is not null)
            {
                _loadedSuperVault = true;
                var superVault = modPlayer.SuperVault;
                var inv = new Item[inventory.Length + superVault.Length];
                for (int i = 0; i < inventory.Length - 1; i++) // 原版没包括58，我们也不包括
                {
                    inv[i] = inventory[i];
                }
                inv[inventory.Length - 1] = new();
                for (int i = 0; i < superVault.Length; i++)
                {
                    if (superVault[i] is null)
                    {
                        inv[i + inventory.Length] = new();
                        continue;
                    }
                    inv[i + inventory.Length] = superVault[i];
                }
                return inv;
            }
            return inventory;
        }
    }
}
