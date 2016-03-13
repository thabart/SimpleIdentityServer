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
using System.Collections;
using System.Diagnostics.Contracts;
using System.Text;
using StringMaker = System.Security.Cryptography.Tokenizer.StringMaker;

namespace System.Security.Cryptography
{
    [JsonObject]
    sealed internal class SecurityDocumentElement : ISecurityElementFactory
    {
        private int m_position;
        private SecurityDocument m_document;

        internal SecurityDocumentElement(SecurityDocument document, int position)
        {
            m_document = document;
            m_position = position;
        }

        SecurityElement ISecurityElementFactory.CreateSecurityElement()
        {
            return m_document.GetElement(m_position, true);
        }

        object ISecurityElementFactory.Copy()
        {
            return new SecurityDocumentElement(m_document, m_position);
        }

        string ISecurityElementFactory.GetTag()
        {
            return m_document.GetTagForElement(m_position);
        }

        string ISecurityElementFactory.Attribute(string attributeName)
        {
            return m_document.GetAttributeForElement(m_position, attributeName);
        }

    }

    [JsonObject]
    sealed internal class SecurityDocument
    {
        internal byte[] m_data;

        internal const byte c_element = 1;
        internal const byte c_attribute = 2;
        internal const byte c_text = 3;
        internal const byte c_children = 4;
        internal const int c_growthSize = 32;

        public SecurityDocument(int numData)
        {
            m_data = new byte[numData];
        }

        public SecurityDocument(byte[] data)
        {
            m_data = data;
        }

        public SecurityDocument(SecurityElement elRoot)
        {
            m_data = new byte[c_growthSize];

            int position = 0;
            ConvertElement(elRoot, ref position);
        }

        public void GuaranteeSize(int size)
        {
            if (m_data.Length < size)
            {
                byte[] m_newData = new byte[((size / c_growthSize) + 1) * c_growthSize];
                Array.Copy(m_data, 0, m_newData, 0, m_data.Length);
                m_data = m_newData;
            }
        }

        public void AddString(string str, ref int position)
        {
            GuaranteeSize(position + str.Length * 2 + 2);

            for (int i = 0; i < str.Length; ++i)
            {
                m_data[position + (2 * i)] = (byte)(str[i] >> 8);
                m_data[position + (2 * i) + 1] = (byte)(str[i] & 0x00FF);
            }
            m_data[position + str.Length * 2] = 0;
            m_data[position + str.Length * 2 + 1] = 0;

            position += str.Length * 2 + 2;
        }

        public void AppendString(string str, ref int position)
        {
            if (position <= 1 ||
                m_data[position - 1] != 0 ||
                m_data[position - 2] != 0)
                throw new XmlSyntaxException("error");

            position -= 2;

            AddString(str, ref position);
        }

        public static int EncodedStringSize(string str)
        {
            return str.Length * 2 + 2;
        }

        public string GetString(ref int position)
        {
            return GetString(ref position, true);
        }

        public string GetString(ref int position, bool bCreate)
        {
            int stringEnd;
            bool bFoundEnd = false;
            for (stringEnd = position; stringEnd < m_data.Length - 1; stringEnd += 2)
            {
                if (m_data[stringEnd] == 0 && m_data[stringEnd + 1] == 0)
                {
                    bFoundEnd = true;
                    break;
                }
            }

            Contract.Assert(bFoundEnd, "Malformed string in parse data");
            var m = SharedStatics.GetSharedStringMaker();
            try
            {

                if (bCreate)
                {
                    m._outStringBuilder = null;
                    m._outIndex = 0;

                    for (int i = position; i < stringEnd; i += 2)
                    {
                        char c = (char)(m_data[i] << 8 | m_data[i + 1]);

                        // add character  to the string 
                        if (m._outIndex < StringMaker.outMaxSize)
                        {
                            // easy case
                            m._outChars[m._outIndex++] = c;
                        }
                        else
                        {
                            if (m._outStringBuilder == null)
                            {
                                // OK, first check if we have to init the StringBuilder
                                m._outStringBuilder = new StringBuilder();
                            }

                            // OK, copy from _outChars to _outStringBuilder
                            m._outStringBuilder.Append(m._outChars, 0, StringMaker.outMaxSize);

                            // reset _outChars pointer 
                            m._outChars[0] = c;
                            m._outIndex = 1;
                        }
                    }
                }

                position = stringEnd + 2;

                if (bCreate)
                    return m.MakeString();
                else
                    return null;
            }
            finally
            {
                SharedStatics.ReleaseSharedStringMaker(ref m);
            }
        }
        
