using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru.Value;

namespace Naru.Function {
    public abstract class FunctionBase{
        protected MathDictionary dictionary;
        protected string name;

        protected string[] ArgumentNames;
        protected string[] ArgumentTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="name"></param>
        /// <param name="args">"numeric x" or "y:string"</param>
        public FunctionBase(MathDictionary dic, string name, string[] args) {
            dictionary = dic;
            this.name = name;

            ArgumentNames = new string[args.Length];
            ArgumentTypes = new string[args.Length];
            for (int i = 0; i < args.Length;i++ ) {
                if (args[i].Contains(' ')) {
                    var t = args[i].Split(' ');
                    ArgumentNames[i] = t[1];
                    ArgumentTypes[i] = t[0];
                }
                else if (args[i].Contains(':')) {
                    var t = args[i].Split(':');
                    ArgumentNames[i] = t[0];
                    ArgumentTypes[i] = t[1];
                }
                else {
                    throw new ApplicationException("引数の書式が不正です。");
                }
            }
        }

        public string FunctionName {
            get {
                return name;
            }
        }
        public int ArgumentNum {
            get {
                return ArgumentNames.Length;
            }
        }

        public virtual bool IsMatch(string name, ValueBase[] args) {
            if (FunctionName != name || ArgumentNames.Length != args.Length) {
                return false;
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].TypeString != ArgumentTypes[i]) {
                    return false;
                }
            }

            return true;
        }
        public virtual bool IsMatch(FunctionBase f) {
            if (this.FunctionName != f.FunctionName || this.ArgumentNum != f.ArgumentNum) {
                return false;
            }
            for (int i = 0; i < ArgumentNum; i++) {
                if (this.ArgumentTypes[i] != f.ArgumentTypes[i]) {
                    return false;
                }
            }
            return true;
        }
        public abstract ValueBase Evaluates(ValueBase[] args);

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append(FunctionName+"(");
            for (int i = 0; i < ArgumentNum; i++) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append(ArgumentTypes[i] + " " + ArgumentNames[i]);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
