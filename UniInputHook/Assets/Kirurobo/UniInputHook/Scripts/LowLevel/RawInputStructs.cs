namespace Kirurobo.UniInputHook
{

    /// <summary>
    /// 一回のキーボード操作をまとめて表す構造体
    /// </summary>
    public struct KeyboardActionArgs
    {
        public int VkCode { get; set; }
        public int Modifires { get; set; } 
        public int ScanCode { get; set; }
        public int Wheel { get; set; }

        /// <summary>
        /// 押下イベントなら false, 離上イベントなら true
        /// </summary>
        public bool IsUp { get; set; }

        override public string ToString()
        {
            var updown = IsUp ? "Up" : "Down";
            return $"Key:{VkCode}, Mod:{Modifires}, Scan:{ScanCode}, {updown}";
        }
    }

    /// <summary>
    /// マウスボタンの種類
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }

    /// <summary>
    /// 一回のマウス操作をまとめて表す構造体
    /// </summary>
    public struct MouseActionArgs
    {
        public MouseButton Button { get; set; }

        /// <summary>
        /// 押下イベントなら false, 離上イベントなら true
        /// </summary>
        public bool IsUp { get; set; }
    }

    /// <summary>
    /// キー監視権限について何かあればその種類を表す
    /// </summary>
    public enum PrivilegeState
    {
        /// <summary>
        /// 問題無し
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 権限が無かったためこのタイミングでダイアログが開いた
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// このタイミングで権限が取得された（システム設定を変更されるのは後になるため、この状態にはならないはず）
        /// </summary>
        Permitted = 2,
    }
}
