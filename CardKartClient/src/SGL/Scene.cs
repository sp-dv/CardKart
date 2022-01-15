using CardKartClient.GUI;
using OpenTK.Input;
using System.Collections.Generic;

namespace SGL
{
    internal abstract class Scene
    {
        public List<GuiComponent> Components { get; } = new List<GuiComponent>();

        private GuiComponent Focused { get; set; }

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
            if (Focused != null)
            {
                Focused.HandleKeyboardEvent(e);
            }    
        }

        public virtual void HandleKeyUp(KeyboardKeyEventArgs e)
        {
        }

        public void HandleMouseButtonDown(MouseButton button, GLCoordinate location)
        {
            if (Focused != null) { Focused.IsFocused = false; }
            Focused = null;

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

        public void RequestFocus(GuiComponent focusMe)
        {
            if (focusMe == null) { return; }
            
            Focused = focusMe;
            Focused.IsFocused = true;
        }
    }
}
