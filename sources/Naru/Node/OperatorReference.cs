using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class OperatorReference : OperatorUnary{
        public OperatorReference(MathDictionary dic, NodeBase term) : base(dic, "&", term) {
        }

        public override ValueBase Evaluates() {
            var t = term as Variable;

            if (t != null) {
                var s = dictionary.FindVariable(t.VariableName);
                if (s == "") {
                    throw new ApplicationException(s + " は存在しない変数です。");
                }

                return new Reference(dictionary, s);
            }

            throw new ApplicationException("& の右側のオペランドは変数でなくてはなりません。");
        }
    }
}
