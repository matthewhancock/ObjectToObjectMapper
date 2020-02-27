using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectToObjectMapper
{
    public class MapperSetter : ObjectCopyBase
    {
        private IEnumerable<(Func<object, object> Getter, Action<object, object> Setter)> _maps;

        public override void MapTypes(Type source, Type target)
        {
            var sourceProperties = source.GetProperties();
            var targetProperties = target.GetProperties();

            _maps = (from s in sourceProperties
                     from t in targetProperties
                     where s.Name == t.Name &&
                           s.CanRead &&
                           t.CanWrite &&
                           s.PropertyType == t.PropertyType
                     select new
                     {
                         SourceGetter = (Func<object, object>)s.GetValue,
                         TargetSetter = (Action<object, object>)t.SetValue
                     }).Select(q => (q.SourceGetter, q.TargetSetter)).ToArray();
        }

        public override void Copy(object source, object target)
        {
            foreach (var i in _maps)
            {
                i.Setter(target, i.Getter(source));
            }
        }
    }
}