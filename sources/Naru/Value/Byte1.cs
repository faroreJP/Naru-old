using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public class Byte1: ValueBase{
        private byte value;

        public Byte1() {
            value = 0;
        }
        public Byte1(byte b) {
            value = b;
        }
        public Byte1(string s) {
            value = Byte.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
        }


        public override string TypeString {
            get {
                return "byte";
            }
        }
        public override string ToString() {
            return "0x"+value.ToString("X2");
        }

        public override ValueBase Evaluates() {
            return this;
        }

        public override ValueBase Operates(string o, ValueBase r) {
            ValueBase result = null;
            var t = r.Evaluates();

            if (t is CharacterString) {
                if (o == "+") {
                    return new CharacterString(ToString() + t.ToString());
                }
                else {
                    throw new ApplicationException("定義されていない演算です。");
                }
            }
            else if(t is Byte1){
                var er = (Byte1)t;
                switch (o) {
                    case "|":
                        result = new Byte1((byte)(this.value | er.value));
                        break;

                    case "&":
                        result = new Byte1((byte)(this.value & er.value));
                        break;

                    case "^":
                        result = new Byte1((byte)(this.value ^ er.value));
                        break;

                    case "==":
                        result = new Bool(this.value == er.value);
                        break;
                    case "!=":
                        result = new Bool(this.value != er.value);
                        break;

                    case ">=":
                        result = new Bool(this.value >= er.value);
                        break;
                    case ">":
                        result = new Bool(this.value > er.value);
                        break;

                    case "<=":
                        result = new Bool(this.value <= er.value);
                        break;
                    case "<":
                        result = new Bool(this.value < er.value);
                        break;

                }
                return result;
            }
            else {
                throw new ApplicationException(t.TypeString + " 型を " + TypeString + " 型に変換できません。");
            }
        }

        public override ValueBase Operates(string o) {
            if (o == "!") {
                return new Byte1((byte)(this.value ^ 0xff));
            }

            throw new ApplicationException("定義されていない演算です。");
        }
        public override Object TryCast<T>() {
            var t = typeof(T);
            if (t == typeof(string)) {
                return ToString();
            }
            else if (t == typeof(Byte1)) {
                return new Byte1(this.value);
            }
            else if (t == typeof(CharacterString)) {
                return new CharacterString(ToString());
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }

        public static Numeric Hamming(Byte1 a, Byte1 b) {
            var va = a.value;
            var vb = b.value;

            int dh=0;
            for (int i = 0; i < 16; i++) {
                if ((va & 0x1) != (vb & 0x1)) {
                    dh++;
                }
                va = (byte)(va >> 1);
                vb = (byte)(vb >> 1);
            }

            return new Numeric(dh.ToString(), false);
        }
    }
}
