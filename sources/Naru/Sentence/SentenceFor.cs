using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceFor : SentenceBase{
        private SentenceBase thenSentence;
        private FormulaAnalyzer[] loop;
        
        public SentenceFor(ScriptAnalyzer owner, string c, SentenceBase t) : base(owner, "0") {
            thenSentence = t;
            loop = new FormulaAnalyzer[3];
            var tt = FormulaAnalyzer.DevideFormula2(c, ';');
            var ttt = FormulaAnalyzer.DevideFormula2(tt[1], ';');
            loop[0] = new FormulaAnalyzer(tt[0], owner.Dictionary);
            loop[1] = new FormulaAnalyzer(ttt[0], owner.Dictionary);
            loop[2] = new FormulaAnalyzer(ttt[1], owner.Dictionary);
        }
        
        public override void Executes() {
            string r;

            owner.Dictionary.Scope++;
            for (loop[0].Evaluates(); !ScriptAnalyzer.Terminate; loop[2].Evaluates()) {
                r = loop[1].Evaluates().ToString();
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
