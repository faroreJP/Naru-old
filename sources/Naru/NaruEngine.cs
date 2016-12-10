using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Naru.Analyzer;
using Naru.Value;
using System.Threading;

namespace Naru {
    /// <summary>
    /// Naruによって記述された式・スクリプトを解析・実行するクラスです。
    /// </summary>
    public class NaruEngine {
        /// <summary>
        /// 使用しているMathDictionaryを取得します。
        /// </summary>
        public MathDictionary Dictionary {
            get;
            private set;
        }

        /// <summary>
        /// 最後に発生した例外メッセージを取得します。
        /// </summary>
        public static string LastErrorMessage {
            get;
            private set;
        }
        public static bool HasError {
            get;
            private set;
        }

        /// <summary>
        /// 既定のMathDictionaryを使用してNaruEngineを初期化します。
        /// </summary>
        public NaruEngine() {
            Dictionary = new MathDictionary();
        }

        /// <summary>
        /// 指定されたMathDictionaryを使用してNaruEngineを初期化します。
        /// </summary>
        /// <param name="dic">使用するMathDictionary</param>
        public NaruEngine(MathDictionary dic) {
            Dictionary = dic;
        }

        /// <summary>
        /// Naruのバージョン情報を取得します。
        /// </summary>
        public static string Information {
            get {
                var asm = Assembly.GetExecutingAssembly();
                var cpr = (AssemblyCopyrightAttribute)AssemblyCopyrightAttribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));

                return "Naru Interpreter 1.0.0 \r\nImplements Naru 2.0.0\r\n(c) Farore 2014";
            }
        }

        /// <summary>
        /// 1文を解析し評価します。
        /// </summary>
        /// <param name="formula">評価する文</param>
        /// <returns>評価結果</returns>
        public ValueBase Evaluates(string formula) {
            var a = new FormulaAnalyzer(formula, Dictionary);
            return a.Evaluates();
        }

        /// <summary>
        /// スクリプトを実行します。
        /// </summary>
        /// <param name="script">実行するスクリプト</param>
        public static void ExecutesScript(string script) {
            HasError = false;
            try {
                var sa = new ScriptAnalyzer(script, new MathDictionary());
                sa.Executes();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                LastErrorMessage = e.Message;
                HasError = true;
            }
        }

        /// <summary>
        /// 出力にostreamを設定してスクリプトを実行します。
        /// </summary>
        /// <param name="script">実行するスクリプト</param>
        /// <param name="ostream">出力先ストリーム</param>
        public static void ExecutesScript(string script, System.IO.TextWriter ostream) {
            Console.SetOut(ostream);
            ExecutesScript(script);
            Console.SetOut(Console.Out);
        }

        /// <summary>
        /// 出力にostreamを設定してスクリプトを実行します。
        /// 実行時間chkCycle[s]ごとに実行を中断するか確認します。
        /// </summary>
        /// <param name="script">実行するスクリプト</param>
        /// <param name="ostream">出力先ストリーム</param>
        /// <param name="chkCycle">中止するか確認する周期</param>
        public static void ExecutesScript(string script, System.IO.TextWriter ostream, int chkCycle) {
            
            Console.SetOut(ostream);
            try {
                var sa = new ScriptAnalyzer(script, new MathDictionary(), chkCycle);
                var t = new Thread(new ThreadStart(sa.MTObserve));
                t.Name = "Naru-Interpreter Observer";
                t.Start();
                sa.Executes();
                t.Abort();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            Console.SetOut(Console.Out);
        }

        

    }
}
