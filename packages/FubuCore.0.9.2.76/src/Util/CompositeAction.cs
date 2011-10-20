using System;
using System.Collections.Generic;

namespace FubuCore.Util
{
    public class CompositeAction<T, U>
    {
        private readonly List<Action<T, U>> _actions = new List<Action<T, U>>();

        public static CompositeAction<T, U> operator +(CompositeAction<T, U> actions, Action<T, U> action)
        {
            actions._actions.Add(action);
            return actions;
        }

        public void Do(T t, U u)
        {
            _actions.Each(x => x(t, u));
        }
    }

    public class CompositeAction<T>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();

        public static CompositeAction<T> operator +(CompositeAction<T> actions, Action<T> action)
        {
            actions._actions.Add(action);
            return actions;
        }

        public void Do(T target)
        {
            _actions.Each(x => x(target));
        }
    }
}