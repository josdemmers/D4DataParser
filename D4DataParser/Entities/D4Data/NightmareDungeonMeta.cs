using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D4DataParser.Entities.D4Data
{
    public class NightmareDungeonMeta
    {
        public List<PtContent> ptContent { get; set; }
    }

    public class PtContent
    {
        public List<ArDungeonLists> arDungeonLists { get; set; }
    }

    public class ArDungeonLists
    {
        public List<ArDungeons> arDungeons { get; set; }
    }

    public class ArDungeons
    {
        public string name { get; set; }
    }
}
