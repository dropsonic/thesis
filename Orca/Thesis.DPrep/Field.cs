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
            DiscreteDataDriven = 1,
            Continuous = 2,
            IgnoreFeature = 3
        }

        public string Name { get; set; }

        public FieldType Type { get; set; }

        /// <summary>
        /// List of values for discrete field.
        /// </summary>
        public IList<string> Values { get; set; }

        public float Weight { get; set; }

        private bool HasWeight 
        { 
            get { return !float.IsNaN(Weight); } 
        }

        public Field(IList<string> s, float realWeight, float discreteWeight)
            : this()
        {
            Contract.Requires<ArgumentNullException>(s != null);
            Contract.Requires<ArgumentException>(s.Count > 0);

            Values = new List<string>();

            Weight = float.NaN; // no weight
            int i = 0; // start token
            float weight;
            if (float.TryParse(s[0], out weight)) // if weight is defined
            {
                Weight = weight;
                i++;
            }

            Name = s[i++];

            if (s.Count == i)
                Type = FieldType.IgnoreFeature;
            else
            {
                switch (s[i++])
                {
                    case "ignore":
                        Type = FieldType.IgnoreFeature;
                        break;
                    case "continuous":
                        Type = FieldType.Continuous;
                        if (!HasWeight)
                            Weight = realWeight;
                        break;
                    case "discrete":
                        Type = FieldType.DiscreteDataDriven;
                        if (!HasWeight)
                            Weight = discreteWeight;
                        break;
                    default:
                        //Discrete type of field: adding all of it's values
                        Type = FieldType.Discrete;
                        Values = new List<string>(s.Count-1);
                        for (int j = 1; j < s.Count; j++)
                            Values.Add(s[j]);
                        if (!HasWeight)
                            Weight = discreteWeight;
                        break;
                }
            }
        }
    }
}
