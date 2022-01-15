using CardKartClient.GUI.Components;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using OpenTK.Input;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Scenes
{
    internal class LoginScene : Scene
    {
        public SmartTextPanel UserNameLabel;
        public SmartTextPanel PasswordLabel;
        public SmartTextPanel Message;

        public TextInputBox UsernameInput;
        public TextInputBox PasswordInput;

        public SmartTextPanel LoginButton;
        public SmartTextPanel RegisterButton;

        public LoginScene()
        {
            Message = new SmartTextPanel();
            Message.Alignment = QuickFont.QFontAlignment.Centre;
            Components.Add(Message);

            UsernameInput = new TextInputBox();
            UsernameInput.Done += Login;
            UsernameInput.SetText(User.Configuration.DefaultUsername);
            Components.Add(UsernameInput);

            PasswordInput = new TextInputBox();
            PasswordInput.Done += Login;
            Components.Add(PasswordInput);

            UserNameLabel = new SmartTextPanel();
            UserNameLabel.Text = "Username";
            Components.Add(UserNameLabel);

            PasswordLabel = new SmartTextPanel();
            PasswordLabel.Text = "Password";
            Components.Add(PasswordLabel);

            LoginButton = new SmartTextPanel();
            LoginButton.Text = "Log In";
            LoginButton.BackgroundImage = Textures.Button1;
            LoginButton.Clicked += Login;
            Components.Add(LoginButton);

            RegisterButton = new SmartTextPanel();
            RegisterButton.Text = "Register";
            RegisterButton.BackgroundImage = Textures.Button1;
            RegisterButton.Clicked += Register;
            Components.Add(RegisterButton);

            Layout();
        }

        private void Layout()
        {
            Message.X = Constants.GUI.LoginMessagePanelX;
            Message.Y = Constants.GUI.LoginMessagePanelY;
            Message.Height = Constants.GUI.LoginMessageH;
            Message.Width = Constants.GUI.LoginMessageW;
            Message.Layout();

            UsernameInput.X = Constants.GUI.LoginUsernameInputX;
            UsernameInput.Y = Constants.GUI.LoginUsernameInputY;
            UsernameInput.Width = Constants.GUI.LoginUsernameInputWidth;
            UsernameInput.Height = Constants.GUI.LoginUsernameInputHeight;
            UsernameInput.Layout();

            PasswordInput.HideBehindStars = true;
            PasswordInput.X = Constants.GUI.LoginPasswordInputX;
            PasswordInput.Y = Constants.GUI.LoginPasswordInputY;
            PasswordInput.Width = Constants.GUI.LoginUsernameInputWidth;
            PasswordInput.Height = Constants.GUI.LoginUsernameInputHeight;
            PasswordInput.Layout();

            UserNameLabel.X = Constants.GUI.LoginUserNameLabelX;
            UserNameLabel.Y = Constants.GUI.LoginUserNameLabelY;
            UserNameLabel.Height = Constants.GUI.LoginUsernameLabelHeight;
            UserNameLabel.Width = Constants.GUI.LoginUsernameLabelWidth;
            UserNameLabel.Layout();

            PasswordLabel.X = Constants.GUI.LoginPasswordLabelX;
            PasswordLabel.Y = Constants.GUI.LoginPasswordLabelY;
            PasswordLabel.Height = Constants.GUI.LoginUsernameLabelHeight;
            PasswordLabel.Width = Constants.GUI.LoginUsernameLabelWidth;
            PasswordLabel.Layout();

            LoginButton.X = Constants.GUI.LoginButtonX;
            LoginButton.Y = Constants.GUI.LoginButtonY;
            LoginButton.Width = Constants.GUI.LoginButtonWidth;
            LoginButton.Height = Constants.GUI.LoginButtonHeight;
            LoginButton.TextOffsetY = Constants.GUI.LoginButtonTextOffset;
            LoginButton.Alignment = QuickFont.QFontAlignment.Centre;
            LoginButton.Layout();

            RegisterButton.X = 1;
            RegisterButton.X = Constants.GUI.LoginRegisterButtonX;
            RegisterButton.Y = Constants.GUI.LoginButtonY;
            RegisterButton.Width = Constants.GUI.LoginButtonWidth;
            RegisterButton.Height = Constants.GUI.LoginButtonHeight;
            RegisterButton.TextOffsetY = Constants.GUI.LoginButtonTextOffset;
            RegisterButton.Alignment = QuickFont.QFontAlignment.Centre;
            RegisterButton.Layout();
        }

        public override void HandleKeyDown(KeyboardKeyEventArgs e)
        {
            base.HandleKeyDown(e);

            if (e.Key == Key.R)
            {
                Layout();
            }
        }

        private void Login()
        {
            if (UsernameInput.Text == null || UsernameInput.Text.Length == 0 ||
                PasswordInput.Text == null || PasswordInput.Text.Length == 0)
            { return; }


            Message.Text = "Logging in";
            Message.Layout();
            var username = UsernameInput.Text;
            var password = PasswordInput.Text;
            var response = CardKartClient.Server.LogIn(username, password);
            if (response.Code == GenericResponseMessage.Codes.OK)
            {
                User.Username = username;
                User.Configuration.DefaultUsername = username;
                CardKartClient.Controller.ToMainMenu();
            }
            else
            {
                Message.Text = response.Info;
                Message.Layout();
                PasswordInput.ClearText();
            }
        }

        private void Register()
        {
            if (UsernameInput.Text == null || UsernameInput.Text.Length == 0 ||
                PasswordInput.Text == null || PasswordInput.Text.Length == 0)
            { return; }


            Message.Text = "Registering";
            Message.Layout();
            var username = UsernameInput.Text;
            var password = PasswordInput.Text;
            var response = CardKartClient.Server.Register(username, password);
            if (response.Code == GenericResponseMessage.Codes.OK)
            {
                Message.Text = "User registered successfully.";
                Message.Layout();
            }
            else
            {
                Message.Text = response.Info;
                Message.Layout();
                PasswordInput.ClearText();
            }
        }
    }
}
