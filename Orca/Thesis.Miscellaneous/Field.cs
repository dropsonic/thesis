using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Thesis
{
    public class Field
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

        private float _weight = float.NaN;
        public float Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        public bool HasWeight 
        { 
            get { return !float.IsNaN(Weight); } 
        }
    }
}
