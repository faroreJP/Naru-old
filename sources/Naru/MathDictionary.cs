using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.IO;

using Naru.Value;
using Naru.Function;

namespace Naru {
    public enum DictionaryType {
        Variable,
        Constant,
        Function
    }

    /// <summary>
    /// Naruで使用する定数、変数、関数及びキーワードを保持するクラスです。
    /// </summary>
    public class MathDictionary {
        public static readonly string ReturnVariableName = "@RETURN";

        public static readonly string RegexStringNumber = @"^-?[0-9]+\.?[0-9]*([Ee]-?[0-9]+)?$";
        public static readonly string RegexStringName = @"^[a-zA-Z_][a-zA-Z0-9_]*\$?[0-9]*$";
        public static readonly string RegexStringOperator = @"^[\+\-\*\/\%=\^><!\|\&,]+";
        public static readonly Regex RegexNumber = new Regex(RegexStringNumber);
        public static readonly Regex RegexName = new Regex(RegexStringName);
        public static readonly Regex RegexOperator = new Regex(RegexStringOperator);

        public static readonly string[] ReverseOperators = new string[]{
            "-","/"
        };
        public static readonly string[] Operators = new string[]{
            "&&","||",
            "==","!=",">=",">","<=","<",
            "&","|",
            "+","-","*","/","%","^",
            "++","--",
            ","
        };
        public static readonly string[] ReservedWords = new string[]{
            "include",
            "if","else","while","for","function","return",
            "var","const",
            "numeric","string","boolean","formula","array","reference",
            "true","false"
        };
        public static readonly string[] AnalyzeWords = new string[]{
            "var ", "const ","return ","function ","else ",
            "numeric ","string ","boolean ","formula ","array ","reference "
        };

        private Dictionary<string, ValueBase> variables;
        private Dictionary<string, ValueBase> constants;
        private List<FunctionBase> functions;
        private Dictionary<string, string> functionTexts;
        private Dictionary<string, string> constantTexts;

        private List<string> IncludedFiles;

        private int nextUnique;
        private int scope;

        public int Scope {
            get {
                return scope;
            }
            set {
                if (value < scope) {
                    //全てのアレ削除
                    var s = GetVariableName("");
                    var l=new List<KeyValuePair<string, ValueBase>>();
                    foreach (var v in variables) {
                        if (!v.Key.EndsWith(s)) {
                            l.Add(v);
                        }
                    }
                    variables.Clear();
                    foreach (var v in l) {
                        variables.Add(v.Key, v.Value);
                    }
                }

                scope = value;
            }
        }
        public int UniqueNumber {
            get {
                nextUnique++;
                return nextUnique;
            }
        }
        
