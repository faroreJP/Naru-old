using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;
using Naru.Value;

namespace Naru.Node {
    public class VariableArray : Variable{
        private FormulaAnalyzer head;
        private FormulaAnalyzer index;

        public VariableArray(MathDictionary dic, string n, string i) : base(dic, n) {
            head = new FormulaAnalyzer(n, dic);
            index = new FormulaAnalyzer(i, dic);
        }

        public override string VariableName {
            get {
                //配列名の取り出し
                ValueBase h = head.Evaluates();
                while( h is Reference && !ScriptAnalyzer.Terminate) {
                    h = ((Reference)h).Operates("*"); ;
                }
                if (!(h is ArrayHead)) {
                    throw new ApplicationException(h.ToString() + " - 存在しない配列です。");
                }

                var i = index.Evaluates();
                if (i is Numeric) {
                    return ((ArrayHead)h).Name + "$" + ((int)((Numeric)i).GetValue());
                }
                else {
                    throw new ApplicationException("配列のインデックスは Numeric 型でなければなりません。");
                }
            }
        }

    }
}
