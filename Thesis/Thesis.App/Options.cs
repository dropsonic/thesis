using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.App
{
    enum Filter
    {
        Gaussian, Difference, Threshold
    }

    enum Normalization
    {
        Minimax, Standard
    }

    enum Metric
    {
        Euclid, SqrEuclid
    }

    enum ClusterDistanceType
    {
        KMeans, Nearest
    }

    class Options
    {
        [ValueOption(0)]
        public string FieldsDescription { get; set; }

        [OptionArray('a', "anregimes", HelpText = "Files with anomalie regimes samples.")]
        public string[] Samples { get; set; }

        [OptionArray('r', "nomregimes", HelpText="Files with nominal regimes samples (will be cleaned from anomalies).", Required = true)]
        public string[] NominalSamples { get; set; }

        [Option('f', "filter", DefaultValue = Filter.Gaussian,
            HelpText = "Anomalies filter type and value. Appropriate values: gaussian, difference (maxdiff-value), threshold (threshold-value).")]
        public Filter Filter { get; set; }

        [Option('n', "normalization", DefaultValue = Normalization.Minimax,
            HelpText = "Normalization type. Appropriate values: minimax, standard.")]
        public Normalization Normalization { get; set; }

        [Option('m', "metric", DefaultValue = Metric.Euclid,
            HelpText = "Distance metric. Appropriate values: euclid, sqreuclid.")]
        public Metric Metric { get; set; }

        [Option('d', "cdist", DefaultValue = ClusterDistanceType.Nearest,
            HelpText = "Cluster distance function. Appropriate values: kmeans, nearest.")]
        public ClusterDistanceType ClusterDistanceType { get; set; }

        [Option('v', "novalue", DefaultValue = "?",
            HelpText = "Non-existing values replacement string.")]
        public string NoValueReplacement { get; set; }

        [Option('o', "output")]
        public string OutputFile { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string> Args { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Thesis: Intergrated System Health Management based on Data Mining techniques."),
                Copyright = new CopyrightInfo("Vladimir Panchenko, 03-617, Moscow Aviation Institute", 2013),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: thesis.app fields.txt -r nominal1.txt nominal2.txt [-a regime1.txt regimeN.txt] [-f threshold 0.5] [-d kmeans] [-m sqreuclid] [-n standard] [-v N/A]");
            help.AddPostOptionsLine("Normalization stats are calculated based on first nominal regime sample.\n");
            help.AddOptions(this);
            return help;
        }
    }

    class FilterOptions
    {
        //[Option('v', "fvalue", HelpText = "Maximum difference for difference filter | Threshold value for threshold filter")]
        [ValueOption(0)]
        public double? Value { get; set; }
    }
}
