using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceReturn : SentenceBase{
        
        public SentenceReturn(ScriptAnalyzer owner, string s) : base(owner, s){
        }

        public override void Executes() {
            analyzer.Dictionary.SetVariable(MathDictionary.ReturnVariableName, analyzer.Evaluates());
            owner.Exit();
        }
    }
}
