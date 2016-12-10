using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceBrace : SentenceBase{
        private ScriptAnalyzer scAnalyzer;


        public SentenceBrace(ScriptAnalyzer owner, string s) : base(owner, "0"){
            scAnalyzer = new ScriptAnalyzer(s, owner.Dictionary);
        }

        public override void Executes() {
            scAnalyzer.Dictionary.Scope++;
            scAnalyzer.Executes();
            scAnalyzer.Dictionary.Scope--;
        }
        public override int GetCount() {
            return scAnalyzer.LineCountAll();
        }
    }
}
