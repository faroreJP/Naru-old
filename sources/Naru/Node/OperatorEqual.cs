using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class OperatorEqual : NodeBase{
        private NodeBase variable;
        private NodeBase node;

        public OperatorEqual(MathDictionary dic, NodeBase v, NodeBase n) : base(dic){
            variable = v;
            node = n;
        }

        public override Value.ValueBase Evaluates() {
            if (!(variable is Variable)) {
                throw new ApplicationException("= 演算子の左辺には変数のみが指定できます。");
            }

            var name = ((Variable)variable).VariableName;

            //値を入手
            var value = variable.Evaluates();
            var right = node.Evaluates();
            ValueBase result;

            if (value == null) {
                //変数が初期化されていない場合
                result = right;
            }
            else if (value.EqualsType(right)) {
                result = right;
            }
            else if (value is Reference) {
                return EvaluatesReference(value, right);
            }
            else {
                result = right.Evaluates();
                if (!value.EqualsType(result)) {
                    throw new ApplicationException(right.TypeString + " を " + value.TypeString + " に変換できません。");
                }
            }

            dictionary.SetVariable(name, result);
            return result;
        }

        private ValueBase EvaluatesReference(ValueBase value, ValueBase right) {
            var vr = (value as Reference);
            var vn = vr.variableName;
            var v = value.Operates("*");

            if (!v.EqualsType(right)) {
                throw new ApplicationException(right.TypeString + " を " + v.TypeString + " に変換できません。");
            }

            var result = right;
            dictionary.SetVariable("&"+vn, result);
            return result;
        }
    }
}
