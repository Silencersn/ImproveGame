﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ImproveGame.UI.UIElements
{
    public class JuItemGrid : UIElement
    {
        public ItemList itemList;
        public FixedUIScrollbar scrollbar;
        public Item[] items;

        public JuItemGrid(Item[] SuperVault)
        {
            items = SuperVault;
            SetPadding(0);
            OverflowHidden = true;

            scrollbar = new();
            itemList = new();
            itemList.SetPadding(0);
            int widthCount = 10;
            for (int i = 0; i < SuperVault.Length; i++)
            {
                JuItemSlot slot = new JuItemSlot(SuperVault, i);
                slot.Left.Set(i % widthCount * (slot.Width.Pixels + 10f), 0f);
                slot.Top.Set(i / widthCount * (slot.Height.Pixels + 10f), 0f);
                itemList.Append(slot);
                if (i == 0)
                {
                    itemList.Width.Set(slot.Width.Pixels * widthCount + 10f * widthCount - 10f, 0f);
                    itemList.Height.Set(slot.Height.Pixels * 5 + 10f * 5 - 10f, 0f);
                    Append(itemList);

                    Width.Set(itemList.Width.Pixels, 0f);
                    Height.Set(itemList.Height.Pixels, 0f);

                    scrollbar.SetView(Height.Pixels,
                        slot.Height.Pixels * (SuperVault.Length / widthCount) + 10f * (SuperVault.Length / widthCount) - 10f);
                    scrollbar.Height.Set(Height.Pixels - 12f, 0f);
                    scrollbar.HAlign = 1f;
                    scrollbar.VAlign = 0.5f;
                    Width.Pixels += scrollbar.Width.Pixels + 10f;
                    Append(scrollbar);
                }
            }
        }

        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            scrollbar.ViewPosition -= evt.ScrollWheelValue;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (scrollbar != null)
            {
                itemList.Top.Set(-scrollbar.GetValue(), 0);
            }
            itemList.Recalculate();
        }

        /// <summary>
        /// 显示物品的列表
        /// </summary>
        public class ItemList : UIElement
        {
            public override bool ContainsPoint(Vector2 point)
            {
                return true;
            }

            protected override void DrawChildren(SpriteBatch spriteBatch)
            {
                Vector2 position = Parent.GetDimensions().Position();
                Vector2 dimensions = new Vector2(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
                foreach (UIElement current in Elements)
                {
                    Vector2 position2 = current.GetDimensions().Position();
                    Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
                    if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
                    {
                        current.Draw(spriteBatch);
                    }
                }
            }
        }
    }
}
