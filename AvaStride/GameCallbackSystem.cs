using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaStride
{
    internal class GameCallbackSystem : SyncScript
    {
        readonly object _lock = new();
        readonly HashSet<Action> _actions = [];

        public override void Update()
        {
            lock (_lock)
            {
                foreach (var action in _actions)
                {
                    action();
                }

                _actions.Clear();
            }
        }

        public void Dispatch(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
            }
        }
    }
}
