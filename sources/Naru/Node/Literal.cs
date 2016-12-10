using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    public class Literal : NodeBase{
        private ValueBase value;

        public Literal(MathDictionary dic, string s) : base(dic){
            try {
                if (s == "true") value = new Bool(true);
                else if (s == "false") value = new Bool(false);
                else if (s[0] == '\"') value = new CharacterString(s.Substring(1, s.Length - 2));
                else if (s[0] == '[') value = new Formula(dic, s.Substring(1, s.Length - 2));
                else if (s.StartsWith("0x")) value = new Byte1(s.Substring(2));
                else if (s.EndsWith("i")) value = new Complex(new Numeric(), new Numeric(s.Substring(0, s.Length - 1), false));
                else value = new Numeric(s, false);
            }
            catch (Exception e) {
                throw new ApplicationException("Literal:"+s, e);
            }
        }
        public Literal(MathDictionary dic, ValueBase v) : base(dic) {
            value = v;
        }

        public override ValueBase Evaluates() {
            return value;
        }
    }
}
