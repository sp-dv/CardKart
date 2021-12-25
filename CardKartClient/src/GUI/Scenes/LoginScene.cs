using CardKartClient.GUI.Components;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Scenes
{
    internal class LoginScene : Scene
    {
        public SmartTextPanel Login1;
        public SmartTextPanel Login2;
        public TextPanel Message;

        public LoginScene()
        {
            Message = new TextPanel();
            Components.Add(Message);

            Login1 = new SmartTextPanel();
            Login1.BackgroundColor = Color.Pink;
            Login1.Text = "1";
            Login1.Y = 0.2f;
            Login1.Width = 0.2f;
            Login1.Height = 0.1f;
            Login1.Layout();
            Login1.Clicked += () =>
            {
                Message.Text = "Logging in";
                Message.Text = CardKartClient.Controller.Login("testuser1", "testpassword");
            };
            Components.Add(Login1);

            Login2 = new SmartTextPanel();
            Login2.BackgroundColor = Color.Pink;
            Login2.Text = "2";
            Login2.Y = -0.2f;
            Login2.Width = 0.2f;
            Login2.Height = 0.1f;
            Login2.Layout();
            Login2.Clicked += () =>
            {
                Message.Text = "Logging in";
                Message.Text = CardKartClient.Controller.Login("testuser2", "testpassword");
            };
            Components.Add(Login2);
        }
    }
}
