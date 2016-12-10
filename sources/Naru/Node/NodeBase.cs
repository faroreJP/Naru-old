using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public abstract class NodeBase {
        protected MathDictionary dictionary;

        public NodeBase(MathDictionary dic) {
            dictionary = dic;
        }
        public abstract ValueBase Evaluates();
    }
}
