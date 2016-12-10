using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Function {
    public class BuiltInArray : FunctionBase{
        public BuiltInArray(MathDictionary dic, string name) : base(dic, name, new string[0]){

        }

        public override bool IsMatch(string name, ValueBase[] args) {
            return this.FunctionName==name && args.Length>0;
        }
        public override bool IsMatch(FunctionBase f) {
            return this.FunctionName == f.FunctionName;
        }

        public override ValueBase Evaluates(ValueBase[] args) {
            string aName = "array" + dictionary.UniqueNumber;

            dictionary.AddVariable(name);
            dictionary.SetVariable(name, new ArrayHead(aName, args.Length));
            for (int i = 0; i < args.Length; i++) {
                var vName=aName + "$" + i;
                dictionary.AddVariable(vName);
                dictionary.SetVariable(vName, args[i]);
            }

            return dictionary.GetVariable(name);
        }

        public override string ToString() {
            return FunctionName + "(...)";
        }

        public  ArrayHead Map(ArrayHead a, Formula f) {
            throw new NotImplementedException();
        }

    }
}
