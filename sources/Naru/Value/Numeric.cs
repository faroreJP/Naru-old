using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Naru.Value {
    public class Numeric : ValueBase{
        private decimal ___d;

        public decimal numerator {
            get;
            private set;
        }
        public decimal denominator {
            get {
                return ___d;
            }
            private set {
                if (value == 0) {
                    throw new ApplicationException("分母は0にはなれない");
                }
                ___d = value;
            }
        }
        private bool isIrrational;

        public static readonly Regex RegexNumber = new Regex(@"^[0-9]+\.?[0-9]*E-?[0-9]*$");

        public Numeric() {
            numerator = 0;
            denominator = 1;
            isIrrational = false;
        }
        public Numeric(string s, bool irr) {
            bool isMinus = false;
            if (s[0] == '-') {
                isMinus = true;
                s = s.Substring(1);
            }
            
            try {
                if (RegexNumber.IsMatch(s)) {
                    numerator = Decimal.Parse(s, System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint);
                }
                else {
                    numerator = Decimal.Parse(s);
                }
            }
            catch (Exception e) {
                throw e;
            }
            
            denominator = 1;
            isIrrational = irr;

            if (GetPrecision(numerator) == 0) {
                //やっぱり有理数じゃないか
                isIrrational = false;
            }

            GCD();
            if (isMinus) {
                numerator *= (-1);
            }
        }
        public Numeric(decimal n, decimal d, bool irr) {
            numerator = n;
            denominator = d;
            isIrrational = irr;

            if (GetPrecision(numerator) == 0) {
                //やっぱり有理数じゃないか
                isIrrational = false;
            }
            GCD();
        }
        
        public override string TypeString{
            get {
                return "numeric";
            }
        }
        public override string ToString() {
            if (isIrrational) {
                return "" + (numerator / denominator);
            }
            else if (denominator != 1) {
                return decimal.Truncate(numerator) + "/" + decimal.Truncate(denominator);
            }
            else {
                return "" + numerator;
            }
        }
        public decimal GetValue() {
            try {
                return numerator / denominator;
            }
            catch (Exception e) {
                throw e;
            }
        }
        public bool IsIrrational() {
            return isIrrational;
        }

        public override ValueBase Evaluates() {
            return this;
        }
        public override ValueBase Operates(string o, ValueBase r) {
            ValueBase result=null;
            var t = r.Evaluates();

            if (t is CharacterString) {
                if (o == "+") {
                    result = new CharacterString(ToString() + t.ToString());
                }
                else {
                    throw new ApplicationException("定義されていない演算です。");
                }
            }
            else if (t is Numeric) {
                var er = (t as Numeric);
                result = new Numeric();
                switch (o) {
                    case "+":
                        result = new Numeric();
                        try {
                            if (this.denominator != er.denominator) {
                                ((Numeric)result).denominator = this.denominator * er.denominator;
                                ((Numeric)result).numerator = this.numerator * er.denominator + er.numerator * this.denominator;
                            }
                            else {
                                ((Numeric)result).denominator = this.denominator;
                                ((Numeric)result).numerator = this.numerator + er.numerator;
                            }
                        }
                        catch (OverflowException ofe) {
                            ((Numeric)result).denominator = 1;
                            ((Numeric)result).numerator = this.GetValue() + er.GetValue();
                        }
                        ((Numeric)result).isIrrational = (this.isIrrational || er.isIrrational);
                        ((Numeric)result).GCD();
                        break;

                    case "-":
                        result = new Numeric();
                        try {
                            if (this.denominator != er.denominator) {
                                ((Numeric)result).denominator = this.denominator * er.denominator;
                                ((Numeric)result).numerator = this.numerator * er.denominator - er.numerator * this.denominator;
                            }
                            else {
                                ((Numeric)result).denominator = this.denominator;
                                ((Numeric)result).numerator = this.numerator - er.numerator;
                            }
                        }
                        catch (OverflowException ofe) {
                            ((Numeric)result).denominator = 1;
                            ((Numeric)result).numerator = this.GetValue() - er.GetValue();
                        }
                        ((Numeric)result).isIrrational = (this.isIrrational || er.isIrrational);
                        ((Numeric)result).GCD();
                        break;

                    case "*":
                        result = new Numeric();
                        try {
                            ((Numeric)result).denominator = this.denominator * er.denominator;
                            ((Numeric)result).numerator = this.numerator * er.numerator;
                        }
                        catch (OverflowException ofe) {
                            ((Numeric)result).denominator = 1;
                            ((Numeric)result).numerator = this.GetValue() * er.GetValue();
                        }
                        ((Numeric)result).isIrrational = (this.isIrrational || er.isIrrational);
                        ((Numeric)result).GCD();
                        break;

                    case "/":
                        result = new Numeric();
                        try {
                            ((Numeric)result).denominator = this.denominator * er.numerator;
                            ((Numeric)result).numerator = this.numerator * er.denominator;
                        }
                        catch (OverflowException ofe) {
                            ((Numeric)result).denominator = 1;
                            ((Numeric)result).numerator = this.GetValue() / er.GetValue();
                        }
                        ((Numeric)result).isIrrational = (this.isIrrational || er.isIrrational);
                        ((Numeric)result).GCD();
                        break;

                    case "%":
                        result = new Numeric();
                        ((Numeric)result).denominator = 1;
                        ((Numeric)result).numerator = decimal.Remainder(this.GetValue(), er.GetValue());
                        ((Numeric)result).isIrrational = false;
                        ((Numeric)result).GCD();
                        break;

                    case "^":
                        var x = (double)er.GetValue();
                        result = new Numeric();
                        try {
                            ((Numeric)result).denominator = (decimal)Math.Pow((double)this.denominator, x);
                            ((Numeric)result).numerator = (decimal)Math.Pow((double)this.numerator, x);
                        }
                        catch (OverflowException ofe) {
                            ((Numeric)result).denominator = 1;
                            ((Numeric)result).numerator = (decimal)Math.Pow((double)this.GetValue(), (double)x);
                        }
                        ((Numeric)result).isIrrational = (this.isIrrational || er.isIrrational);
                        ((Numeric)result).GCD();
                        break;

                    case "==":
                        result = new Bool(this.GetValue() == er.GetValue());
                        break;

                    case "!=":
                        result = new Bool(this.GetValue() != er.GetValue());
                        break;

                    case ">=":
                        result = new Bool(this.GetValue() >= er.GetValue());
                        break;

                    case ">":
                        result = new Bool(this.GetValue() > er.GetValue());
                        break;

                    case "<=":
                        result = new Bool(this.GetValue() <= er.GetValue());
                        break;

                    case "<":
                        result = new Bool(this.GetValue() < er.GetValue());
                        break;

                    default:
                        throw new ApplicationException("定義されていない演算です。");
                }
            }
            else if (t is Complex) {
                var c = new Complex(this, new Numeric());
                return c.Operates(o, t);
            }
            else {
                throw new ApplicationException(t.TypeString + " 型を " + TypeString + " 型に変換できません。");
            }

            return result;
        }
        public override ValueBase Operates(string o) {
            var result = new Numeric();

            switch (o) {
                case "-":
                    result.numerator = numerator * (-1);
                    result.denominator = denominator;
                    result.isIrrational = isIrrational;
                    break;

                default:
                    throw new ApplicationException("定義されていない演算です。");
            }

            return result;
        }

        private void GCD() {

            //小数点の整理
            var dd = GetPrecision(denominator);
            var dn = GetPrecision(numerator);
            if (dd > 0 && dn > 0) {
                decimal p;
                if (dd < dn) {
                    p = (decimal)Math.Pow(10, dd);
                }
                else {
                    p = (decimal)Math.Pow(10, dn);
                }
                denominator *= p;
                numerator *= p;
            }

            if (isIrrational) {
                return;
            }

            bool reserveArmy = (denominator == 1);//無理数予備軍

            //整数値の準備
            bool minus=(numerator*denominator<0);
            if (numerator * 1 < 0) numerator *= -1;
            if (denominator * 1 < 0) denominator *= -1;

            var tn = Math.Pow(10, GetPrecision(numerator));
            var td = Math.Pow(10, GetPrecision(denominator));
            var tp = (tn > td ? tn : td);

            var pn = numerator;
            var pd = denominator;

            try {
                numerator *= (decimal)tp;
                denominator *= (decimal)tp;

                int m = decimal.ToInt32(numerator);
                int n = decimal.ToInt32(denominator);
                int r;
                if (m < n) {
                    int t = m;
                    m = n;
                    n = t;
                }

                //互除法
                while (n != 0) {
                    r = m % n;
                    m = n;
                    n = r;
                }

                //約分
                if (m == 1 && reserveArmy) {
                    numerator /= denominator;
                    denominator = 1;
                    isIrrational = true;
                }
                else {
                    numerator /= m;
                    denominator /= m;
                }
            }
            catch (OverflowException oe) {
                numerator = pn / pd;
                denominator = 1;
                isIrrational = true;
            }

            //符号
            if (minus) numerator *= -1;
        }

        private static int GetPrecision(decimal price) {
            string priceString = price.ToString().TrimEnd('0');

            int index = priceString.IndexOf('.');
            if (index == -1)
                return 0;

            return priceString.Substring(index + 1).Length;
        }
        public static Numeric Precision(decimal price) {
            return new Numeric(GetPrecision(price).ToString(), false);
        }
        public static Numeric Round(decimal x, decimal d) {
            int dd = (int)d;
            return new Numeric(Decimal.Round(x, dd).ToString(), true);
        }
        public static Numeric Rand() {
            var r=new Random();
            return new Numeric(r.NextDouble().ToString(), false);
        }
        public static Numeric Rand(decimal s) {
            var r = new Random(Decimal.ToInt32(s));
            return new Numeric(r.NextDouble().ToString(), false);
        }

        public override Object TryCast<T>() {
            var t = typeof(T);
            if (t == typeof(double)) {
                return (double)GetValue();
            }
            else if (t == typeof(decimal)) {
                return GetValue();
            }
            else if (t == typeof(string)) {
                return ToString();
            }
            else if (t == typeof(Numeric)) {
                var r = new Numeric();
                r.numerator = this.numerator;
                r.denominator = this.denominator;
                r.isIrrational = this.isIrrational;
                return r;
            }
            else if(t==typeof(CharacterString)){
                return new CharacterString(ToString());
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }
    }
}
