﻿using ImproveGame.Common.Animations;

namespace ImproveGame.Interface.BaseViews
{
    /// <summary>
    /// 大背包按钮背景上的效果，需要直接继承此类即可。(继承自 RelativeUIE)
    /// </summary>
    public class HoverView : View
    {
        public AnimationTimer hoverTimer;
        public Color beginColor, endColor;
        public float startWidth, endWidth;
        public HoverView()
        {
            hoverTimer = new AnimationTimer(3);
            Rounded = new Vector4(10f);
            beginColor = new Color(0, 0, 0, 0);
            endColor = new Color(0, 0, 0, 0.5f);
            startWidth = 0f;
            endWidth = 4f;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            hoverTimer.Open();
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            hoverTimer.Close();
            base.MouseOut(evt);
        }

        public override void Update(GameTime gameTime)
        {
            hoverTimer.Update();
            base.Update(gameTime);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            Vector2 pos = GetDimensions().Position() ;
            Vector2 size = GetDimensions().Size() ;

            Vector2 shadow = Vector2.Lerp(new(startWidth), new(endWidth), hoverTimer.Schedule);
            Color color = Color.Lerp(beginColor, endColor, hoverTimer.Schedule);

            PixelShader.RoundedRectangle(pos - shadow, size + shadow * 2, Rounded + new Vector4(shadow.X), color);
            base.DrawSelf(sb);
        }
    }
}
