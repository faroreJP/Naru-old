using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Function {
    public class BuiltIn0Operand<RET> : FunctionBase{
        private Func<RET> function;

        public BuiltIn0Operand(MathDictionary dic, Func<RET> f, string name)
            : base(dic, name, new string[0]{}) {
            function = f;
        }

        public override ValueBase Evaluates(ValueBase[] args) {
            var t = typeof(RET);
            RET r = function();

            if (t == typeof(ValueBase) || t.IsSubclassOf(typeof(ValueBase))) {
                return r as ValueBase;
            }
            else if (t == typeof(double) || t == typeof(decimal)) {
                return new Numeric(r.ToString(), false);
            }
            else if (t == typeof(string)) {
                return new CharacterString(r.ToString());
            }
            else if (t == typeof(bool)) {
                return new Bool(r.ToString() == "True");
            }
            else {
                throw new ApplicationException(t.Name + " : 戻り値の型がよく分かりません。");
            }
        }
    }
}
