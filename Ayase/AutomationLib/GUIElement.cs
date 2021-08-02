using System;
using System.Windows;

namespace Ayase.AutomationLib
{
    public class GUIElement
    {
        public Rect BoundingRectangle = new Rect(0, 0, 0, 0);
        public String Name = "";

        
        public GUIElement(Rect rect)
        {
            BoundingRectangle = rect;
        }

        public GUIElement(String name)
        {
            Name = name;
        }


        public GUIElement(Rect rect, String name)
        {
            BoundingRectangle = rect;
            Name = name;
        }
    }
}
