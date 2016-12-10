using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Function {
    public class BuiltIn1Operand<ARG, RET> : FunctionBase{

        private Func<ARG, RET> function;
        private bool isIrrational;
        private bool takeOverIrrational;

        public BuiltIn1Operand(MathDictionary dic, Func<ARG, RET> f, string name, string arg)
            : base(dic, name, new string[] { arg }) {
            function = f;
        }
        public BuiltIn1Operand(MathDictionary dic, Func<ARG, RET> f, string name, string arg, bool irr)
            : base(dic, name, new string[] { arg }) {
            function = f;
            isIrrational = irr;
        }
        public BuiltIn1Operand(MathDictionary dic, Func<ARG, RET> f, string name, string arg, bool irr, bool takeover)
            : base(dic, name, new string[] { arg }) {
            function = f;
            isIrrational = irr;
            takeOverIrrational = takeover;
        }

        public override ValueBase Evaluates(ValueBase[] args) {
            var t = typeof(RET);
            ARG a = (ARG)args[0].TryCast<ARG>();
            RET r = function(a);

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
                return new Bool(r.ToString() == "True");
            }
            else {
                throw new ApplicationException(t.Name + " : 戻り値の型がよく分かりません。");
            }
        }


        public static double Print(ARG s) {
            var str = s.ToString();
            Console.Write(str);
            return str.Length;
        }
        public static double PrintLn(ARG s) {
            var str = s.ToString();
            Console.WriteLine(str);
            return str.Length;
        }
        public static string Typeof(ARG s) {
            var v = s as ValueBase;
            if (v == null) {
                throw new ApplicationException("引数の型がおかしいです。");
            }
            return v.TypeString;
        }
    }
}
