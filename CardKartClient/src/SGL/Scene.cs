using CardKartClient.GUI;
using OpenTK.Input;
using System.Collections.Generic;

namespace SGL
{
    internal abstract class Scene
    {
        public List<GuiComponent> Components = new List<GuiComponent>();

        public void Draw(DrawAdapter drawAdapter)
        {
            foreach (var component in Components)
            {
                component.Draw(drawAdapter);
            }
        }

        public void HandleKeyDown(KeyboardKeyEventArgs e)
        {
        }

        public void HandleKeyUp(KeyboardKeyEventArgs e)
        {
        }

        public void HandleMouseButtonDown(MouseButton button, GLCoordinate location)
        {
            foreach (var component in Components)
            {
                if (component.HandleClick(location)) { return; }
            }
        }

        public void HandleMouseButtonUp(MouseButtonEventArgs e)
        {
        }

        public void HandleMouseMove(GLCoordinate location)
        {
        }
    }
}
