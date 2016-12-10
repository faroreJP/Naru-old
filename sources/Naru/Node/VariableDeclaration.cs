using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;
using Naru.Analyzer;

namespace Naru.Node {
    public class VariableDeclaration : Variable {
        private FormulaAnalyzer length;

        public VariableDeclaration(MathDictionary dic, string name) : base(dic, name) {
            if (!name.Contains('[')) {
                this.name = name;
            }
            else {
                int index = name.IndexOf('[');
                this.name = name.Substring(0, index);
                var ls = name.Substring(index + 1, name.IndexOf(']') - index - 1);
                length = new FormulaAnalyzer(ls, dic);
            }
        }

        public override ValueBase Evaluates() {
            if (length == null) {
                dictionary.AddVariable(name);
            }
            else {
                var len = length.Evaluates();
                if (len is Numeric) {
                    int l = (int)(((Numeric)len).GetValue());
                    if (l <= 0) {
                        throw new ApplicationException("長さが0または負の配列を作成する事はできません。");
                    }
                    string aName = "array" + dictionary.UniqueNumber;

                    dictionary.AddVariable(name);
                    dictionary.SetVariable(name, new ArrayHead(aName, l));
                    for (int i = 0; i < l; i++) {
                        dictionary.AddVariable(aName + "$" + i);
                    }
                }
                else {
                    throw new ApplicationException("配列の長さは Numeric 型でなければなりません。");
                }
            }

            return base.Evaluates();
        }
    }
}
