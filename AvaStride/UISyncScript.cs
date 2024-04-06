using Avalonia.Controls;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaStride
{
    public abstract class UISyncScript : SyncScript
    {
        public abstract void UIUpdate(Window window, TimeSpan t);

        /// <summary>
        /// Called before the script enters it's update loop. NOTE: You MUST call base method if overrideing.
        /// </summary>
        public override void Start()
        {
            AvaloniaInStride.RegisterUIThreadScript(this);
        }
    }
}
