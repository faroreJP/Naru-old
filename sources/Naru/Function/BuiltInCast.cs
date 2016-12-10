using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Function {
    public class BuiltInCast<T> : FunctionBase where T : ValueBase{
        public BuiltInCast(MathDictionary dic, string name, string arg) : base(dic, name, new string[]{arg}) {

        }

        public override ValueBase Evaluates(ValueBase[] args) {
            if (typeof(T) == typeof(Formula)) {
                return new Formula(dictionary, args[0].ToString());
            }
            else {
                return (ValueBase)args[0].TryCast<T>();
            }
        }

    }
}
