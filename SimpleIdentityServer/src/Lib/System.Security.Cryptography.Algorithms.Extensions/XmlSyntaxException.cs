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

namespace System.Security.Cryptography
{
    [JsonObject]
    public sealed class XmlSyntaxException : Exception
    {
        public XmlSyntaxException()
        {

        }

        public XmlSyntaxException(string message)
            : base(message)
        {
        }
        
        public XmlSyntaxException(string message, Exception inner)
            : base(message, inner)
        {
        }
        
        public XmlSyntaxException(int lineNumber)
            : base("XMLSyntax_SyntaxError")
        {
        }
        
        public XmlSyntaxException(int lineNumber, string message)
            : base("XMLSyntax_SyntaxErrorEx")
        {
        }
    }
}
