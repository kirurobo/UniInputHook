using System.ComponentModel;

namespace Kirurobo.UniInputHook
{

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

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }

    public struct MouseActionArgs
    {
        public MouseButton Button { get; set; }

        /// <summary>
        /// 押下イベントなら false, 離上イベントなら true
        /// </summary>
        public bool IsUp { get; set; }
    }
}
