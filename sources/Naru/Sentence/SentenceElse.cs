using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Analyzer;

namespace Naru.Sentence {
    public class SentenceElse : SentenceBase{
        
        public SentenceElse(ScriptAnalyzer owner, string s) : base(owner, s) {
        }

        public override void Executes() {
            //owner.Seek(1);
        }
    }
}
