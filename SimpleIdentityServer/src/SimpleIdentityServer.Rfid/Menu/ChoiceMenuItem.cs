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

using System;
using System.Linq;
using System.Collections.Generic;

namespace SimpleIdentityServer.Rfid.Menu
{
    internal class ChoiceMenuItem : BaseMenuItem
    {
        private readonly IList<BaseMenuItem> _items;

        public ChoiceMenuItem()
        {
            _items = new List<BaseMenuItem>
            {
                new ExitMenuItem()
            };
        }

        public ChoiceMenuItem(string title) : base(title)
        {
            _items = new List<BaseMenuItem>
            {
                new ExitMenuItem()
            };
        }

        public void Add(BaseMenuItem item)
        {
            _items.Add(item);
            item.SetParent(this);
        }

        public override void SetParent(BaseMenuItem item)
        {
            base.SetParent(item);
            _items.Add(new BackMenuItem(item));
        }

        public override void Execute()
        {
            var item = ChooseOption();
            item.Execute();
            Execute();
        }

        private BaseMenuItem ChooseOption()
        {
            Console.WriteLine("\t Choose the option \t");
            int i = 0;
            foreach (var item in _items)
            {
                Console.WriteLine($"{i} : \t {item.Title}");
                i++;
            }

            int option;
            if (!int.TryParse(Console.ReadLine(), out option) || option < 0 || option >= _items.Count)
            {
                Console.WriteLine("Please choose a correct option ...");
                return ChooseOption();
            }

            return _items.ElementAt(option);
        }
    }
}
