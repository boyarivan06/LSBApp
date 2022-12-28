using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSB
{
    internal class Action
    {
        public short type { get; set; }
        public DateTime time { get; set; }
        public List<List<int>> coords { get; set; }
        public string image_path { get; set; }
    }
}
