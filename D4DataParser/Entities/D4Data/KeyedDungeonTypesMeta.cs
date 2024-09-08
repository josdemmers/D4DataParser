using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace D4DataParser.Entities.D4Data
{
    public class KeyedDungeonTypesMeta
    {
        public List<PtData> ptData { get; set; }
    }

    public class PtData
    {
        public List<TEntries> tEntries { get; set; }
    }

    public class TEntries
    {
        public THeader tHeader { get; set; }
        //public List<ArDungeonLists> arDungeonLists { get; set; }
        public List<ArDungeons> arDungeons { get; set; }
    }

    public class THeader
    {
        public string szName { get; set; }
    }

    //public class ArDungeonLists
    //{
    //    public List<ArDungeons> arDungeons { get; set; }
    //}

    public class ArDungeons
    {
        public long __raw__ { get; set; }
        public string name { get; set; }
    }
}
