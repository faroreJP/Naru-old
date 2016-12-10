using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceBase {
        protected ScriptAnalyzer owner;
        protected FormulaAnalyzer analyzer;

        public SentenceBase(ScriptAnalyzer owner, string s) {
            this.owner = owner;
            analyzer = new FormulaAnalyzer(s, owner.Dictionary);
        }

        public virtual void Executes() {
            analyzer.Evaluates();
        }

        public virtual int GetCount() {
            return 1;
        }

        public override string ToString() {
            return analyzer.ToString();
        }
    }
}
