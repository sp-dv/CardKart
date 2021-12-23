using SGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class SmartTextPanel : GuiComponent
    {
        public string Text { get; set; }

        public SmartTextPanel()
        {
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (Text != null)
            {
            }
        }
    }
}
