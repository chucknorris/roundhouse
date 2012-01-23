using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FubuCore.Util
{
    public class CompositePredicate<T>
    {
        private class LoggablePredicate
        {
            public LoggablePredicate(Expression<Func<T,bool>> expression)
            {
                Description = expression.Body.ToString();
                Matches = expression.Compile();    
            }

            public string Description { get; private set; }
            public Func<T, bool> Matches { get; private set; }
        }

        private bool _hasChanged = false;
        private readonly List<LoggablePredicate> _list = new List<LoggablePredicate>();
        private Func<T, bool> _matchesAll = x => true;
        private Func<T, bool> _matchesAny = x => true;
        private Func<T, bool> _matchesNone = x => false;

        public void ResetChangeTracking()
        {
            _hasChanged = false;
        }

        public bool HasChanged
        {
            get { return _hasChanged; }
        }

        public void Add(Expression<Func<T, bool>> filter)
        {
            _hasChanged = true;

            _matchesAll = x => _list.All(predicate => predicate.Matches(x));
            _matchesAny = x => _list.Any(predicate => predicate.Matches(x));
            _matchesNone = x => !MatchesAny(x);

            _list.Add(new LoggablePredicate(filter));
        }

        public static CompositePredicate<T> operator +(CompositePredicate<T> invokes, Expression<Func<T, bool>> filter)
        {
            invokes.Add(filter);
            return invokes;
        }

        public bool MatchesAll(T target)
        {
            return _matchesAll(target);
        }

        public bool MatchesAny(T target)
        {
            return _matchesAny(target);
        }

        public bool MatchesNone(T target)
        {
            return _matchesNone(target);
        }

        public IEnumerable<string> GetDescriptionOfMatches(T target)
        {
            return _list.Where(p => p.Matches(target)).Select(p => p.Description);
        }

        public bool DoesNotMatchAny(T target)
        {
            return _list.Count == 0 ? true : !MatchesAny(target);
        }
    }
}