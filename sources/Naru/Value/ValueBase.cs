using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public abstract class ValueBase {
        public bool EqualsType(ValueBase v) {
            return TypeString == v.TypeString;
        }

        public abstract string TypeString {
            get;
        }
        public abstract ValueBase Evaluates();
        public abstract ValueBase Operates(string o, ValueBase r);
        public abstract ValueBase Operates(string o);
        public abstract Object TryCast<T>();
    }
}
