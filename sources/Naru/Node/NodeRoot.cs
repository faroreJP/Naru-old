using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class NodeRoot : NodeBase{
        private NodeBase child;

        public NodeRoot(MathDictionary dic, NodeBase chd) : base(dic) {
            child = chd;
        }

        public override ValueBase Evaluates() {
            return child.Evaluates();
        }
    }
}
