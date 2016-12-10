using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class OperatorBinary : NodeBase{
        private string opr;
        private NodeBase left;
        private NodeBase right;

        public OperatorBinary(MathDictionary dic, string opr, NodeBase left, NodeBase right) : base(dic){
            this.opr = opr;
            this.left = left;
            this.right = right;
        }

        public override ValueBase Evaluates() {
            if (opr != ",") {
                return left.Evaluates().Operates(opr, right.Evaluates());
            }
            else {
                var r = left.Evaluates();
                right.Evaluates();
                return r;
            }
        }
    }
}
