using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Naru.Sentence;
using System.Threading;

namespace Naru.Analyzer {
    public class ScriptAnalyzer {
        public static bool Terminate;
        private static int ActiveNum;
        private static bool IsStopped;

        private MathDictionary dictionary;
        private string baseScript;

        private SentenceBase[] sentences;
        private int seek;
        private bool end;

        private int cycle;

        public ScriptAnalyzer(string s, MathDictionary dic) {
            baseScript = (string)s.Clone();
            dictionary = dic;

            //解析開始
            s = s.Replace('\t', ' ');

            //1.改行,タブは問答無用で削除
            //2.ダブルクォート内の内容は削除しない
            //3.var,elseとマッチする分は削除しない
            //4.コメント(//,/*-*/)
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++) {
                //4.
                if (i < s.Length - 1) {
                    if (s[i] == '/' && s[i + 1] == '/') {
                        while (i<s.Length) {
                            if (s[i] == '\n' || s[i] == '\r') {
                                break;
                            }
                            i++;
                        }
                        continue;
                    }
                    else if (s[i] == '/' && s[i + 1] == '*') {
                        while (i < s.Length-1) {
                            if (s[i] == '*' && s[i + 1] == '/') {
                                break;
                            }
                            i++;
                        }
                        i++;
                        continue;
                    }
                }

                //1.
                if (s[i] == '\n' || s[i]=='\r') {
                    continue;
                }

                //2.
                if (s[i] == '\"') {
                    sb.Append(s[i]);
                    for (i++; i < s.Length && s[i] != '\"'; i++) {
                        if (s[i] == '\t') sb.Append("\\t");
                        else sb.Append(s[i]);
                    }
                    sb.Append(s[i]);
                    continue;
                }

                //3.
                bool flag = false;
                do {
                    flag = false;
                    var sss = s.Substring(i);
                    foreach (string aw in MathDictionary.AnalyzeWords) {
                        if (sss.StartsWith(aw)) {
                            sb.Append(aw);
                            i += aw.Length;
                            flag = true;
                            break;
                        }
                    }
                } while (flag);


                if (s[i] != ' ') {
                    sb.Append(s[i]);
                }
            }

            List<SentenceBase> ls = new List<SentenceBase>();
            try {
                //';',"if","while",'{','}'で区切る
                Analyzes(ls, sb.ToString());

                sentences = ls.ToArray();
            }
            catch (Exception e) {
                int c = 0;
                foreach (var it in ls) {
                    c += it.GetCount();
                }
                throw new ApplicationException(c + ":" + e.Message, e);
            }
        }
        public ScriptAnalyzer(string s, MathDictionary dic, int chkCycle) : this(s, dic) {
            cycle = chkCycle;
            Terminate = false;
        }

