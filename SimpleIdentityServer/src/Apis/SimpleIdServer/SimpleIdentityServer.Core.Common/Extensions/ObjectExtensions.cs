#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SimpleIdentityServer.Core.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static string SerializeWithDataContract(this object parameter)
        {
            var serializer = new DataContractJsonSerializer(parameter.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, parameter);
                ms.Position = 0;
                var reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        public static T DeserializeWithDataContract<T>(this string serialized)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.Unicode.GetBytes(serialized));
            var obj = serializer.ReadObject(ms);
            return (T)obj;
        }

        public static string SerializeWithJavascript(this object parameter)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(parameter);
        }

        public static T DeserializeWithJavascript<T>(this string parameter)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameter);
        }
    }
}
