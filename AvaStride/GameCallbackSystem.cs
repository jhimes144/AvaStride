using Stride.Core;
using Stride.Engine;
using Stride.Games;

namespace AvaStride
{
    /// <summary>
    /// Gets added by <see cref="AvaloniaInStride"/> to allow for callbacks to be dispatched on the main game thread.
    /// </summary>
    internal class GameCallbackSystem : GameSystem
    {
        readonly object _lock = new();
        readonly HashSet<Action> _actions = [];

		public GameCallbackSystem(IServiceRegistry registry, bool isEnabled) : base(registry)
		{
            Enabled = isEnabled;
		}

		public override void Update(GameTime time)
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
