using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceWhile : SentenceBase{
        private SentenceBase thenSentence;
        
        public SentenceWhile(ScriptAnalyzer owner, string c, SentenceBase t) : base(owner, c) {
            thenSentence = t;
        }
        
        public override void Executes() {
            string r;

            owner.Dictionary.Scope++;
            while (!ScriptAnalyzer.Terminate) {
                r = analyzer.Evaluates().Evaluates().ToString();
                if (r == "true") {
                    thenSentence.Executes();
                }
                else if (r == "false") {
                    break;
                }
                else {
                    throw new ApplicationException("条件式は boolean となる必要があります。");
                }
            }
            owner.Dictionary.Scope--;
        }
        public override int GetCount() {
            return 1 + thenSentence.GetCount();
        }
    }
}
