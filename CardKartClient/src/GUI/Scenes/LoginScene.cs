using CardKartClient.GUI.Components;
using SGL;

namespace CardKartClient.GUI.Scenes
{
    internal class LoginScene : Scene
    {
        public Button Login1;
        public Button Login2;
        public TextPanel Message;

        public LoginScene()
        {
            Message = new TextPanel();
            Components.Add(Message);

            Login1 = new Button();
            Login1.Text = "1";
            Login1.Y = 0.2f;
            Login1.Width = 0.2f;
            Login1.Height = 0.1f;
            Login1.Clicked += () =>
            {
                Message.Text = "Logging in";
                Message.Text = CardKartClient.Controller.Login("testuser1", "testpassword");
            };
            Components.Add(Login1);

            Login2 = new Button();
            Login2.Text = "1";
            Login2.Y = -0.2f;
            Login2.Width = 0.2f;
            Login2.Height = 0.1f;
            Login2.Clicked += () =>
            {
                Message.Text = "Logging in";
                Message.Text = CardKartClient.Controller.Login("testuser1", "testpasswordx");
            };
            Components.Add(Login2);
        }
    }
}
