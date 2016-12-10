using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;
using Naru.Analyzer;

namespace Naru.Node {
    public class ConstantDeclaration : NodeBase{
        private string name;
        private NodeBase node;

        public ConstantDeclaration(MathDictionary dic, string name, NodeBase init) : base(dic) {
            if (!name.Contains('[')) {
                this.name = name;
                node = init;
                if (node == null) {
                    throw new ApplicationException("定数には初期値が必要です。");
                }
                dictionary.AddConstant(name, node.Evaluates());
            }
            else {
                throw new ApplicationException("配列の定数は宣言できません。");
            }
        }

        public override ValueBase Evaluates() {
            return new Numeric("0", false);
        }
    }
}
