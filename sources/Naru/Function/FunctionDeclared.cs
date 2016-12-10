using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;
using Naru.Value;

namespace Naru.Function {
    public class FunctionDeclared : FunctionBase{
        private ScriptAnalyzer analyzer;

        public FunctionDeclared(MathDictionary dic, string name, string[] args, string scr) : base(dic, name, args) {
            analyzer = new ScriptAnalyzer(scr, dic);
        }

        public override ValueBase Evaluates(ValueBase[] args) {
            analyzer.Dictionary.Scope++;

            analyzer.Dictionary.AddVariable(MathDictionary.ReturnVariableName);
            analyzer.Dictionary.SetVariable(MathDictionary.ReturnVariableName, new Numeric());

            //引数の登録
            for (int i = 0; i < args.Length; i++) {
                var n = ArgumentNames[i];
                analyzer.Dictionary.AddVariable(n);
                analyzer.Dictionary.SetVariable(n, args[i]);
            }
            analyzer.Executes();
            var r = analyzer.Dictionary.GetVariable(MathDictionary.ReturnVariableName);

            analyzer.Dictionary.Scope--;

            return r;
        }

    }
}
