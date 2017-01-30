using System.Collections.Generic;

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
