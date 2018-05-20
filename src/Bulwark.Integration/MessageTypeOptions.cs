using System;
using System.Collections.Generic;

namespace Bulwark.Integration
{
    public class MessageTypeOptions
    {
        public MessageTypeOptions()
        {
            Types = new List<Type>();
        }
        
        public List<Type> Types { get; set; }

        public void AddType<T>()
        {
            Types.Add(typeof(T));
        }
    }
}