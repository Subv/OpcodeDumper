using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// todo: enforce [base +- index * scale +- displacement] addressing scheme

namespace x86
{
    /// <summary>
    /// A stripped down and modified version of http://jarloo.com/code/math/c-formula-evaluator/
    /// </summary>
    public class Expression
    {
        private static readonly Regex hexidecimalRegex = new Regex(@"([a-f0-9A-F]*)h", RegexOptions.Compiled);
        private static readonly Regex binaryRegex = new Regex(@"([0-1]*)b", RegexOptions.Compiled);
        private static readonly Regex multiplyRegex = new Regex(@"(-?\d+\.?\d*)\*(-?\d+\.?\d*)", RegexOptions.Compiled);
        private static readonly Regex addRegex = new Regex(@"(-?\d+\.?\d*)\+(-?\d+\.?\d*)", RegexOptions.Compiled);
        private static readonly Regex subtractRegex = new Regex(@"(-?\d+\.?\d*)-(-?\d+\.?\d*)", RegexOptions.Compiled);

        /// <summary>
        /// Attempts to evaluate the specified instruction and return a numerical result.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static uint Evaluate(string expr)
        {
            // strip all whitespace from the expression
            expr = Regex.Replace(expr, @"\s+", string.Empty, RegexOptions.IgnoreCase);

            // convert binary constants to decimal
            expr = Do(binaryRegex, expr, (x) => (Convert.ToUInt32(x.Groups[1].Value, 2)).ToString());

            // convert hexidecimal constants to decimal
            expr = Do(hexidecimalRegex, expr, (x) => (Convert.ToUInt32(x.Groups[1].Value, 16)).ToString());

            // evaluate index * scale
            expr = Do(multiplyRegex, expr, (x) => (Convert.ToUInt32(x.Groups[1].Value) * Convert.ToUInt32(x.Groups[2].Value)).ToString());

            // evaluate any additions
            expr = Do(addRegex, expr, (x) => (Convert.ToUInt32(x.Groups[1].Value) + Convert.ToUInt32(x.Groups[2].Value)).ToString());

            // evaluate any subtractions
            expr = Do(subtractRegex, expr, (x) => (Convert.ToUInt32(x.Groups[1].Value) - Convert.ToUInt32(x.Groups[2].Value)).ToString());

            return Convert.ToUInt32(expr);
        }

        private static string Do(Regex regex, string formula, Func<Match, string> func)
        {
            MatchCollection collection = regex.Matches(formula);
            if (collection.Count == 0) return formula;
            for (int i = 0; i < collection.Count; i++) formula = formula.Replace(collection[i].Groups[0].Value, func(collection[i]));
            return Do(regex, formula, func);
        }
    }
}