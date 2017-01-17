using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreedyKidEditor
{
    public sealed class Building
    {
        public string Name = "";

        public List<Level> Levels = new List<Level>();

        public Building(string name)
        {
            Name = name;
        }
    }
}
