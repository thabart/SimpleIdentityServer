#region copyright
// Copyright 2016 Habart Thierry
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

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Core.Common.Serializers
{
    public interface IParamSerializer
    {
        string Serialize(object obj);
        object Deserialize(string input);
        T Deserialize<T>(string input);
        object Deserialize(NameValueCollection input);
        T Deserialize<T>(NameValueCollection input);
    }

    public class ParamSerializer : IParamSerializer
    {
        public string Serialize(object obj)
        {
            var paramString = this.Parametrize(JObject.FromObject(obj));
            return paramString.TrimEnd(new[] { '&' });
        }

        public object Deserialize(NameValueCollection input)
        {
            return Deserialize(this.ConvertNameValueCollection(input));
        }

        public T Deserialize<T>(NameValueCollection input)
        {
            return Deserialize<T>(this.ConvertNameValueCollection(input));
        }

        public T Deserialize<T>(IFormCollection form)
        {
            return Deserialize<T>(ConvertNameValueCollection(form));
        }

        public T Deserialize<T>(IQueryCollection query)
        {
            return Deserialize<T>(ConvertNameValueCollection(query));
        }

        public object Deserialize(string input)
        {
            return Deparametrize(input).ToObject<object>();
        }

        public T Deserialize<T>(string input)
        {
            return this.Deparametrize(input).ToObject<T>();
        }

        private JObject Deparametrize(string input)
        {
            var obj = new JObject();
            var items = input.Replace("+", " ").Split(new[] { '&' });
            foreach (string item in items)
            {
                var param = item.Split(new[] { '=' });
                var key = WebUtility.UrlDecode(param[0]);
                if (!string.IsNullOrEmpty(key))
                {
                    var keys = key.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                    var keysLast = keys.Length - 1;
                    if (Regex.IsMatch(keys[0], @"\[") && Regex.IsMatch(keys[keysLast], @"\]$"))
                    {
                        keys[keysLast] = Regex.Replace(keys[keysLast], @"\]$", string.Empty);
                        keys = keys[0].Split(new[] { '[' }).Concat(keys.Skip(1)).ToArray();
                        keysLast = keys.Length - 1;
                    }
                    else
                    {
                        keysLast = 0;
                    }
                    
                    if (param.Length == 2)
                    {
                        var val = WebUtility.UrlDecode(param[1]);
                        if (keysLast != 0)
                        {
                            object cur = obj;
                            for (var i = 0; i <= keysLast; i++)
                            {
                                int index = -1, nextindex;                                
                                key = keys[i];
                                if (key == string.Empty || int.TryParse(key, out index))
                                {
                                    key = index == -1 ? "0" : index.ToString(CultureInfo.InvariantCulture);
                                }

                                if (cur is JArray)
                                {
                                    var jarr = cur as JArray;
                                    if (i == keysLast)
                                    {
                                        if (index >= 0 && index < jarr.Count)
                                        {
                                            jarr[index] = val;
                                        }
                                        else
                                        {
                                            jarr.Add(val);
                                        }
                                    }
                                    else
                                    {
                                        if (index < 0 || index >= jarr.Count)
                                        {
                                            if (keys[i + 1] == string.Empty || int.TryParse(keys[i + 1], out nextindex))
                                            {
                                                jarr.Add(new JArray());
                                            }
                                            else
                                            {
                                                jarr.Add(new JObject());
                                            }

                                            index = jarr.Count - 1;
                                        }

                                        cur = jarr.ElementAt(index);
                                    }
                                }
                                else if (cur is JObject)
                                {
                                    var jobj = cur as JObject;
                                    if (i == keysLast)
                                    {
                                        jobj[key] = val;
                                    }
                                    else
                                    {
                                        if (jobj[key] == null)
                                        {
                                            if (keys[i + 1] == string.Empty || int.TryParse(keys[i + 1], out nextindex))
                                            {
                                                jobj.Add(key, new JArray());
                                            }
                                            else
                                            {
                                                jobj.Add(key, new JObject());
                                            }
                                        }

                                        cur = jobj[key];
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (obj[key] is JArray)
                            {
                                (obj[key] as JArray).Add(val);
                            }
                            else if (obj[key] != null && val != null)
                            {
                                obj[key] = new JArray { obj[key], val };
                            }
                            else
                            {
                                obj[key] = val;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(key))
                    {
                        obj[key] = null;
                    }
                }
            }

            return obj;
        }

        private string Parametrize(object obj, string value = "")
        {
            var returnVal = string.Empty;

            if (obj is JObject)
            {
                var jobj = obj as JObject;
                foreach (var key in jobj.Properties())
                {
                    if (jobj[key.Name] == null || string.IsNullOrWhiteSpace(jobj[key.Name].ToString()))
                    {
                        continue;
                    }

                    returnVal += this.Parametrize(
                        jobj[key.Name], value == string.Empty ? key.Name : string.Format("{0}[{1}]", value, key.Name));
                }
            }
            else if (obj is JArray)
            {
                var arr = obj as JArray;
                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i];
                    if (item is JArray || item is JObject)
                    {
                        returnVal += this.Parametrize(item, string.Format("{0}[{1}]", value, i));
                    }
                    else
                    {
                        returnVal += this.Parametrize(item, string.Format("{0}[]", value));
                    }
                }
            }
            else
            {
                return string.Format("{0}={1}&", value, obj);
            }

            return returnVal;
        }

        private string ConvertNameValueCollection(NameValueCollection input)
        {
            var output = new StringBuilder();
            foreach (var key in input.AllKeys)
            {
                var values = input.GetValues(key) ?? new string[] { };
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }
		
        private string ConvertNameValueCollection(IFormCollection form)
        {
            var output = new StringBuilder();
            foreach (var key in form.Keys)
            {
                var values = form[key];
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }

        private string ConvertNameValueCollection(IQueryCollection query)
        {
            var output = new StringBuilder();
            foreach (var key in query.Keys)
            {
                var values = query[key];
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }
    }
}
