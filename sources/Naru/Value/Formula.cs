using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;
using Naru.Node;

namespace Naru.Value {
    public class Formula : ValueBase{
        private FormulaAnalyzer analyzer;
        private string formula;

        public Formula(MathDictionary dic, string f) {
            analyzer = new FormulaAnalyzer(f, dic);
            formula = f;
        }

        public override string TypeString {
            get {
                return "formula";
            }
        }
        public override string ToString() {
            return formula;
        }

        public override ValueBase Evaluates() {
            return analyzer.Evaluates();
        }
        public override ValueBase Operates(string o, ValueBase r) {
            if (r is Formula) {
                var fr = (Formula)r;
                ValueBase result;

                switch (o) {
                    case "+":
                        result = new Formula(analyzer.Dictionary, "(" + this.formula + ")+(" + fr.formula + ")");
                        break;

                    case "-":
                        result = new Formula(analyzer.Dictionary, "(" + this.formula + ")-(" + fr.formula + ")");
                        break;

                    case "*":
                        result = new Formula(analyzer.Dictionary, "(" + this.formula + ")*(" + fr.formula + ")");
                        break;

                    case "/":
                        result = new Formula(analyzer.Dictionary, "(" + this.formula + ")/(" + fr.formula + ")");
                        break;

                    case "==":
                        result = new Bool(this.formula == fr.formula);
                        break;

                    case "!=":
                        result = new Bool(this.formula != fr.formula);
                        break;

                    default:
                        throw new ApplicationException("定義されていない演算です。");
                }


                return result;
            }
            else {
                var l = Evaluates();
                var t = r.Evaluates();
                return l.Operates(o, t);
            }
        }
        public override ValueBase Operates(string o) {
            if (o == "*") {
                return Evaluates();
            }

            throw new ApplicationException("定義されていない演算です。");
        }

        public override Object TryCast<T>() {
            var t = typeof(T);
            if (t == typeof(double) || t == typeof(decimal)) {
                return Evaluates().TryCast<T>();
            }
            else if (t == typeof(string)) {
                return ToString();
            }
            else if (t == typeof(Formula)) {
                return new Formula(this.analyzer.Dictionary, this.formula);
            }
            else if (t == typeof(CharacterString)) {
                return new CharacterString(ToString());
            }
            else {
                throw new ApplicationException(TypeString + " 型を " + t.Name + " 型に変換できません。");
            }
        }


        public static ValueBase Condition(Formula a1, Formula a2, Formula a3) {
            var b = (Bool)a1.Evaluates().TryCast<Bool>();
            if (b.GetValue()) {
                return a2.Evaluates();
            }
            else {
                return a3.Evaluates();
            }
        }
    }
}
