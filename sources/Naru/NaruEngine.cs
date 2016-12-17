using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Naru.Analyzer;
using Naru.Value;
using System.Threading;

namespace Naru {
  //######################################################################################
  // Naru Engine
  // Analyzing and Execution Naru
  //
  // @Author : Farore
  // @Date   : 2014
  // @Edit   : Farore, 2016/12/17
  //
  //######################################################################################
  public class NaruEngine {

    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // Property
    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    public MathDictionary Dictionary {
        get;
        private set;
    }
    public static bool HasError {
        get;
        private set;
    }
    public static string LastErrorMessage {
        get;
        private set;
    }

    public static string Information {
        get {
            var asm = Assembly.GetExecutingAssembly();
            var cpr = (AssemblyCopyrightAttribute)AssemblyCopyrightAttribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));

            return "Naru Interpreter 1.0.0 \r\nImplements Naru 2.0.0\r\n(c) Farore 2014";
        }
    }

    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // Constructor
    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    public NaruEngine() {
        Dictionary = new MathDictionary();
    }
    public NaruEngine(MathDictionary dic) {
        Dictionary = dic;
    }

    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // Public Method
    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // Evaluate a Sentence.
    // @Param formula : Naru Sentence
    // @Return        : Result
    public ValueBase Evaluates(string formula) {
        var a = new FormulaAnalyzer(formula, Dictionary);
        return a.Evaluates();
    }

    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // Public Static Method
    //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // Execute Script 
    // @Param script  : Naru Script
    // @Param ostream : Output Stream
    public static void ExecutesScript(string script, System.IO.TextWriter ostream = Console.Out) {
        Console.SetOut(ostream);

        HasError = false;
        try {
            var sa = new ScriptAnalyzer(script, new MathDictionary());
            sa.Executes();
        }
        catch (Exception e) {
            Console.WriteLine(e);
            LastErrorMessage = e.Message;
            HasError         = true;
        }

        Console.SetOut(Console.Out);
    }

    // Execute Script with Process Observer
    // @Param script   : Naru Script
    // @Param chkCycle : Check Cycle(second) for Execution 
    // @Param ostream  : Output Stream
    public static void ExecutesScript(string script, int chkCycle, System.IO.TextWriter ostream = Console.Out) {
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
