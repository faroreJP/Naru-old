using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Naru.Value {
    public class CharacterString : ValueBase{
        private string str;

        public CharacterString() {

        }
        public CharacterString(string s) {
            str = s;
        }

        public override string TypeString {
            get {
                return "string";
            }
        }
        public override string ToString() {
            return str;
        }

        public override ValueBase Evaluates() {
            return this;
        }
        public override ValueBase Operates(string o, ValueBase r) {
            ValueBase result;
            var t = r.Evaluates();

            if (t is CharacterString) {
                //文字列同士の演算
                var er = (t as CharacterString);
                switch (o) {
                    case "+":
                        result = new CharacterString();
                        ((CharacterString)result).str = this.str + er.str;
                        break;

                    case "==":
                        result = new Bool(this.str == er.str);
                        break;

                    case "!=":
                        result = new Bool(this.str != er.str);
                        break;

                    default:
                        throw new ApplicationException(this.TypeString + " " + o + " " + er.TypeString + " は定義されていません。");
                }
            }
            else {
                result = new CharacterString();
                switch (o) {
                    case "+":
                        ((CharacterString)result).str = this.str + t.ToString();
                        break;

                    default:
                        throw new ApplicationException(this.TypeString + " " + o + " " + t.TypeString + " は定義されていません。");
                }
            }

            

            return result;
        }
        public override ValueBase Operates(string o) {
            ValueBase result;

            switch (o) {
                case "*":
                    result = new Numeric(str.Length.ToString(), false);
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
            else if (t == typeof(Numeric)) {
                var s = ToString();
                //var r = new Regex(@"^-?[0-9]*\.?[0-9]+$");
                if (MathDictionary.RegexNumber.IsMatch(s)) {
                    return new Numeric(s, false);
                }
                else {
                    throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
                }
            }
            else if (t == typeof(Complex)) {
                var s = ToString();
                var r = new Regex(@"^-?[0-9]+\.?[0-9]*([Ee]-?[0-9]+)?[+\-][0-9]+\.?[0-9]*([Ee]-?[0-9]+)?i$");
                if (r.IsMatch(s)) {
                    var rr = new Regex(@"^-?[0-9]+\.?[0-9]*([Ee]-?[0-9]+)?");
                    var m = rr.Match(s);

                    var res = m.Value;
                    var ims = s.Substring(m.Length, s.Length - m.Length - 1);

                    if (ims[0] == '+') {
                        ims = ims.Substring(1);
                    }

                    return new Complex(new Numeric(res, false), new Numeric(ims, false));
                }
                else {
                    throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
                }
            }
            else if (t == typeof(Bool)) {
                var s = ToString();
                if (s == "true") {
                    return new Bool(true);
                }
                else if (s == "false") {
                    return new Bool(false);
                }
                else {
                    throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
                }
            }
            else if (t == typeof(Byte1)) {
                var s = ToString();
                var r = new Regex(@"^0x[0-9ABCDEFabcdef]{1,2}$");
                if (r.IsMatch(s)) {
                    return new Byte1(s.Substring(2));
                }
                else {
                    throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
                }
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }
    }
}
