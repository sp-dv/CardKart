using CardKartShared.GameState;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class PlayerPanel : GuiComponent
    {
        private Player Player;

        public ManaButtonBar ManaBar;
        public TextPanel HealthPanel;

        public PlayerPortrait PlayerPortrait;

        public event PlayerPortrait.PlayerClickedHandler PlayerPortraitClicked;

        public PlayerPanel(Player player)
        {
            Player = player;

            Width = 0.6f;
            Height = 0.2f;

            ManaBar = new ManaButtonBar();

            HealthPanel = new TextPanel();
            HealthPanel.BackgroundImage = Textures.Health1;

            PlayerPortrait = new PlayerPortrait(player);
            PlayerPortrait.PlayerClicked += player => PlayerPortraitClicked?.Invoke(player);

            Player.PlayerChanged += Update;
        }

        private void Update()
        {
            ManaBar.Update(Player.CurrentMana, Player.MaxMana);
            HealthPanel.Text = Player.HeroCard.Token.CurrentHealth.ToString();
        }

        public void Layout()
        {
            ManaBar.X = X + 0.01f;
            ManaBar.Y = Y + 0.015f;
            ManaBar.Layout();

            HealthPanel.X = X + 0.335f;
            HealthPanel.Y = Y + 0.11f;
            HealthPanel.Width = 0.06f;
            HealthPanel.Height = 0.08f;
            HealthPanel.TextInsetX = 0.02f;
            HealthPanel.TextInsetY = 0.075f;

            PlayerPortrait.X = X + 0.48f;
            PlayerPortrait.Y = Y + 0.02f;
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.LightGray);

            ManaBar.Draw(drawAdapter);
            HealthPanel.Draw(drawAdapter);
            PlayerPortrait.Draw(drawAdapter);
        }

        protected override void HandleClickInternal(GLCoordinate location)
        {
            if (PlayerPortrait.HandleClick(location)) { }
        }
    }

    internal class PlayerPortrait : GuiComponent
    {
        public Player Player;

        private Texture PortraitTexture;

        public delegate void PlayerClickedHandler(Player player);
        public event PlayerClickedHandler PlayerClicked;

        public PlayerPortrait(Player player)
        {
            Player = player;

            Width = 0.10f;
            Height = 0.16f;
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (PortraitTexture != null)
            {
                drawAdapter.DrawSprite(X, Y, X + Width, Y + Height, PortraitTexture);
            }
            else if (Player.HeroCard != null)
            {
                PortraitTexture = Textures.Portraits(Player.HeroCard.Template);
            }
        }

        protected override void HandleClickInternal(GLCoordinate location)
        {
            PlayerClicked?.Invoke(Player);
        }
    }
}
