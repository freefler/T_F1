using System.Windows.Forms;

namespace MY_Helper
{
    public static class DoTo_Pan
    {
        /// <summary>
        /// 滚动图双缓冲处理，防止闪屏(panel1.EnableDoubleBuffering(); )
        /// </summary>//or//
        //public class BufferedPanel : Panel
        //{
        //    public BufferedPanel() => DoubleBuffered = true;  // 子类能访问父类的 protected 成员
        //}
        public static void Pan_EnableDoubleBuffering(this Control ctl)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(ctl, true);
        }

       
    }

  
}
