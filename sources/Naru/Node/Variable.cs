using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Node {
    /// <summary>
    /// 変数の"参照"
    /// </summary>
    public class Variable : NodeBase{
        protected string name;

        public Variable(MathDictionary dic, string n) : base(dic) {
            name = n;
        }

        public virtual string VariableName {
            get {
                return name;
            }
        }
        
        public override ValueBase Evaluates() {
            return dictionary.GetVariable(VariableName);
        }
    }
}
