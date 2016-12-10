using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naru.Value;

namespace Naru.Function {
    public class BuiltIn2Operand<ARG1, ARG2, RET> : FunctionBase {
        private Func<ARG1, ARG2, RET> function;
        private bool isIrrational;
        private bool takeOverIrrational;

        public BuiltIn2Operand(MathDictionary dic, Func<ARG1, ARG2, RET> f, string name, string arg1, string arg2)
            : base(dic, name, new string[] { arg1 , arg2 }) {
            function = f;
        }
        public BuiltIn2Operand(MathDictionary dic, Func<ARG1, ARG2, RET> f, string name, string arg1, string arg2, bool irr)
            : base(dic, name, new string[] { arg1, arg2 }) {
            function = f;
            isIrrational = irr;
        }
        public BuiltIn2Operand(MathDictionary dic, Func<ARG1, ARG2, RET> f, string name, string arg1, string arg2, bool irr, bool takeover)
            : base(dic, name, new string[] { arg1, arg2 }) {
            function = f;
            isIrrational = irr;
            takeOverIrrational = takeover;
        }

        public override ValueBase Evaluates(ValueBase[] args) {
            var t = typeof(RET);
            ARG1 a = (ARG1)args[0].TryCast<ARG1>();
            ARG2 b = (ARG2)args[1].TryCast<ARG2>();
            RET r = function(a, b);

            if (t == typeof(ValueBase) || t.IsSubclassOf(typeof(ValueBase))) {
                return r as ValueBase;
            }
            else if (t == typeof(double) || t == typeof(decimal)) {
                return new Numeric(r.ToString(), takeOverIrrational ? (isIrrational && ((Numeric)args[0].Evaluates().Evaluates()).IsIrrational()) : isIrrational);
            }
            else if (t == typeof(string)) {
                return new CharacterString(r.ToString());
            }
            else if (t == typeof(bool)) {
                return new Bool(r.ToString() == "true");
            }
            else {
                throw new ApplicationException(t.Name + " : 戻り値の型がよく分かりません。");
            }
        }

        
    }
}
