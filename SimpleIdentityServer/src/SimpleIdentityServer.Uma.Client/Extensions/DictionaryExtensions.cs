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

using Newtonsoft.Json;
using System.Collections.Generic;

namespace SimpleIdentityServer.Client.Extensions
{
    internal static class DictionaryExtensions
    {
        #region Set methods

        public static void SetObject(
            this Dictionary<string, object> dic,
            string name,
            object value)
        {
            if (!dic.ContainsKey(name))
            {
                var serializedObject = JsonConvert.SerializeObject(value);
                dic.Add(name, serializedObject);
                return;
            }

            dic[name] = value;
        }

        public static void SetValue(
            this Dictionary<string, object> dic,
            string name,
            object value)
        {
            if (!dic.ContainsKey(name))
            {
                dic.Add(name, value.ToString());
                return;
            }

            dic[name] = value;
        }

        #endregion

        #region Get methods

        public static T GetObject<T>(
            this Dictionary<string, object> dic,
            string name) where T : new()
        {
            if (!dic.ContainsKey(name))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(dic[name].ToString());
        }

        public static bool GetBoolean(
            this Dictionary<string, object> dic,
            string name)
        {
            if (!dic.ContainsKey(name))
            {
                return false;
            }

            return bool.Parse(dic[name].ToString());
        }

        public static string GetString(
            this Dictionary<string, object> dic,
            string name)
        {
            if (!dic.ContainsKey(name))
            {
                return string.Empty;
            }

            return dic[name].ToString();
        }

        public static double GetDouble(
            this Dictionary<string, object> dic,
            string name)
        {
            if (!dic.ContainsKey(name))
            {
                return default(double);
            }

            return double.Parse(dic[name].ToString());
        }

        #endregion
    }
}
