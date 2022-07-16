﻿namespace ImproveGame.Common.Systems
{
    /// <summary>
    /// 快捷键
    /// </summary>
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind SuperVaultKeybind { get; private set; }
        public static ModKeybind BuffTrackerKeybind { get; private set; }

        public override void Load()
        {
            SuperVaultKeybind = KeybindLoader.RegisterKeybind(Mod, "大背包 Huge Inventory", "I");
            BuffTrackerKeybind = KeybindLoader.RegisterKeybind(Mod, "增益追踪器 Buff Tracker", "NumPad3");
        }

        public override void Unload()
        {
            SuperVaultKeybind = null;
            BuffTrackerKeybind = null;
        }
    }
}
