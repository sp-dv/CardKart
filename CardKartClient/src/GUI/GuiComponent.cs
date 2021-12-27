using SGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartClient.GUI
{
    internal abstract class GuiComponent
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public GLCoordinate GuiLocation1 => new GLCoordinate(X + Width / 2, Y + Height / 2);

        public bool Visible { get; set; } = true;

        protected List<GuiComponent> Components = new List<GuiComponent>();

        public delegate void ClickedHandler();
        public event ClickedHandler Clicked;

        public delegate void MouseEventHandler();
        public event MouseEventHandler MouseEnteredEvent;
        public event MouseEventHandler MouseMovedEvent;
        public event MouseEventHandler MouseExitedEvent;

        protected bool MouseIsInComponent { get; private set; }

        public virtual void Draw(DrawAdapter drawAdapter)
        {
            if (Visible)
            {
                DrawInternal(drawAdapter);

                lock (Components)
                {
                    foreach (var child in Components)
                    {
                        child.Draw(drawAdapter);
                    }
                }
            }
        }

        protected abstract void DrawInternal(DrawAdapter drawAdapter);

        public bool HandleClick(GLCoordinate location)
        {
            if (!Visible) { return false; }
            if (!ComponentRectangleContains(location)) { return false; }

            foreach (var child in Components.Reverse<GuiComponent>())
            {
                if (child.HandleClick(location)) { return true; }
            }

            Clicked?.Invoke();
            return true;
        }

        public bool ComponentRectangleContains(GLCoordinate location)
        {
            if (location == null) { return false; }
            return location.InBounds(X, Y, X + Width, Y + Height);
        }

        public void HandleMouseMove(GLCoordinate location)
        {
            bool rt;
            if (ComponentRectangleContains(location))
            {
                if (!MouseIsInComponent) 
                { 
                    MouseEntered(location);
                    MouseEnteredEvent?.Invoke();
                }

                MouseIsInComponent = true;
                MouseMoved(location);
                MouseMovedEvent?.Invoke();
            }
            else
            {
                if (MouseIsInComponent) { MouseExited(location); }
                MouseIsInComponent = false;
                MouseExitedEvent?.Invoke();
            }

            foreach (var child in Components.Reverse<GuiComponent>())
            {
                child.HandleMouseMove(location);
            }
        }

        protected virtual void MouseMoved(GLCoordinate location)
        {
        }

        protected virtual void MouseEntered(GLCoordinate location)
        {
        }
        protected virtual void MouseExited(GLCoordinate location)
        {
        }
    }
}
