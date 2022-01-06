using CardKartClient.GUI;
using OpenTK.Input;
using System.Collections.Generic;

namespace SGL
{
    internal abstract class Scene
    {
        public List<GuiComponent> Components { get; } = new List<GuiComponent>();

        public void Draw(DrawAdapter drawAdapter)
        {
            lock (Components)
            {
                PreDraw(drawAdapter);

                foreach (var component in Components)
                {
                    component.Draw(drawAdapter);
                }

                PostDraw(drawAdapter);
            }
        }

        protected virtual void PreDraw(DrawAdapter drawAdapter)
        {
        }

        protected virtual void PostDraw(DrawAdapter drawAdapter)
        {
        }

        public virtual void HandleKeyDown(KeyboardKeyEventArgs e)
        {
        }

        public virtual void HandleKeyUp(KeyboardKeyEventArgs e)
        {
        }

        public void HandleMouseButtonDown(MouseButton button, GLCoordinate location)
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                GuiComponent component = Components[i];
                if (component.HandleClick(location)) { return; }
            }
        }

        public void HandleMouseButtonUp(MouseButtonEventArgs e)
        {
        }

        public void HandleMouseMove(GLCoordinate location)
        {
            // Reverse seems unneccesary.
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                Components[i].HandleMouseMove(location, true);
            }
        }
    }
}
