using CardKartShared.GameState;
using SGL;
using System;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class PlayerPanel : GuiComponent
    {
        private Player Player;

        public ManaButtonBar ManaBar;

        public PlayerPanel(Player player)
        {
            Player = player;

            Width = 0.4f;
            Height = 0.15f;

            ManaBar = new ManaButtonBar();

            Player.PlayerChanged += Update;
        }

        private void Update()
        {
            ManaBar.Update(Player.CurrentMana);
        }

        public void Layout()
        {
            ManaBar.X = X + 0.01f;
            ManaBar.Y = Y + 0.015f;

            ManaBar.Layout();
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.LightGray);

            ManaBar.Draw(drawAdapter);
        }
    }
}
