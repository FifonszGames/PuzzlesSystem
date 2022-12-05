using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    public static class ReflectionExtensions
    {
        #region Private Fields

        private static readonly BindingFlags PrivateFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly BindingFlags PublicFlags = BindingFlags.Public | BindingFlags.Instance;

        #endregion

        #region Public Methods

        public static void SetFieldValue<T, K>(this T self, K fieldValue, string fieldName, bool isPrivate = true)
        {
            FieldInfo fieldInfo = self.GetType().GetField(fieldName, isPrivate ? PrivateFlags : PublicFlags);

            if (fieldInfo == null)
            {
                Debug.Log($"couldn't find field {fieldName}, it doesn't exits, it's name was changed or somehow it cannot be found with reflection");

                return;
            }

            fieldInfo.SetValue(self, fieldValue);
        }

        public static void SetPropertyValue<T, K>(this T self, K propertyValue, string propertyName, bool isPrivate = true)
        {
            PropertyInfo propertyInfo = self.GetType().GetProperty(propertyName, isPrivate ? PrivateFlags : PublicFlags);

            if (propertyInfo == null)
            {
                Debug.Log($"couldn't find property {propertyName}, it doesn't exits, it's name was changed or somehow it cannot be found with reflection");

                return;
            }

            propertyInfo.SetValue(self, propertyValue);
        }

        public static void CallPrivateMethod<T>(this T self, string methodName, params object[] parameters)
        {
            MethodInfo methodInfo = self.GetType().GetMethod(methodName, PrivateFlags);

            if (methodInfo == null)
            {
                Debug.Log($"couldn't find method {methodName}, it doesn't exits, it's name was changed, parameters do not match or somehow it cannot be found with reflection");

                return;
            }

            methodInfo.Invoke(self, parameters);
        }

        public static IEnumerable<Type> GetAllDerivedOf<T>(T target) where T : Type
        {
            return Assembly.GetAssembly(target).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(target));
        }

        #endregion
    }
}
