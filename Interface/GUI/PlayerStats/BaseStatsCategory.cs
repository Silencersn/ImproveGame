﻿using ImproveGame.Interface.Common;
using ImproveGame.Interface.SUIElements;

namespace ImproveGame.Interface.GUI.PlayerStats;

/// <summary>
/// 属性类别
/// </summary>
public class BaseStatsCategory
{
    /// <summary>
    /// Whether this stats category is added via Mod.Call
    /// </summary>
    public bool IsAddedFromCall { get; set; }

    public Vector2? UIPosition;

    /// <summary>
    /// 收藏
    /// </summary>
    public bool Favorite { get; set; }

    public List<BaseStat> BaseProperties { get; private set; } = new();

    public BaseStatsCategory(Texture2D texture, string nameKey, bool isAddedFromCall = false, Texture2D modSmallIcon = null)
    {
        Texture = texture;
        NameKey = nameKey;
        IsAddedFromCall = isAddedFromCall;
        ModSmallIcon = modSmallIcon;
    }

    public Texture2D ModSmallIcon { get; set; }

    public Texture2D Texture { get; set; }

    public string NameKey { get; set; }

    public string Name => IsAddedFromCall ? Language.GetTextValue(NameKey) : GetText(NameKey);

    /// <summary>
    /// 创建卡片
    /// </summary>
    public StatsCard CreateCard(out SUIImage image, out SUITitle title)
    {
        // 卡片主体
        StatsCard card = new(this, UIColor.PanelBorder * 0f, UIColor.PanelBg * 0f, 10f, 2);
        card.OverflowHidden = true;

        // 标题图标
        image = new SUIImage(Texture)
        {
            ImagePercent = new Vector2(0.5f),
            ImageOrigin = new Vector2(0.5f),
            ImageScale = 0.8f,
            DragIgnore = true,
            TickSound = false
        };
        image.Width.Pixels = 20f;
        image.Height.Percent = 1f;
        image.Join(card.TitleView);

        // 标题文字
        title = new(Name, 0.36f)
        {
            HAlign = 0.5f
        };
        title.Height.Percent = 1f;
        title.Width.Pixels = title.TextSize.X;
        title.Join(card.TitleView);

        // 模组图标
        if (ModSmallIcon is null) return card;
        
        var modIcon = new SUIImage(ModSmallIcon)
        {
            ImagePercent = new Vector2(0.5f),
            ImageOrigin = new Vector2(0.5f),
            ImageScale = 0.8f,
            DragIgnore = true,
            TickSound = false,
            Width = {Pixels = 20f},
            Height = {Percent = 1f},
            Left = {Pixels = -20f, Percent = 1f}
        };
        modIcon.Join(card.TitleView);

        return card;
    }

    public void AppendProperties(StatsCard card)
    {
        for (int i = 0; i < BaseProperties.Count; i++)
        {
            BaseStat stat = BaseProperties[i];
            StatBar statBar = new(stat.Name, stat.Value, stat);

            if (stat.Favorite)
            {
                card.Append(statBar);
            }
        }
    }

    public void AppendPropertiesForControl(View view, StatsCard card)
    {
        for (int i = 0; i < BaseProperties.Count; i++)
        {
            BaseStat stat = BaseProperties[i];
            StatBar bar = new(stat.Name, stat.Value, stat);

            bar.OnUpdate += (_) =>
            {
                Color red = new Color(1f, 0f, 0f);
                bar.BorderColor = (stat.Parent.Favorite && stat.Favorite) ? UIColor.ItemSlotBorderFav : Color.Transparent;
                bar.Border = (stat.Parent.Favorite && stat.Favorite) ? 2 : 0;
            };

            bar.OnLeftMouseDown += (_, _) =>
            {
                stat.Favorite = !stat.Favorite;

                foreach (var item in view.Children)
                {
                    if (item is StatsCard innerCard && innerCard.StatsCategory == card.StatsCategory)
                    {
                        innerCard.RemoveAllChildren();
                        innerCard.Append(innerCard.TitleView);
                        innerCard.StatsCategory.AppendProperties(innerCard);
                    }
                }
            };

            card.Console = true;
            card.Append(bar);
        }
    }
}