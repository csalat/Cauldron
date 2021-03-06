﻿using System;
using System.Reflection;

namespace Cauldron.Interception.External.Test
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ExternalMethodInterceptorAttribute : Attribute, IMethodInterceptor
    {
        public ExternalMethodInterceptorAttribute(string message, int length)
        {
        }

        public void OnEnter(Type declaringType, object instance, MethodBase methodbase, object[] values)
        {
        }

        public void OnException(Exception e)
        {
        }

        public void OnExit()
        {
        }
    }
}