﻿#region copyright
// Copyright 2017 Habart Thierry
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class FilterParser
    {
        private const char _instructionSeparator = '$';
        private const string _selectInstruction = "select";
        private const string _whereInstruction = "where";
        private static IEnumerable<string> _supportedInstructions = new List<string>
        {
            _selectInstruction,
            _whereInstruction
        };
        private static IEnumerable<char> _openSign = new[]
        {
            '(',
            '['
        };
        private static IEnumerable<char> _closeSign = new[]
        {
            ')',
            ']'
        };

        public SelectInstruction Parse(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var instructions = SplitInstructions(path);
            return ExtractInstruction(instructions);
        }

        private static SelectInstruction ExtractInstruction(IEnumerable<string> instructions)
        {
            SelectInstruction selectInstruction = null;
            WhereInstruction whereInst = null;
            foreach(var instruction in instructions)
            {
                var attrs = SplitInstruction(instruction);
                if (!attrs.Any())
                {
                    throw new InvalidOperationException($"the instruction {instruction} is not valid");
                }
                
                var method = attrs.First();
                var parameter = attrs.ElementAt(1);
                if (string.Equals(method, _selectInstruction, StringComparison.CurrentCultureIgnoreCase))
                {
                    selectInstruction = new SelectInstruction(parameter);
                }
                else if (string.Equals(method, _whereInstruction, StringComparison.CurrentCultureIgnoreCase))
                {
                    IEnumerable<string> parameters;
                    whereInst = new WhereInstruction();
                    if (IsInstruction(parameter, out parameters))
                    {
                        parameters = SplitInstructions(parameter);
                        whereInst.SetInstruction(ExtractInstruction(parameters));
                    }
                    else
                    {
                        whereInst.SetParameter(parameter);
                    }
                }
            }

            if (selectInstruction == null)
            {
                selectInstruction = new SelectInstruction();
            }

            selectInstruction.WhereInst = whereInst;
            return selectInstruction;
        }

        public static bool IsInstruction(string instruction, out IEnumerable<string> parameters)
        {
            parameters = null;
            if (string.IsNullOrWhiteSpace(instruction))
            {
                return false;
            }

            var splitted = instruction.Split(_instructionSeparator);
            if (!splitted.Any() || !_supportedInstructions.Contains(splitted.First()) || splitted.Count() != 2)
            {
                return false;
            }

            parameters = new[] 
            {
                splitted.First(),
                splitted.ElementAt(1).TrimStart('(').TrimEnd(')')
            };
            return true;

        }

        private static IEnumerable<string> SplitInstruction(string instruction)
        {
            IEnumerable<string> parameters;
            if (!IsInstruction(instruction, out parameters))
            {
                return new string[0];
            }

            return parameters;
        }

        private static IEnumerable<string> SplitInstructions(string filter)
        {
            return SplitStr(filter, ' ');
        }

        private static IEnumerable<string> SplitStr(string str, char separator)
        {
            int i = 0,
                level = 0;
            var strBuilder = new StringBuilder();
            var attrs = new List<string>();
            foreach (var character in str)
            {
                i++;
                if (_closeSign.Contains(character))
                {
                    level--;
                }

                // 1. Add the character.
                if (i == str.Length ||
                    (level == 0 && character != separator) ||
                    (level > 0))
                {
                    strBuilder.Append(character);
                }

                // 2. Add string.
                if (level == 0 && (character == separator || i == str.Length))
                {
                    attrs.Add(strBuilder.ToString());
                    strBuilder.Clear();
                    continue;
                }

                if (_openSign.Contains(character))
                {
                    level++;
                }
            }

            return attrs;
        }

        private static IEnumerable<int> FindAllIndexes(IEnumerable<string> parameters, IEnumerable<string> lst)
        {
            var result = new List<int>();
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (lst.Contains(parameter))
                {
                    result.Add(index);
                }

                index++;
            }

            return result.OrderBy(d => d);
        }
    }
}
