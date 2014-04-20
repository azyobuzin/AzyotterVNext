using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Livet;

namespace Azyotter
{
    public static class PropertyHelper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Tuple<Func<object, object>, Action<object, object>>>>
            propCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Tuple<Func<object, object>, Action<object, object>>>>();
        
        public static void Set<T>(NotificationObject source, string propName, T value, Action<string> raisePropertyChanged)
        {
            var thisType = source.GetType();
            var cacheDic = propCache.GetOrAdd(thisType, new ConcurrentDictionary<string, Tuple<Func<object, object>, Action<object, object>>>());
            Tuple<Func<object, object>, Action<object, object>> t;
            if (cacheDic.TryGetValue(propName, out t))
            {
                var old = (T)t.Item1(source);
                if (!EqualityComparer<T>.Default.Equals(old, value))
                {
                    t.Item2(source, value);
                    raisePropertyChanged(propName);
                }
            }
            else
            {
                var tType = typeof(T);
                FieldInfo field = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .First(f => f.FieldType.IsAssignableFrom(tType) &&
                        (f.Name.Equals(propName, StringComparison.InvariantCultureIgnoreCase)
                        || f.Name.Equals("_" + propName, StringComparison.InvariantCultureIgnoreCase)));

                var old = (T)field.GetValue(source);
                if (!EqualityComparer<T>.Default.Equals(old, value))
                {
                    field.SetValue(source, value);
                    raisePropertyChanged(propName);
                }

                TaskEx.Run(() =>
                {
                    var objType = typeof(object);
                    var sourcePrm = Expression.Parameter(objType, "source");
                    var getFunc = Expression.Lambda<Func<object, object>>(
                        Expression.Convert(
                            Expression.Field(
                                Expression.Convert(sourcePrm, thisType),
                                field
                            ),
                            objType
                        ),
                        sourcePrm
                    ).Compile();

                    var targetPrm = Expression.Parameter(objType, "target");
                    var valuePrm = Expression.Parameter(objType, "value");
                    var setAction = Expression.Lambda<Action<object, object>>(
                        Expression.Call(
                            typeof(PropertyHelper).GetMethod("Assign", BindingFlags.NonPublic | BindingFlags.Static)
                                .MakeGenericMethod(tType),
                            Expression.Field(
                                Expression.Convert(targetPrm, thisType),
                                field
                            ),
                            Expression.Convert(valuePrm, tType)
                        ),
                        targetPrm,
                        valuePrm
                    ).Compile();

                    cacheDic.TryAdd(propName, Tuple.Create(getFunc, setAction));
                });
            }
        }

        private static void Assign<T>(ref T left, T right)
        {
            left = right;
        }
    }
}
