using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public class Bool : ValueBase{
        private bool value;

        public Bool() {
            value = false;
        }
        public Bool(bool f) {
            value = f;
        }


        public override string TypeString {
            get {
                return "boolean";
            }
        }
        public override string ToString() {
            return value ? "true" : "false";
        }
        public bool GetValue() {
            return value;
        }

        public override ValueBase Evaluates() {
            return this;
        }
        public override ValueBase Operates(string o, ValueBase r) {
            var t = r.Evaluates();
            if (!(t is Bool)) {
                throw new ApplicationException(t.TypeString + " 型を " + TypeString + " 型に変換できません。");
            }

            var er = (t as Bool);
            var result = new Bool();

            switch (o) {
                case "||":
                    result.value = this.value || er.value;
                    break;

                case "&&":
                    result.value = this.value && er.value;
                    break;

                case "==":
                    result.value = (this.value == er.value);
                    break;

                case "!=":
                    result.value = (this.value != er.value);
                    break;

                default:
                    throw new ApplicationException("定義されていない演算です。");
            }

            return result;
        }
        public override ValueBase Operates(string o) {
            ValueBase result;

            switch (o) {
                case "!":
                    result = new Bool(!value);
                    break;

                default:
                    throw new ApplicationException("定義されていない演算です。");
            }

            return result;
        }

        public override Object TryCast<T>() {
            var t = typeof(T);
            if (t == typeof(string)) {
                return ToString();
            }
            else if (t == typeof(Bool)) {
                return new Bool(this.value);
            }
            else if (t == typeof(CharacterString)) {
                return new CharacterString(ToString());
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }
    }
}
