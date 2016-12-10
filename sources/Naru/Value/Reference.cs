using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public class Reference : ValueBase{
        private MathDictionary dictionary;
        public string variableName {
            get;
            private set;
        }

        public Reference(MathDictionary dic) {
            dictionary = dic;
        }
        public Reference(MathDictionary dic, string name) {
            dictionary = dic;
            variableName = name;
        }

        public override string TypeString {
            get { 
                return "reference";
            }
        }

        public override ValueBase Evaluates() {
            return this;
        }

        public override ValueBase Operates(string o, ValueBase r) {
            throw new ApplicationException("定義されていない演算です。");
        }

        public override ValueBase Operates(string o) {
            if (o == "*") {
                return dictionary.GetVariable("&" + variableName);
            }
            throw new ApplicationException("定義されていない演算です。");
        }

        public override object TryCast<T>() {
            throw new ApplicationException(TypeString+" は型変換できません。");
        }
    }
}
