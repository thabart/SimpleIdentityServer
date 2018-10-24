using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Bus
{
    public static class ProcessMessageHelper
    {
        public static void Process(string json, IEnumerable<IEventHandler> eventHandlers)
        {
            var serializeMessage = JsonConvert.DeserializeObject<SerializedMessage>(json);
            var type = Type.GetType(serializeMessage.AssemblyQualifiedName);
            var deserializedObj = JsonConvert.DeserializeObject(serializeMessage.Content, type);
            foreach (var eventHandler in eventHandlers)
            {
                var interfaces = eventHandler.GetType().GetInterfaces().Where(i =>
                {
                    return i.IsInterface && i.IsGenericType && i.GetGenericArguments().Any(a => typeof(Event).IsAssignableFrom(a) && a == type);
                });
                if (!interfaces.Any())
                {
                    continue;
                }

                var method = eventHandler.GetType().GetMethods().FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Any() && m.GetParameters().Count() == 1 && m.GetParameters().First().ParameterType == type);
                if (method == null)
                {
                    continue;
                }

                method.Invoke(eventHandler, new object[] { deserializedObj });
            }
        }
    }
}
