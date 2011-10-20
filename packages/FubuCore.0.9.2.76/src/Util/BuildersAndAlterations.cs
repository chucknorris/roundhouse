using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuCore.Util
{
    public class Builder<TInput, TOutput>
    {
        private readonly List<BuilderStrategy<TInput, TOutput>> _builders
            = new List<BuilderStrategy<TInput, TOutput>>();

        private readonly Func<TInput, TOutput> _defaultBuilder;

        public Builder(Func<TInput, TOutput> defaultBuilder)
        {
            _defaultBuilder = defaultBuilder;
        }

        public TOutput Build(TInput input)
        {
            return selectBuilder(input)(input);
        }

        private Func<TInput, TOutput> selectBuilder(TInput input)
        {
            BuilderStrategy<TInput, TOutput> strategy = _builders.FirstOrDefault(x => x.Filter(input));
            return strategy == null ? _defaultBuilder : strategy.Output;
        }

        public void Register(Func<TInput, bool> filter, Func<TInput, TOutput> output)
        {
            _builders.Add(new BuilderStrategy<TInput, TOutput>
            {
                Filter = filter,
                Output = output
            });
        }
    }

    public class BuilderStrategy<TInput, TOutput>
    {
        public Func<TInput, bool> Filter;
        public Func<TInput, TOutput> Output;
    }

    public class Alteration<TBigObject, TLittleObject>
    {
        private readonly List<AlterationStrategy<TBigObject, TLittleObject>> _alterations
            = new List<AlterationStrategy<TBigObject, TLittleObject>>();

        private readonly Action<TBigObject, TLittleObject> _defaultAlteration;

        public Alteration(Action<TBigObject, TLittleObject> defaultAlteration)
        {
            _defaultAlteration = defaultAlteration;
        }

        public void Alter(TBigObject big, TLittleObject little)
        {
            findAlteration(little)(big, little);
        }

        private Action<TBigObject, TLittleObject> findAlteration(TLittleObject target)
        {
            AlterationStrategy<TBigObject, TLittleObject> alteration = _alterations.FirstOrDefault(x => x.Filter(target));
            return alteration == null ? _defaultAlteration : alteration.Alteration;
        }

        public void Register(Func<TLittleObject, bool> filter, Action<TBigObject, TLittleObject> alteration)
        {
            _alterations.Add(new AlterationStrategy<TBigObject, TLittleObject>
            {
                Alteration = alteration,
                Filter = filter
            });
        }
    }

    public class AlterationStrategy<TBigObject, TLittleObject>
    {
        public Action<TBigObject, TLittleObject> Alteration;
        public Func<TLittleObject, bool> Filter;
    }
}