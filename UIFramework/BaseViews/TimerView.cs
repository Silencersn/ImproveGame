﻿namespace ImproveGame.UIFramework.BaseViews;

/// <summary>
/// 大背包按钮背景上的效果，需要直接继承此类即可。(继承自 RelativeUIE)
/// </summary>
public class TimerView : View
{
    public AnimationTimer HoverTimer = new AnimationTimer(3);

    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            if (!HoverTimer.AnyOpen)
                HoverTimer.OpenAndResetTimer();
        }
        else
        {
            if (!HoverTimer.AnyClose)
                HoverTimer.CloseAndResetTimer();
        }

        HoverTimer.Update();
        base.Update(gameTime);
    }
}