        private void Analyzes(List<SentenceBase> ls, string s) {
            if (s == null) {
                return;
            }

            try {
                ls.Add(AnalyzesOnce(ls, ref s));
            }
            catch (Exception e) {
                throw new ApplicationException("["+s+"]"+e.Message, e);
            }
            Analyzes(ls, s);
        }
        private SentenceBase AnalyzesOnce(List<SentenceBase> ls, ref string s) {
            SentenceBase stc = null;

            string ts = "";
            if (s[0] == '{') {
                ts = FormulaAnalyzer.SubString(s, '{', '}');
                stc = new SentenceBrace(this, ts);

                if (s.Length == 2 + ts.Length) s = null;
                else s = s.Substring(2 + ts.Length);
            }
            else if (s.StartsWith("include\"")) {
                //ファイル名取り出し
                var fn = FormulaAnalyzer.SubString(s.Substring(7), '\"', '\"');

                //include後のセミコロン
                if (s.Length < 9 + fn.Length || s[9 + fn.Length] != ';') {
                    throw new ApplicationException("; が見つかりません。");
                }

                //読み込み
                Dictionary.Include(fn);

                if (s.Length == 10 + fn.Length) s = null;
                else s = s.Substring(10 + fn.Length);

                stc = new SentenceBase(this, "0");
            }
            else if (s.StartsWith("if(")) {
                ts = FormulaAnalyzer.SubString(s.Substring(2), '(', ')');
                //len = 4 + ts.Length;

                if (s.Length == 4 + ts.Length) throw new ApplicationException("if 構文エラー");
                else s = s.Substring(4 + ts.Length);

                //new stc
                stc = new SentenceIf(this, ts, AnalyzesOnce(ls, ref s));
            }
            else if (s.StartsWith("else") && (s[4] == ' ' || s[4] == '{' || s[4] == '(')) {
                var pre = ls[ls.Count - 1];
                SentenceIf sif = null;

                if (pre is SentenceIf) {
                    sif = (SentenceIf)pre;
                }
                else if (pre is SentenceElse) {
                    int i=ls.Count-2;
                    while (i > 0 && ls[i] is SentenceElse) {
                        i--;
                    }
                    sif = ls[i] as SentenceIf;
                }
                if (sif == null) {
                    throw new ApplicationException("else 構文エラー");
                }
                while (sif.hasElseIf()) {
                    sif = sif.getElse() as SentenceIf;
                }

                if (s[4] == ' ') {
                    if (s.Length == 5) throw new ApplicationException("else 構文エラー");
                    else s = s.Substring(5);

                    sif.SetElse(AnalyzesOnce(ls, ref s));
                    stc = new SentenceElse(this, "0");
                }
                else {
                    s = s.Substring(4);

                    sif.SetElse(AnalyzesOnce(ls, ref s));
                    stc = new SentenceElse(this, "0");
                }
            }
            else if (s.StartsWith("while(")) {
                ts = FormulaAnalyzer.SubString(s.Substring(5), '(', ')');

                if (s.Length == 7 + ts.Length) throw new ApplicationException("while 構文エラー");
                else s = s.Substring(7 + ts.Length);

                stc = new SentenceWhile(this, ts, AnalyzesOnce(ls, ref s));
            }
            else if (s.StartsWith("for(")) {
                ts = FormulaAnalyzer.SubString(s.Substring(3), '(', ')');

                if (s.Length == 5 + ts.Length) throw new ApplicationException("while 構文エラー");
                else s = s.Substring(5 + ts.Length);

                stc = new SentenceFor(this, ts, AnalyzesOnce(ls, ref s));
            }
            else if (s.StartsWith("function ")) {
                int psi, pei;

                psi = s.IndexOf('(');
                pei = s.IndexOf(')');
                if (psi == -1 || pei == -1 || psi > pei) {
                    throw new ApplicationException("function 構文エラー");
                }

                //関数名の取り出し
                if (psi == 9) {
                    throw new ApplicationException("function 構文エラー");
                }
                string funcName = s.Substring(9, psi - 9);

                //引数の取り出し
                var arg = s.Substring(psi + 1, pei - psi - 1);

                //内容の取り出し
                if (s[pei + 1] != '{') {
                    throw new ApplicationException("function 構文エラー");
                }
                var str = FormulaAnalyzer.SubString(s.Substring(pei + 1), '{', '}');

                var func = new Function.FunctionDeclared(Dictionary, funcName, arg.Length > 0 ? arg.Split(',') : new string[0] { }, str);
                Dictionary.AddFunction(func);

                int len = 9 + funcName.Length + 2 + arg.Length + 2 + str.Length;

                if (s.Length == len) s = null;
                else s = s.Substring(len);

                stc = new SentenceBase(this, "0");
            }
            else if (s.StartsWith("return ")) {
                int index = s.IndexOf(';');
                if (index == -1) {
                    throw new ApplicationException("; が見つかりません。");
                }
                ts = s.Substring(7, index - 7);
                stc = new SentenceReturn(this, ts);

                if (s.Length == ts.Length + 8) s = null;
                else s = s.Substring(ts.Length + 8);
            }
            else if (s.StartsWith("return;")) {
                stc = new SentenceReturn(this, "0");

                if (s.Length == 8) s = null;
                else s = s.Substring(8);
            }
            else {
                int index = s.IndexOf(';');
                if (index == -1) {
                    throw new ApplicationException("; が見つかりません。");
                }
                //len = index + 1;
                ts = s.Substring(0, index);
                stc = new SentenceBase(this, ts);

                if (s.Length == ts.Length + 1) s = null;
                else s = s.Substring(ts.Length + 1);
            }
            return stc;
        }

        public MathDictionary Dictionary {
            get {
                return dictionary;
            }
        }

        public void Executes() {
            end = false;
            ActiveNum++;
            try {
                for (seek = 0; seek < sentences.Length && !end && !Terminate; seek++) {
                    while (IsStopped) ;
                    sentences[seek].Executes();
                }
                ActiveNum--;
            }
            catch (Exception e) {
                end = true;
                ActiveNum--;
                int line = LineCount(seek);
                throw new ApplicationException(line + "行目でエラーが発生しました。:" + sentences[seek].ToString() + " - " + e.Message, e);
            }
        }
        public int LineCountAll() {
            int i = 0;
            foreach (var s in sentences) {
                i += s.GetCount();
            }
            return i;
        }
        public int LineCount(int length) {
            int c = 0;
            for (int i = 0; i < length && i < sentences.Length; i++) {
                c += sentences[i].GetCount();
            }
            return c;
        }

        public void Exit() {
            end = true;
        }

        public T FindReverse<T>() where T : SentenceBase {
            int ifCount = 0;
            int whileCount = 0;
            for (int i = sentences.Length - 1; i >= 0; i--) {
                Type t = sentences[i].GetType();

                if (t == typeof(SentenceElse)) {
                    ifCount++;
                    continue;
                }

                if (ifCount == 0 && whileCount == 0 && t == typeof(T)) {
                    return (T)sentences[i];
                }
            }
            throw new ApplicationException(typeof(T).Name + " が見つかりません。");
        }

        /// <summary>
        /// 現在の行数からqだけ移動する
        /// </summary>
        /// <param name="q"></param>
        public void Seek(int q) {

        }

        public void MTObserve() {
            if (cycle <= 0) {
                return;
            }

            while (true) {
                Thread.Sleep(1000 * cycle);
                if (end) {
                    return;
                }
                else {
                    IsStopped = true;
                    var r = MessageBox.Show("実行に時間が掛かりすぎています。\n強制停止しますか？", "警告", MessageBoxButtons.YesNo);
                    IsStopped = false;
                    if (r == DialogResult.Yes) {
                        Console.WriteLine("[Naru]強制停止しました。");
                        Terminate = true;
                        while (ScriptAnalyzer.ActiveNum > 0) ;
                        return;
                    }
                }
            }
        }
    }
}
