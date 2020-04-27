using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{
    public class MetaTreeEditNode : MetaTreeNode
    {
        public string Comments;
        public bool Disabled;

        public object[] ToObjectArray()
        {
            object[] childrenArray = { Type.ToString(), Label, Name, Target, Comments, Disabled };
            return childrenArray;
        }
    }
}
