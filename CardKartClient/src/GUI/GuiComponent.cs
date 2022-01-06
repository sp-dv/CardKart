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

            foreach (var child in Components.Reverse<GuiComponent>())
            {
                if (child.HandleClick(location)) { return true; }
            }

            if (!ComponentRectangleContains(location)) { return false; }

            Clicked?.Invoke();
            return true;
        }

        public bool ComponentRectangleContains(GLCoordinate location)
        {
            if (location == null) { return false; }
            return location.InBounds(X, Y, X + Width, Y + Height);
        }

        public void HandleMouseMove(GLCoordinate location, bool root)
        {
            // Ugly dupe to make root component events fire.
            // Since we fire events for children from the parent (to ensure exit gets called
            // before enter) this has to be done explicitly for root components.
            // It's ugly but it just werks.
            if (root)
            {
                if (ComponentRectangleContains(location))
                {
                    if (!MouseIsInComponent)
                    {
                        MouseEnteredEvent?.Invoke();
                    }
                    MouseIsInComponent = true;
                    MouseMovedEvent?.Invoke();
                }
                else
                {
                    if (MouseIsInComponent)
                    {
                        MouseExitedEvent?.Invoke();
                    }
                    MouseIsInComponent = false;
                }
            }

            GuiComponent hit = null;

            foreach (var child in Components.Reverse<GuiComponent>())
            {
                child.HandleMouseMove(hit == null ? location : null, false);

                
                if (hit == null && child.ComponentRectangleContains(location)) 
                {
                    hit = child;
                }
                else
                {
                    if (child.MouseIsInComponent)
                    {
                        child.MouseIsInComponent = false;
                        child.MouseExitedEvent?.Invoke();
                    }
                }
            }

            // Ensure entered event is always invoked after exited events.
            if (hit != null)
            {
                if (!hit.MouseIsInComponent)
                {
                    hit.MouseIsInComponent = true;
                    hit.MouseEnteredEvent?.Invoke();
                }
                hit.MouseMovedEvent?.Invoke();
            }
        }
    }
}
