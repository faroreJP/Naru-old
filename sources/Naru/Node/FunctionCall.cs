using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;
using Naru.Value;

namespace Naru.Node {
    public class FunctionCall : NodeBase{
        private FormulaAnalyzer[] arguments;
        private string funcName;
        
        public FunctionCall(MathDictionary dic, string name, string[] args) : base(dic){
            funcName = name;
            arguments = new FormulaAnalyzer[args.Length];
            for (int i = 0; i < args.Length; i++) {
                arguments[i] = new FormulaAnalyzer(args[i], dic);
            }
        }
        public override ValueBase Evaluates() {
            var a = new ValueBase[arguments.Length];
            for (int i = 0; i < a.Length; i++) {
                a[i] = arguments[i].Evaluates();
            }

            var f = dictionary.GetFunction(funcName, a);
            //Console.WriteLine(f.FunctionName + ":" + f.ArgumentNum);
            return f.Evaluates(a);
        }
    }
}
