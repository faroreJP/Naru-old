using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naru.Value {
    public class Complex : ValueBase{
        public Numeric re {
            get;
            private set;
        }
        public Numeric im {
            get;
            private set;
        }

        public Complex() {
            re = new Numeric();
            im = new Numeric();
        }
        public Complex(Numeric r, Numeric i) {
            re = r;
            im = i;
        }

        public override string TypeString {
            get {
                return "complex";
            }
        }
        public override string ToString() {
            if (im.numerator >= 0) {
                return re.ToString() + "+" + im.ToString() + "i";
            }
            else {
                return re.ToString() + im.ToString() + "i";
            }
        }

        public override ValueBase Evaluates() {
            return this;
        }

        public override ValueBase Operates(string o, ValueBase r) {
            ValueBase result = null;
            var t = r.Evaluates();

            if (t is CharacterString) {
                if (o == "+") {
                    result = new CharacterString(ToString() + t.ToString());
                }
                else {
                    throw new ApplicationException("定義されていない演算です。");
                }
            }
            else if (t is Complex) {
                var er = (t as Complex);

                switch (o) {
                    case "==":
                        result = new Bool((bool)this.re.Operates(o, er.re).TryCast<bool>() && (bool)this.im.Operates(o, er.im).TryCast<bool>());
                        break;

                    case "!=":
                        result = new Bool((bool)this.re.Operates(o, er.re).TryCast<bool>() || (bool)this.im.Operates(o, er.im).TryCast<bool>());
                        break;

                    case ">=":
                        result = new Bool(Size(this) >= Size(er));
                        break;

                    case ">":
                        result = new Bool(Size(this) > Size(er));
                        break;

                    case "<=":
                        result = new Bool(Size(this) <= Size(er));
                        break;

                    case "<":
                        result = new Bool(Size(this) < Size(er));
                        break;

                    case "*":
                        result = new Complex();
                        ((Complex)result).re = (Numeric)((Numeric)this.re.Operates("*", er.re)).Operates("-", (Numeric)this.im.Operates("*", er.im));
                        ((Complex)result).im = (Numeric)((Numeric)this.re.Operates("*", er.im)).Operates("+", (Numeric)this.im.Operates("*", er.re));
                        break;

                    case "/":
                        result = new Complex();
                        var ddd=((Numeric)er.re.Operates("*", er.re)).Operates("+", (Numeric)er.im.Operates("*", er.im));
                        ((Complex)result).re = (Numeric)((Numeric)this.re.Operates("*", er.re)).Operates("+", (Numeric)this.im.Operates("*", er.im));
                        ((Complex)result).im = (Numeric)((Numeric)this.im.Operates("*", er.re)).Operates("-", (Numeric)this.re.Operates("*", er.im));

                        ((Complex)result).re = (Numeric)((Complex)result).re.Operates("/", ddd);
                        ((Complex)result).im = (Numeric)((Complex)result).im.Operates("/", ddd);
                        break;

                    default:
                        result = new Complex();
                        ((Complex)result).re = (Numeric)this.re.Operates(o, er.re);
                        ((Complex)result).im = (Numeric)this.im.Operates(o, er.im);
                        break;

                    case "^":
                        throw new ApplicationException("定義されていない演算です。");
                    case "%":
                        throw new ApplicationException("定義されていない演算です。");
                }
            }
            else if (t is Numeric) {
                return Operates(o, new Complex((Numeric)t, new Numeric()));
            }
            else {
                throw new ApplicationException(t.TypeString + " 型を " + TypeString + " 型に変換できません。");
            }

            return result;
        }
        public override ValueBase Operates(string o) {
            if (o == "-") {
                return new Complex((Numeric)re.Operates("-"), (Numeric)im.Operates("-"));
            }
            throw new ApplicationException("定義されていない演算です。");
        }

        public override object TryCast<T>() {
            var t = typeof(T);

            if (t == typeof(string)) {
                return ToString();
            }
            else if (t == typeof(Complex)) {
                return this;
            }
            else if (t == typeof(CharacterString)) {
                return new CharacterString(ToString());
            }

            throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
        }

        public static decimal Size(Complex c) {
            var d0 = c.re.GetValue();
            var d1 = c.im.GetValue();

            return (decimal)Math.Sqrt((double)(d0 * d0 + d1 * d1));
        }
        public static decimal Phase(Complex c) {
            return (decimal)Math.Atan2((double)c.im.TryCast<double>(), (double)c.re.TryCast<double>());
        }
        public static Numeric Re(Complex c) {
            return c.re;
        }
        public static Numeric Im(Complex c) {
            return c.im;
        }

        public static Complex Instatiate(Numeric r, Numeric i) {
            return new Complex(r, i);
        }
        public static Complex Round(Complex c, decimal d) {
            var d0 = new Numeric(Decimal.Round(c.re.GetValue(), (int)d), 1, true);
            var d1 = new Numeric(Decimal.Round(c.re.GetValue(), (int)d), 1, true);
            return new Complex(d0, d1);
        }
        public static Complex Unit(Complex c) {
            return (Complex)c.Operates("/", new Numeric(Size(c).ToString(), false));
        }
    }
}
