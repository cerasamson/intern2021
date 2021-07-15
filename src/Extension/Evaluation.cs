using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extension
{
    public enum Outcome
    {
        Failure,
        PartialSuccess,
        Success
    };
    [TypeFormatterSource(typeof(EvaluationFormatterSource))]
    // make immutable
    // recursively defined?
    public class Evaluation
    {
        private readonly string label;

        private readonly Dictionary<string, Evaluation>ruleEvaluations = new();

        public IEnumerable<Evaluation> Rules => ruleEvaluations.Values;

        public Outcome Outcome { get; private set; }

        public string Reason { get; private set; }

        public object Hint { get; private set; }

        public bool Passed { get { return Outcome == Outcome.Success; } }

        public Evaluation(string label = null)
        {
            this.label = label;
        }

        public void SetOutcome(Outcome outcome, string reason = null, object hint = null)
        {
            Hint = hint;
            Outcome = outcome;
            if (string.IsNullOrWhiteSpace(reason))
            {
                Reason = outcome switch
                {
                    Outcome.Success => "All tests passed.",
                    Outcome.PartialSuccess => "Some tests passed.",
                    Outcome.Failure => "Incorrect solution.",
                    _ => throw new NotImplementedException()
                };   
            }
            else
            {
                Reason = reason;
            }

        }

        public PocketView FormatAsHtml()
        {
            var outcomeDivStyle = Outcome switch
            {
                Outcome.Success => "background:green",
                Outcome.PartialSuccess => "background:#eb6f00",
                Outcome.Failure => "background:red",
                _ => throw new NotImplementedException()
            };

            var outcomeMessage = Outcome switch
            {
                Outcome.Success => "Success",
                Outcome.PartialSuccess => "Partial Success",
                Outcome.Failure => "Failure",
                _ => throw new NotImplementedException()
            };

            var outcomeRuleStyle = Outcome switch
            {
                Outcome.Success => "color:green",
                Outcome.PartialSuccess => "color:orange",
                Outcome.Failure => "color:red",
                _ => throw new NotImplementedException()
            };


            var elements = new List<PocketView>();
            var succeededRules = ruleEvaluations.Values.Count(r => r.Outcome == Outcome.Success);
            var totalRules = ruleEvaluations.Count;
            var countReport = totalRules > 0 ? $" ({succeededRules}/{totalRules})" : string.Empty;
            outcomeMessage = $"{outcomeMessage}{countReport}: ";
            string newline = "\n";

            if (string.IsNullOrWhiteSpace(label))
            {
                PocketView summary = div[@class: "summary", style: outcomeDivStyle](b(outcomeMessage), (Reason), (newline));

                elements.Add(summary);

            }
            else
            {
                PocketView summary = div[@class: "summary", style: outcomeDivStyle](b($"[{this.label}] "), b(outcomeMessage), (Reason), (newline));

                elements.Add(summary);
            }
            
            if (Hint is not null)
            {
                var hintElement = div[@class: "hint", style: "border-left-style:solid"](Hint.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent());
                elements.Add(hintElement);
            }
            foreach (var rule in ruleEvaluations.Values.OrderBy(r=>r.Outcome).ThenBy(r=>r.label))
            {
                elements.Add(div[@class: "rule", style: outcomeRuleStyle](rule.ToDisplayString(HtmlFormatter.MimeType).ToHtmlContent()));
            }

            PocketView report = div(elements);

            return report;
        }

        public void SetRuleOutcome(string name, Outcome outcome, string reason = null, object hint = null)
        {
            var ruleEvaluation = new Evaluation(name);
            ruleEvaluation.SetOutcome(outcome, reason, hint);
            ruleEvaluations[name] = ruleEvaluation;
        }

    }
}