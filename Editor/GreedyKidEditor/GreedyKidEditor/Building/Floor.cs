using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreedyKidEditor
{
    public sealed class Floor
    {
        public string Name = "";

        public int BackgroundColor = 0;

        public int LeftMargin = 0;
        public int RightMargin = 0;

        public int LeftDecoration = 0;
        public int RightDecoration = 0;

        public const int PaintCount = 4;
        public const int DecorationCount = 5;
    }
}
