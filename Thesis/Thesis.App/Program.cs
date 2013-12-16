using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.DataCleansing;
using Thesis.DDMS;
using Thesis.Orca;

namespace Thesis.App
{
    class Program
    {
        static Options _options;
        static string _fieldsFile;
        static IAnomaliesFilter _filter;
        static IRecordParser<string> _parser;
        static IScaling _scaling;
        static SystemModel _model;
        static DistanceMetric _metric;
        static ClusterDistanceMetric _clusterDistMetric;
        static IList<Field> _fields;

        static bool _writeToFile;

        static string GetBinName(string name)
        {
            return String.Concat(name, ".bin");
        }

        static void ShowUsage()
        {
            Console.WriteLine(new Options().GetUsage());
            Console.WriteLine();
            Environment.Exit(-1);
        }

        static void GetScaling()
        {
            var readers = _options.NominalSamples.Concat(_options.Samples).Select(s => new PlainTextReader(s, _fieldsFile, _parser));
            var allReader = new MultipleDataReader(readers);
            switch (_options.Normalization)
            {
                case Normalization.Standard:
                    _scaling = new StandardScaling(allReader);
                    break;
                default:
                    _scaling = new MinmaxScaling(allReader);
                    break;
            }
            foreach (var reader in readers)
                reader.Dispose();
        }

        static void AddNominalSample(string filename)
        {
            if (!File.Exists(filename))
                throw new ApplicationException("File not found.");

            using (IDataReader reader = new PlainTextReader(filename, _fieldsFile, _parser))    // read input data   
            using (IDataReader binReader = new BinaryDataReader(reader, GetBinName(filename)))  // convert input data to binary format
            using (IDataReader scaleReader = new ScaleDataReader(binReader, _scaling))       // scale input data
            {
                if (_fields == null)
                    _fields = reader.Fields;

                string shuffleFile = String.Concat(filename, ".shuffle");
                scaleReader.Shuffle(shuffleFile);
                IEnumerable<Outlier> outliers = Enumerable.Empty<Outlier>();

                string regimeName = Path.GetFileNameWithoutExtension(filename);

                using (IDataReader cases = new BinaryDataReader(shuffleFile))
                using (IDataReader references = new BinaryDataReader(shuffleFile))
                {
                    var orca = new OrcaAD(DistanceMetrics.Euclid, neighborsCount: 10);
                    outliers = orca.Run(cases, references, true);
                }

                File.Delete(shuffleFile);

                var anomalies = _filter.Filter(outliers);

                Console.WriteLine("\n%%%%%% {0} %%%%%%", regimeName);
                Console.WriteLine("Anomalies:");
                foreach (var anomaly in anomalies)
                    Console.WriteLine("  Id = {0}, Score = {1}", anomaly.Id, anomaly.Score);

                Console.WriteLine("%%%%%%%%%%%%%%%\n");

                using (IDataReader cleanReader = new CleanDataReader(scaleReader, anomalies)) // clean input data from anomalies
                {
                    _model.AddRegime(regimeName, cleanReader);
                }
            }
        }

        static void AddSample(string filename)
        {
            if (!File.Exists(filename))
                throw new ApplicationException("File not found.");

            string regimeName = Path.GetFileNameWithoutExtension(filename);

            using (IDataReader tempReader = new PlainTextReader(filename, _fieldsFile, _parser))
            using (IDataReader tempBinReader = new BinaryDataReader(tempReader, GetBinName(filename)))
            {
                using (IDataReader tempScaleReader = new ScaleDataReader(tempBinReader, _scaling))
                {
                    _model.AddRegime(regimeName, tempScaleReader);
                }
            }
        }

