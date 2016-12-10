using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceIf : SentenceBase{
        private SentenceBase thenSentence;
        private SentenceBase elseSentence;

        public SentenceIf(ScriptAnalyzer owner, string c, SentenceBase t) : base(owner, c) {
            thenSentence = t;
        }
        public void SetElse(SentenceBase e) {
            elseSentence = e;
        }
        public SentenceBase getElse(){
            return elseSentence;
        }
        public Boolean hasElseIf() {
            return elseSentence is SentenceIf;
        }

        public override void Executes() {
            var r = analyzer.Evaluates().Evaluates().ToString();

            if (r == "true") {
                thenSentence.Executes();
            }
            else if (r == "false") {
                if (elseSentence != null) {
                    elseSentence.Executes();
                }
            }
            else {
                throw new ApplicationException("条件式は boolean となる必要があります。");
            }
        }
        public override int GetCount() {
            return 1 + thenSentence.GetCount() + (elseSentence == null ? 0 : elseSentence.GetCount());
        }

    }
}
