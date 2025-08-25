using System.Collections.Generic;

namespace DailyCommissionPanel.Models
{
    public class Settings
    {
        public int EndHour { get; set; } = 21;
        public int EndMinute { get; set; } = 50;
        public List<string> Rules { get; set; } = new List<string>
        {
            "保持安静，专注学习📕",
            "有问题憋着下课问",
            "合理规划自习时间⏰",
            "今天不学习，明天变垃圾🚮",
            "珍惜每分每秒🕙",
            "物品轻拿轻放🐾",
            "作业做完了吗就讲话，闭嘴👊🔥"
        };
    }
}