        static void Main(string[] args)
        {
            Contract.ContractFailed += (s, e) =>
            {
                Console.WriteLine("Something went wrong. Please contact the developer.");
            };

            var argsParser = CommandLine.Parser.Default;
            _options = new Options();
            if (argsParser.ParseArgumentsStrict(args, _options))
            {
                _writeToFile = !String.IsNullOrEmpty(_options.OutputFile);

                // Filter type
                if (_options.Filter == Filter.Gaussian)
                    _filter = new GaussianFilter();
                else
                {
                    FilterOptions filterOpts = new FilterOptions();
                    if (argsParser.ParseArgumentsStrict(_options.Args.ToArray(), filterOpts))
                    {
                        if (filterOpts.Value == null)
                            ShowUsage();
                        switch (_options.Filter)
                        {
                            case Filter.Difference:
                                _filter = new DifferenceFilter((double)filterOpts.Value);
                                break;
                            case Filter.Threshold:
                                _filter = new ThresholdFilter((double)filterOpts.Value);
                                break;
                            default:
                                _filter = new GaussianFilter();
                                break;
                        }
                    }
                }

                try
                {
                    _parser = new PlainTextParser(_options.NoValueReplacement);
                    _fieldsFile = _options.FieldsDescription;

                    GetScaling();

                    switch (_options.Metric)
                    {
                        case Metric.SqrEuclid:
                            _metric = DistanceMetrics.SqrEuсlid;
                            break;
                        default:
                            _metric = DistanceMetrics.Euclid;
                            break;
                    }

                    switch(_options.ClusterDistanceType)
                    {
                        case ClusterDistanceType.KMeans:
                            _clusterDistMetric = ClusterDistances.CenterDistance;
                            break;
                        default:
                            _clusterDistMetric = ClusterDistances.NearestBoundDistance;
                            break;
                    }

                    Console.WriteLine("Enter epsilon:");
                    double eps;
                    while (!double.TryParse(Console.ReadLine(), out eps))
                        Console.WriteLine("Wrong format. Please enter epsilon again.");

                    _model = new SystemModel(eps);

                    foreach (var nominalSample in _options.NominalSamples)
                        AddNominalSample(nominalSample);

                    foreach (var sample in _options.Samples)
                        AddSample(sample);


                    Console.WriteLine("Knowledge base has been created. {0} regime(s) total:", _model.Regimes.Count);
                    foreach (var regime in _model.Regimes)
                    {
                        Console.WriteLine("\n***** {0} *****", regime.Name);
                        Console.WriteLine("{0} cluster(s) in regime.", regime.Clusters.Count);
                        int i = 0;
                        foreach (var cluster in regime.Clusters)
                        {
                            Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + 10);
                            Console.WriteLine("  --------------------------");
                            Console.WriteLine("  Cluster #{0}:", ++i);
                            Console.WriteLine("  Lower bound: {0}", String.Join(" | ", _scaling.Unscale(cluster.LowerBound)));
                            Console.WriteLine("  Upper bound: {0}", String.Join(" | ", _scaling.Unscale(cluster.UpperBound)));
                            Console.WriteLine("  Appropriate discrete values: {0}", String.Join(" | ", cluster.DiscreteValues.Select(f => String.Join("; ", f))));
                        }
                        Console.WriteLine("  --------------------------");
                        Console.WriteLine("******************", regime.Name);
                    }

                    Console.WriteLine("\nEnter record, or press enter to quit.");

                    string line = String.Empty;
                    do
                    {
                        line = Console.ReadLine();
                        if (String.IsNullOrEmpty(line)) break;

                        var record = _parser.TryParse(line, _fields);
                        if (record == null)
                        {
                            Console.WriteLine("Wrong record format. Please enter record again.");
                            continue;
                        }

                        _scaling.Scale(record);
                        double distance;
                        Regime closest;
                        Regime currentRegime = _model.DetectRegime(record, out distance, out closest);
                        if (currentRegime == null)
                            Console.WriteLine("Anomaly behavior detected (closest regime: {0}, distance: {1:0.00000}).\n",
                                closest.Name, distance);
                        else
                            Console.WriteLine("Current regime: {0}\n", currentRegime.Name);
                    } while (true);
                }
                catch (DataFormatException dfex)
                {
                    Console.WriteLine("Wrong data format. {0}", dfex.Message);
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0} Please contact the developer.", ex.Message);
                    Console.ReadLine();
                }
            }
        }
    }
}
