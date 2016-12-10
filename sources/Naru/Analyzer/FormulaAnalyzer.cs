using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Naru.Node;

namespace Naru.Analyzer {
    public class FormulaAnalyzer {
        /// <summary>
        /// 変数・関数の名前制限
        /// </summary>

        private MathDictionary dictionary;
        private string sentence;
        private NodeBase node;

        public FormulaAnalyzer(string s, MathDictionary dic) {
            sentence = s;
            dictionary = dic;

            //解析開始
            node = new NodeRoot(dic, Analyzes(s));
        }

        public override string ToString() {
            return sentence;
        }

        public MathDictionary Dictionary {
            get {
                return dictionary;
            }
        }

        public Value.ValueBase Evaluates() {
            return node.Evaluates();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        private NodeBase Analyzes(string s) {
            //括弧
            if (s[0] == '(') {
                var ss = SubString(s, '(', ')');
                if (ss.Length == s.Length - 2) {
                    return Analyzes(ss);
                }
            }

            //真偽値リテラル
            if (s == "true" || s == "false") {
                return new Literal(dictionary, s);
            }

            //式、文字列リテラル
            if (s[0] == '\"') {
                var ss = SubString(s, '\"', '\"');
                if (ss.Length == s.Length - 2) {
                    return new Literal(dictionary, s);
                }
            }
            if (s[0] == '[') {
                var ss = SubString(s, '[', ']');
                if (ss.Length == s.Length - 2) {
                    return new Literal(dictionary, s);
                }
            }

            //定数宣言
            if (s.StartsWith("const ")) {
                var d = DevideFormula(s, "=");
                if (d != null) {
                    return new ConstantDeclaration(dictionary, d[0].Substring(6).Trim(), Analyzes(d[2]));
                }
                else {
                    return new ConstantDeclaration(dictionary, s.Substring(6).Trim(), null);
                }
            }

            //カンマ
            {
                var d = DevideFormula(s, ",");
                if (d != null) {
                    return new OperatorBinary(dictionary, d[1], Analyzes(d[0]), Analyzes(d[2]));
                }
            }

            //代入
            {
                var d = DevideFormula(s, "=");
                if (d != null) {
                    return new OperatorEqual(dictionary, Analyzes(d[0]), Analyzes(d[2]));
                }
            }

            //二項演算子
            foreach (string o in MathDictionary.Operators) {
                var d = DevideFormula(s, o);
                if (d != null) {
                    var result = new OperatorBinary(dictionary, d[1], Analyzes(d[0]), Analyzes(d[2]));
                    return result;
                }
            }

            //単項演算子
            {
                var m = MathDictionary.RegexOperator.Match(s);
                if (m.Success) {
                    if (m.Value == "&") {
                        return new OperatorReference(dictionary, Analyzes(s.Substring(m.Length)));
                    }
                    else {
                        return new OperatorUnary(dictionary, m.Value, Analyzes(s.Substring(m.Length)));
                    }
                }
            }

            //変数宣言の検索
            if (s.StartsWith("var ")) {
                /*
                var d = DevideFormula(s, "=");
                if (d != null) {
                    return new VariableDeclaration(dictionary, d[0].Substring(4).Trim(), Analyzes(d[2]));
                }
                else {
                    */
                return new VariableDeclaration(dictionary, s.Substring(4).Trim());
                //}
            }

            //定数
            if (MathDictionary.RegexName.IsMatch(s)) {
                var result = dictionary.GetConstants(s);
                if (result != null) {
                    return new Literal(dictionary, result);
                }
            }

            //変数、関数
            if (s.EndsWith(")")) {
                int index = s.IndexOf('(');
                var fn = s.Substring(0, index);
                var a = SubString(s.Substring(index), '(', ')');
                var args = new List<string>();

                if (a.Length > 0) {
                    //引数の取り出し
                    while (true) {
                        var d = DevideFormula(a, ",");
                        if (d == null) {
                            break;
                        }
                        args.Add(d[0]);
                        a = d[2];
                    }
                    args.Add(a);
                }

                return new FunctionCall(dictionary, fn, args.ToArray());
            }
            else if (s.EndsWith("]")) {
                int index = s.IndexOf('[');
                return new VariableArray(dictionary, s.Substring(0, index), SubString(s.Substring(index), '[', ']'));
            }
            else if (MathDictionary.RegexName.IsMatch(s)) {
                return new Variable(dictionary, s);
            }

            //定数値
            return new Literal(dictionary, s);
        }

        /// <summary>
        /// strから対応するstartc,endcを抜き取る
        /// </summary>
        /// <param name="str">startcから始まる文字列</param>
        /// <param name="startc"></param>
        /// <param name="endc"></param>
        /// <returns></returns>
        public static string SubString(string str, char startc, char endc) {
            if (str[0] != startc) {
                throw new ApplicationException("strがstartcから始まっていません。");
            }

            int count = 1;
            for (int i = 1; i < str.Length; i++) {
                if (str[i] == endc) {
                    count--;
                    if (count == 0) {
                        if (i == 1) return "";
                        else return str.Substring(1, i - 1);
                    }
                }
                else if (str[i] == startc) {
                    count++;
                }
            }

            throw new ApplicationException("startcとendcの数がつりあっていません。");
        }

        /// <summary>
        /// fを2項演算子oprで区切る。
        /// </summary>
        /// <param name="f"></param>
        /// <param name="opr"></param>
        /// <returns></returns>
        public static string[] DevideFormula(string f, string opr) {
            bool reverse = MathDictionary.ReverseOperators.Contains(opr);
            string[] result = null;

            int spCount = 0;//括弧カウント
            int lpCount = 0;//大括弧カウント
            bool quFlag = false;//ダブルクォートカウント

            if (!reverse) {
                for (int i = 0; i < f.Length; i++) {
                    //括弧は飛ばす
                    if (lpCount == 0 && !quFlag) {
                        if (f[i] == '(') {
                            spCount++;
                            continue;
                        }
                        else if (f[i] == ')') {
                            spCount--;
                            if (spCount < 0) throw new ApplicationException("対応のない\')\'を検出しました。");
                            continue;
                        }
                    }
                    if (spCount == 0 && !quFlag) {
                        if (f[i] == '[') {
                            lpCount++;
                            continue;
                        }
                        else if (f[i] == ']') {
                            lpCount--;
                            if (lpCount < 0) throw new ApplicationException("対応のない\']\'を検出しました。");
                            continue;
                        }
                    }
                    if (spCount == 0 && lpCount == 0) {
                        if (f[i] == '\"') {
                            quFlag = !quFlag;
                            continue;
                        }
                    }

                    //演算子の検索
                    if (spCount == 0 && lpCount == 0 && !quFlag) {
                        var tf = f.Substring(i);

                        var rgx = MathDictionary.RegexOperator.Match(tf);
                        if (rgx.Success) {
                            if (i == 0) {
                                i += rgx.Length - 1;
                                continue;
                            }
                            else if (rgx.Value == opr) {
                                result = new string[3];
                                result[0] = f.Substring(0, i);
                                result[1] = opr;
                                result[2] = f.Substring(i + opr.Length);
                                break;
                            }
                            else {
                                //演算子が登録されている場合、飛ばす
                                var os = MathDictionary.Operators.Where((string x) => { return x.Length > opr.Length; });
                                if (os.Contains(rgx.Value)) {
                                    i += rgx.Length - 1;
                                    continue;
                                }
                                //演算子がoprより短い場合、飛ばす
                                if (rgx.Length <= opr.Length) {
                                    i += rgx.Length - 1;
                                    continue;
                                }
                                //始めの部分がoprなら大勝利
                                if (rgx.Value.Substring(0, opr.Length) == opr) {
                                    result = new string[3];
                                    result[0] = f.Substring(0, i);
                                    result[1] = opr;
                                    result[2] = f.Substring(i + opr.Length);
                                    break;
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            else {
                for (int i = f.Length - 1; i >= 0; i--) {
                    //括弧は飛ばす
                    if (f[i] == ')') {
                        spCount++;
                        continue;
                    }
                    else if (f[i] == '(') {
                        spCount--;
                        if (spCount < 0) throw new ApplicationException("対応のない\')\'を検出しました。");
                        continue;
                    }
                    else if (f[i] == ']') {
                        lpCount++;
                        continue;
                    }
                    else if (f[i] == '[') {
                        lpCount--;
                        if (lpCount < 0) throw new ApplicationException("対応のない\']\'を検出しました。");
                        continue;
                    }
                    else if (f[i] == '\"') {
                        quFlag = !quFlag;
                        continue;
                    }

                    //演算子の検索
                    if (spCount == 0 && lpCount == 0 && !quFlag && i > 0) {
                        var tf = f.Substring(i);

                        var rgx = MathDictionary.RegexOperator.Match(tf);
                        if (rgx.Success) {

                            //その演算子を全て取り出す
                            string preOpr = "";
                            do {
                                if (i == 0) break;
                                i--;
                                tf = f.Substring(i);
                                preOpr = rgx.Value;
                                rgx = MathDictionary.RegexOperator.Match(tf);
                            } while (rgx.Success);
                            i++;

                            //
                            if (preOpr == opr) {
                                result = new string[3];
                                result[0] = f.Substring(0, i);
                                result[1] = opr;
                                result[2] = f.Substring(i + opr.Length);
                                break;
                            }

                            //preOprが登録されている場合、飛ばす
                            var os = MathDictionary.Operators.Where((string x) => { return x.Length > opr.Length; });
                            if (os.Contains(preOpr)) {
                                i -= preOpr.Length - 1;
                                continue;
                            }

                            //preOprがoprよりも短い場合、飛ばす
                            if (preOpr.Length <= opr.Length) {
                                i += preOpr.Length - 1;
                                continue;
                            }

                            //preOprの末尾がなんかアレなら大勝利
                            if (preOpr.Substring(opr.Length - 1) == opr) {
                                result = new string[3];
                                result[0] = f.Substring(0, i);
                                result[1] = opr;
                                result[2] = f.Substring(i + opr.Length);
                                break;
                            }
                        }
                        /*
                        if (rgx.Success && i > 0) {
                            if (rgx.Value == opr) {
                                result = new string[3];
                                result[0] = f.Substring(0, i);
                                result[1] = opr;
                                result[2] = f.Substring(i + opr.Length);
                                break;
                            }
                            else {
                                //i += rgx.Value.Length - 1;
                                continue;
                            }
                        }
                        */
                    }
                }
            }

            return result;
        }

        public static string[] DevideFormula2(string f, char opr) {
            string[] result = null;

            int spCount = 0;//括弧カウント
            int lpCount = 0;//大括弧カウント
            bool quFlag = false;//ダブルクォートカウント

            for (int i = 0; i < f.Length; i++) {
                //括弧は飛ばす
                if (lpCount == 0 && !quFlag) {
                    if (f[i] == '(') {
                        spCount++;
                        continue;
                    }
                    else if (f[i] == ')') {
                        spCount--;
                        if (spCount < 0) throw new ApplicationException("対応のない\')\'を検出しました。");
                        continue;
                    }
                }
                if (spCount == 0 && !quFlag) {
                    if (f[i] == '[') {
                        lpCount++;
                        continue;
                    }
                    else if (f[i] == ']') {
                        lpCount--;
                        if (lpCount < 0) throw new ApplicationException("対応のない\']\'を検出しました。");
                        continue;
                    }
                }
                if (spCount == 0 && lpCount == 0) {
                    if (f[i] == '\"') {
                        quFlag = !quFlag;
                        continue;
                    }
                }

                //演算子の検索
                if (spCount == 0 && lpCount == 0 && !quFlag) {
                    if (f[i] == opr) {
                        result = new string[2];
                        result[0] = f.Substring(0, i);
                        result[1] = f.Substring(i + 1);
                        return result;
                    }
                }
            }

            return result;
        }
    }
}
