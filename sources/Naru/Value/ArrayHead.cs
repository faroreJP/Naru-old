using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public class ArrayHead : ValueBase{
        public int Length {
            get;
            private set;
        }
        public string Name {
            get;
            private set;
        }

        public ArrayHead(string name, int length) {
            Name = name;
            this.Length = length;
        }

        public override string TypeString {
            get {
                return "array";
            }
        }
        public override string ToString() {
            return Name;
        }

        public override ValueBase Evaluates() {
            return this;
        }

        public override ValueBase Operates(string o, ValueBase r) {
            if (r is CharacterString) {
                var t = new CharacterString(ToString());
                return t.Operates(o, r);
            }
            throw new ApplicationException("定義されていない演算です。");
        }

        public override ValueBase Operates(string o) {
            if (o == "*") {
                return new Numeric(Length.ToString(), false);
            }
            throw new NotImplementedException();
        }

        public override object TryCast<T>() {
            var t = typeof(T);
            if (t == typeof(CharacterString)) {
                return new CharacterString(ToString());
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }


    }
}
