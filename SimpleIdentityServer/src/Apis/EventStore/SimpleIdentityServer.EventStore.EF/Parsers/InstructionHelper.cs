#region copyright
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
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public static class InstructionHelper
    {
        public static KeyValuePair<string, string>? ExtractInstruction(string instruction)
        {
            var indOpen = instruction.IndexOf('(');
            var indEnd = instruction.IndexOf(')');
            if (indOpen == -1 || indEnd == -1)
            {
                return null;
            }

            return new KeyValuePair<string, string>(instruction.Substring(0, indOpen), instruction.Substring(indOpen + 1, indEnd - indOpen - 1));
        }

        public static KeyValuePair<string, string>? ExtractAggregateInstruction(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
            {
                return null;
            }

            instruction = Regex.Replace(instruction, @"\s+", "");
            var splitted = instruction.Split(new string[] { "with" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Count() != 2)
            {
                return null;
            }

            return new KeyValuePair<string, string>(splitted.First(), splitted.ElementAt(1));
        }
    }
}
