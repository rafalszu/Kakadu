using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Kakadu.ActionApi.Tests
{
    public class PrivateObject<T> where T : class
    {
        private readonly object caller;

        public PrivateObject(object obj)
        {
            caller = obj;
        }

        #nullable enable
        public object? Invoke(string methodName, object?[]? parameters)
        {
            if(string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));

            var type = typeof(T);

            MethodInfo method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.Name == "InterceptKnownRouteAsync" && x.IsPrivate)
                .FirstOrDefault();

            if(method == null)
                throw new Exception($"'{methodName}' not found in '{type.ToString()}' type");

            return method.Invoke(caller, parameters);
        }

        #nullable disable
    }
}