        public void AddToken(byte b, ref int position)
        {
            GuaranteeSize(position + 1);
            m_data[position++] = b;
        }

        public void ConvertElement(SecurityElement elCurrent, ref int position)
        {
            AddToken(c_element, ref position);
            AddString(elCurrent.m_strTag, ref position);

            if (elCurrent.m_lAttributes != null)
            {
                for (int i = 0; i < elCurrent.m_lAttributes.Count; i += 2)
                {
                    AddToken(c_attribute, ref position);
                    AddString((String)elCurrent.m_lAttributes[i], ref position);
                    AddString((String)elCurrent.m_lAttributes[i + 1], ref position);
                }
            }

            if (elCurrent.m_strText != null)
            {
                AddToken(c_text, ref position);
                AddString(elCurrent.m_strText, ref position);
            }

            if (elCurrent.InternalChildren != null)
            {
                for (int i = 0; i < elCurrent.InternalChildren.Count; ++i)
                {
                    ConvertElement((SecurityElement)elCurrent.Children[i], ref position);
                }
            }
            AddToken(c_children, ref position);
        }

        public SecurityElement GetRootElement()
        {
            return GetElement(0, true);
        }

        public SecurityElement GetElement(int position, bool bCreate)
        {
            SecurityElement elRoot = InternalGetElement(ref position, bCreate);
            return elRoot;
        }

        internal SecurityElement InternalGetElement(ref int position, bool bCreate)
        {
            if (m_data.Length <= position)
                throw new XmlSyntaxException();

            if (m_data[position++] != c_element)
                throw new XmlSyntaxException();

            SecurityElement elCurrent = null;
            var strTag = GetString(ref position, bCreate);
            if (bCreate)
                elCurrent = new SecurityElement(strTag);

            while (m_data[position] == c_attribute)
            {
                position++;
                var strName = GetString(ref position, bCreate);
                var strValue = GetString(ref position, bCreate);
                if (bCreate)
                    elCurrent.AddAttribute(strName, strValue);
            }

            if (m_data[position] == c_text)
            {
                position++;
                var strText = GetString(ref position, bCreate);
                if (bCreate)
                    elCurrent.m_strText = strText;
            }

            while (m_data[position] != c_children)
            {
                var elChild = InternalGetElement(ref position, bCreate);
                if (bCreate)
                    elCurrent.AddChild(elChild);
            }

            position++;
            return elCurrent;
        }

        public string GetTagForElement(int position)
        {
            if (m_data.Length <= position)
                throw new XmlSyntaxException();

            if (m_data[position++] != c_element)
                throw new XmlSyntaxException();

            var strTag = GetString(ref position);
            return strTag;
        }

        public ArrayList GetChildrenPositionForElement(int position)
        {
            if (m_data.Length <= position)
                throw new XmlSyntaxException();

            if (m_data[position++] != c_element)
                throw new XmlSyntaxException();

            var children = new ArrayList();

            // This is to move past the tag string
            GetString(ref position);

            while (m_data[position] == c_attribute)
            {
                position++;
                // Read name and value, then throw them away 
                GetString(ref position, false);
                GetString(ref position, false);
            }

            if (m_data[position] == c_text)
            {
                position++;
                // Read text, then throw it away.
                GetString(ref position, false);
            }

            while (m_data[position] != c_children)
            {
                children.Add(position);
                InternalGetElement(ref position, false);
            }

            position++;
            return children;
        }

        public string GetAttributeForElement(int position, String attributeName)
        {
            if (m_data.Length <= position)
                throw new XmlSyntaxException();

            if (m_data[position++] != c_element)
                throw new XmlSyntaxException();

            string strRetValue = null;
            // This is to move past the tag string. 
            GetString(ref position, false);


            while (m_data[position] == c_attribute)
            {
                position++;
                var strName = GetString(ref position);
                var strValue = GetString(ref position);

                if (string.Equals(strName, attributeName))
                {
                    strRetValue = strValue;
                    break;
                }
            }

            return strRetValue;
        }
    }
}
