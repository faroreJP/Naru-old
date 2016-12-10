using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class OperatorUnary : NodeBase{
        
        protected string opr;
        protected NodeBase term;

        public OperatorUnary(MathDictionary dic, string opr, NodeBase term) : base(dic){
            this.opr = opr;
            this.term = term;
        }

        public override ValueBase Evaluates() {
            if (opr == "++" || opr == "--") {
                var t = term as Variable;
                if (t == null) {
                    throw new ApplicationException(opr + " の右側のオペランドは変数でなくてはなりません。");
                }

                var v = dictionary.GetVariable(t.VariableName) as Numeric;
                if (v == null) {
                    throw new ApplicationException(opr + v.TypeString + " - 定義されていない演算です。");
                }

                Numeric n;
                if (opr == "++") n = new Numeric((v.numerator + 1), v.denominator, v.IsIrrational());
                else n = new Numeric((v.numerator - 1), v.denominator, v.IsIrrational());

                dictionary.SetVariable(t.VariableName, n);
                return n;
            }
            else {
                return term.Evaluates().Operates(opr);
            }
        }
    }
}