        /// <summary>
        /// 規定値でMathDictionaryを初期化します。
        /// </summary>
        public MathDictionary() {
            variables = new Dictionary<string, ValueBase>();
            constants = new Dictionary<string, ValueBase>();
            functions = new List<FunctionBase>();
            functionTexts = new Dictionary<string, string>();
            constantTexts = new Dictionary<string, string>();
            IncludedFiles = new List<string>();

            AddConstant("pi", new Numeric(Math.PI.ToString(), true));
            AddConstant("E", new Numeric(Math.E.ToString(), true));


            //---------------Built-In Functions-----------------

            //---------Cast--------
            AddFunction(new BuiltInCast<Numeric>(this, "to_numeric", "x:string"));
            AddFunction(new BuiltInCast<Complex>(this, "to_complex", "x:string"));
            AddFunction(new BuiltInCast<Bool>(this, "to_boolean", "x:string"));
            AddFunction(new BuiltInCast<Byte1>(this, "to_byte", "x:string"));

            AddFunction(new BuiltInCast<CharacterString>(this, "to_string", "x:numeric"));
            AddFunction(new BuiltInCast<CharacterString>(this, "to_string", "x:complex"));
            AddFunction(new BuiltInCast<CharacterString>(this, "to_string", "x:boolean"));
            AddFunction(new BuiltInCast<CharacterString>(this, "to_string", "x:byte"));
            AddFunction(new BuiltInCast<CharacterString>(this, "to_string", "x:formula"));

            AddFunction(new BuiltInCast<Formula>(this, "to_formula", "x:numeric"));
            AddFunction(new BuiltInCast<Formula>(this, "to_formula", "x:complex"));
            AddFunction(new BuiltInCast<Formula>(this, "to_formula", "x:boolean"));
            AddFunction(new BuiltInCast<Formula>(this, "to_formula", "x:byte"));
            AddFunction(new BuiltInCast<Formula>(this, "to_formula", "x:string"));

            AddFunction(new BuiltIn1Operand<Numeric, string>(this, BuiltIn1Operand<Numeric, string>.Typeof, "typeof", "x:numeric"));
            AddFunction(new BuiltIn1Operand<Bool, string>(this, BuiltIn1Operand<Bool, string>.Typeof, "typeof", "x:boolean"));
            AddFunction(new BuiltIn1Operand<Byte1, string>(this, BuiltIn1Operand<Byte1, string>.Typeof, "typeof", "x:byte"));
            AddFunction(new BuiltIn1Operand<CharacterString, string>(this, BuiltIn1Operand<CharacterString, string>.Typeof, "typeof", "x:string"));
            AddFunction(new BuiltIn1Operand<Formula, string>(this, BuiltIn1Operand<Formula, string>.Typeof, "typeof", "x:formula"));

            //---------Math--------
            string x="x:numeric", y="y:numeric";
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Cos, "cos", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Sin, "sin", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Tan, "tan", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Acos, "acos", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Asin, "asin", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Atan, "atan", x, true));
            AddFunction(new BuiltIn2Operand<double, double, double>(this, Math.Atan2, "atan2", y, x, true));

            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Abs, "abs", x, true, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Ceiling, "ceil", x, false));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Floor, "floor", x, false));
            AddFunction(new BuiltIn2Operand<decimal, decimal, Numeric>(this, Numeric.Round, "round", x, y, true));

            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Exp, "exp", x, true));
            AddFunction(new BuiltIn2Operand<double, double, double>(this, Math.Pow, "pow", x, y, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Sqrt, "sqrt", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Log, "ln", x, true));
            AddFunction(new BuiltIn1Operand<double, double>(this, Math.Log10, "log10", x, true));
            AddFunction(new BuiltIn2Operand<double, double, double>(this, Math.Log, "log", x, y, true));

            //---------Print--------
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:numeric"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:boolean"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:byte"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:string"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:formula"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.Print, "print", "x:complex"));

            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:numeric"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:boolean"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:byte"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:string"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:formula"));
            AddFunction(new BuiltIn1Operand<string, double>(this, BuiltIn1Operand<string, double>.PrintLn, "println", "x:complex"));

            //--------complex-------
            AddFunction(new BuiltIn2Operand<Numeric, Numeric, Complex>(this, Complex.Instatiate, "_cmp", "re:numeric", "im:numeric"));
            AddFunction(new BuiltIn1Operand<Complex, Numeric>(this, Complex.Re, "re", "x:complex"));
            AddFunction(new BuiltIn1Operand<Complex, Numeric>(this, Complex.Im, "im", "x:complex"));
            AddFunction(new BuiltIn1Operand<Complex, decimal>(this, Complex.Size, "size", "x:complex"));
            AddFunction(new BuiltIn1Operand<Complex, decimal>(this, Complex.Phase, "phase", "x:complex"));
            AddFunction(new BuiltIn2Operand<Complex, decimal, Complex>(this, Complex.Round, "round", "x:complex", "y:numeric"));
            AddFunction(new BuiltIn1Operand<Complex, Complex>(this, Complex.Unit, "unit", "x:complex"));

            //--------array---------
            AddFunction(new BuiltInArray(this, "new_array"));

            //---------ETC----------
            AddFunction(new BuiltIn1Operand<decimal, Numeric>(this, Numeric.Precision, "digit", "x:numeric"));
            AddFunction(new BuiltIn2Operand<Byte1, Byte1, Numeric>(this, Byte1.Hamming, "hamming", "x:byte", "y:byte"));
            AddFunction(new BuiltIn0Operand<Numeric>(this, Numeric.Rand, "rand"));
            AddFunction(new BuiltIn1Operand<decimal, Numeric>(this, Numeric.Rand, "rand", "x:numeric"));
            AddFunction(new BuiltIn3Operand<Formula, Formula, Formula, ValueBase>(this, Formula.Condition, "cond", "c:formula", "t:formula", "f:formula"));

        }

        private string GetVariableName(string name) {
            if (name.Length>0 && name[0] == '&') {
                return name.Substring(1);
            }
            else {
                var n = name;
                for (int i = 0; i < Scope; i++) {
                    n += "@";
                }
                return n;
            }
        }
        public void AddVariable(string name) {
            if (name == ReturnVariableName) {
                variables.Add(GetVariableName(name), null);
                return;
            }

            if (!RegexName.IsMatch(name)) {
                throw new ApplicationException(name + " : 使用できない名前です。");
            }
            if (constants.ContainsKey(name) || ReservedWords.Contains(name)) {
                throw new ApplicationException(name + " は既に使用されています。");
            }

            var n=GetVariableName(name);
            if (variables.ContainsKey(n)) {
                throw new ApplicationException(name+" は同一スコープ内で既に宣言されています。");
            }
            variables.Add(n, null);
        }
        public void SetVariable(string name, ValueBase value){
            var _t = value as Numeric;
            if (_t != null) {
                if (_t.numerator == 0 && _t.denominator == 0) {
                    ;
                }
            }

            var n = GetVariableName(name);
            while (true) {
                if (!variables.ContainsKey(n)) {
                    if (n.EndsWith("@")) {
                        //見つからなかった場合、一つ上のスコープを検索
                        n = n.Substring(0, n.Length - 1);
                        continue;
                    }
                    else {
                        //どこにもない
                        throw new ApplicationException(name + " は宣言されていません。");
                    }
                }

                //見つかった
                variables[n] = value;
                break;
            }
        }
        public ValueBase GetVariable(string name) {
            var n = GetVariableName(name);
            while(true){
                if (!variables.ContainsKey(n)) {
                    if (n.EndsWith("@")) {
                        //見つからなかった場合、一つ上のスコープを検索
                        n = n.Substring(0, n.Length - 1);
                        continue;
                    }
                    else {
                        //どこにもない
                        throw new ApplicationException(name + " は宣言されていません。");
                    }
                }

                //見つかった
                return variables[n];
            }
        }
        public string FindVariable(string baseName) {
            var n = GetVariableName(baseName);
            while (true) {
                if (!variables.ContainsKey(n)) {
                    if (n.EndsWith("@")) {
                        //見つからなかった場合、一つ上のスコープを検索
                        n = n.Substring(0, n.Length - 1);
                        continue;
                    }
                    else {
                        //どこにもない
                        return null;
                    }
                }

                //見つかった
                return n;
            }
        }

        public void AddConstant(string name, ValueBase value) {
            if (!RegexName.IsMatch(name)) {
                throw new ApplicationException(name+" : 使用できない名前です。");
            }

            if (value == null) {
                throw new ApplicationException(name + " : 値は非 null でなければなりません。");
            }
            if (constants.ContainsKey(name) || ReservedWords.Contains(name)) {
                throw new ApplicationException(name + " は既に使用されています。");
            }
            var rg=new System.Text.RegularExpressions.Regex("^"+name+"@*");
            if (variables.Any( (KeyValuePair<string, ValueBase> k) => { return rg.IsMatch(k.Key); })) {
                throw new ApplicationException(name + " は既に使用されています。");
            }

            constants.Add(name, value);
        }
        public ValueBase GetConstants(string name) {
            if (constants.ContainsKey(name)) {
                return constants[name];
            }
            return null;
        }

        public void AddFunction(FunctionBase f) {
            foreach (var tf in functions) {
                if (tf.IsMatch(f)) {
                    throw new ApplicationException(f.FunctionName + " は既に定義されている関数です。");
                }
            }

            functions.Add(f);
        }
        public FunctionBase GetFunction(string name, ValueBase[] args) {
            foreach (var tf in functions) {
                if (tf.IsMatch(name, args)) {
                    return tf;
                }
            }
            throw new ApplicationException(name + " は定義されていません。");
        }

        public void Include(string fileName) {
            if (IncludedFiles.Contains(fileName)) {
                Console.WriteLine("worning : ファイル " + fileName + " が再度インクルードされました。");
            }
            else {
                using (var s = new StreamReader(new FileStream(fileName, FileMode.Open), Encoding.GetEncoding("UTF-8"))) {
                    var an = new Analyzer.ScriptAnalyzer(s.ReadToEnd(), this);
                }
            }
        }
        public void LoadFuncText(string filePath) {
            try {
                using (var stream = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.GetEncoding("UTF-8"))) {
                    while (!stream.EndOfStream) {
                        var s = stream.ReadLine().Split(':');
                        if (s.Length == 2) {
                            functionTexts.Add(s[0], s[1]);
                        }
                    }
                }
            }
            catch (Exception e) {

            }
        }
        public void LoadConstText(string filePath) {
            try {
                using (var stream = new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.GetEncoding("UTF-8"))) {
                    while (!stream.EndOfStream) {
                        var s = stream.ReadLine().Split(':');
                        if (s.Length == 2) {
                            constantTexts.Add(s[0], s[1]);
                        }
                    }
                }
            }
            catch (Exception e) {

            }
        }

        /// <summary>
        /// 存在する定数、変数、関数を全て表示します。
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("---Constants---");
            foreach (var v in constants) {
                sb.AppendLine(v.Value.TypeString + " " + v.Key + "=" + v.Value.ToString());
            }

            sb.AppendLine("---Variables---");
            foreach (var v in variables) {
                if (v.Value == null) {
                    sb.AppendLine("null " + v.Key);
                }
                else {
                    sb.AppendLine(v.Value.TypeString + " " + v.Key + "=" + v.Value.ToString());
                }
            }

            sb.AppendLine("---Functions---");
            foreach (var f in functions) {
                sb.AppendLine(f.ToString());
            }

            return sb.ToString();
        }

        public Numeric PrintConstants() {
            var sb = new StringBuilder();
            sb.AppendLine("---Constants---");
            foreach (var v in constants) {
                sb.AppendLine(v.Value.TypeString + " " + v.Key + "=" + v.Value.ToString());
            }

            Console.WriteLine(sb.ToString());
            return new Numeric();
        }
        public Numeric PrintVariables() {
            var sb=new StringBuilder();
            sb.AppendLine("---Variables---");
            foreach (var v in variables) {
                if (v.Value == null) {
                    sb.AppendLine("null " + v.Key);
                }
                else {
                    sb.AppendLine(v.Value.TypeString + " " + v.Key + "=" + v.Value.ToString());
                }
            }

            Console.WriteLine(sb.ToString());
            return new Numeric();
        }
        public Numeric PrintFunctions() {
            var sb = new StringBuilder();
            sb.AppendLine("---Functions---");
            foreach (var f in functions) {
                sb.AppendLine(f.ToString());
            }

            Console.WriteLine(sb.ToString());
            return new Numeric();
        }

        /// <summary>
        /// 指定の要素を取得します。
        /// "c"  - 定数|
        /// "cn" - 定数名|
        /// "v"  - 変数|
        /// "vn" - 変数名|
        /// "f"  - 関数|
        /// "fn" - 関数名|
        /// "fs" - 関数名と引数リスト
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object[] GetValues(string type) {
            var ol = new List<object>();

            for (int i = 0; i < type.Length; i++) {
                if (type[i] == 'c') {
                    //定数
                    if (i + 1 < type.Length && type[i + 1] == 'n') {
                        ol.AddRange(constants.Keys);
                    }
                    else {
                        ol.AddRange(constants.Values);
                    }
                }
                else if (type[i] == 'v') {
                    //変数
                    if (i + 1 < type.Length && type[i + 1] == 'n') {
                        ol.AddRange(variables.Keys);
                    }
                    else {
                        ol.AddRange(variables.Values);
                    }
                }
                else if (type[i] == 'f') {
                    //変数
                    if (i + 1 < type.Length && type[i + 1] == 'n') {
                        foreach (var f in functions) {
                            ol.Add(f.FunctionName);
                        }
                    }
                    else if (i + 1 < type.Length && type[i + 1] == 's') {
                        foreach (var f in functions) {
                            ol.Add(f.ToString());
                        }
                    }
                    else {
                        ol.AddRange(functions);
                    }
                }
            }

            return ol.ToArray();
        }

        public Dictionary<string, string> GetFuncTexts(){
            return functionTexts;
        }
        public Dictionary<string, string> GetConstTexts() {
            return constantTexts;
        }
    }
}
