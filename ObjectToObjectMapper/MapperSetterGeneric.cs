using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectToObjectMapper
{
    public class MapperSetterGeneric<S, T> : ObjectCopyBase<S, T>
    {
        private IEnumerable<(Func<S, object> Getter, Action<T, object> Setter)> _maps;

        public override void MapTypes()
        {
            var sourceProperties = typeof(S).GetProperties();
            var targetProperties = typeof(T).GetProperties();

            _maps = (from s in sourceProperties
                     from t in targetProperties
                     where s.Name == t.Name &&
                           s.CanRead &&
                           t.CanWrite &&
                           s.PropertyType == t.PropertyType
                     select new
                     {
                         SourceGetter = CreateGetter(s),
                         TargetSetter = CreateSetter(t)
                     }).Select(q => (q.SourceGetter, q.TargetSetter)).ToArray();
        }

        public override void Copy(S source, T target)
        {
            foreach (var i in _maps)
            {
                i.Setter(target, i.Getter(source));
            }
        }

        Func<S, object> CreateGetter(PropertyInfo Reflection)
        {
            var param = Expression.Parameter(typeof(S));
            var field = Expression.Convert(Expression.Property(param, Reflection.Name), typeof(object));

            return Expression.Lambda<Func<S, object>>(field, param).Compile();
        }
        // https://stackoverflow.com/a/13773898
        Action<T, object> CreateSetter(PropertyInfo Reflection)
        {
            var targetType = Expression.Parameter(typeof(T));
            var target = Expression.Property(targetType, Reflection.Name);
            var value = Expression.Parameter(typeof(object));
            return Expression.Lambda<Action<T, object>>(Expression.Assign(target, Expression.Convert(value, target.Type)),
                targetType,
                value).Compile();
        }
    }
}