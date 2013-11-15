using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Thesis.DPrep
{
    struct Field
    {
        public enum FieldType
        {
            Discrete = 0,
            /// <summary>
            /// Discrete Data Driven
            /// </summary>
            DiscreteCompiled = 1,
            Continuous = 2,
            IgnoreFeature = 3
        }

        public string Name { get; set; }

        public FieldType Type { get; set; }

        /// <summary>
        /// List of values for discrete field.
        /// </summary>
        public IList<string> Values { get; set; }

        public Field(IList<string> s)
            : this()
        {
            Contract.Requires<ArgumentNullException>(s != null);
            Contract.Requires<ArgumentException>(s.Count > 0);

            Values = new List<string>();
            Name = s[0];

            if (s.Count == 1)
                Type = FieldType.IgnoreFeature;
            else
            {
                switch (s[1])
                {
                    case "ignore":
                        Type = FieldType.IgnoreFeature;
                        break;
                    case "continuous":
                        Type = FieldType.Continuous;
                        break;
                    case "discrete":
                        Type = FieldType.DiscreteCompiled;
                        break;
                    default:
                        //Discrete type of field: adding all of it's values
                        Type = FieldType.Discrete;
                        Values = new List<string>(s.Count-1);
                        for (int i = 1; i < s.Count; i++)
                            Values.Add(s[i]);
                        break;
                }
            }
        }
    }
}
