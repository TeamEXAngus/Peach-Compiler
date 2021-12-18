using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.CodeAnalysis.Types
{
    internal sealed class List
    {
        private readonly List<object> contents = new();

        public void Add(object element)
        {
            contents.Add(element);
        }

        public object ElementAt(int index)
        {
            return contents.ElementAtOrDefault(index) ?? new object();
        }

        public override string ToString()
        {
            var SB = new StringBuilder("[");

            for (int i = 0; i < contents.Count; i++)
            {
                SB.Append(contents[i].ToString());

                if (i + 1 < contents.Count)
                    SB.Append(", ");
            }

            SB.Append(']');

            return SB.ToString();
        }
    }
}