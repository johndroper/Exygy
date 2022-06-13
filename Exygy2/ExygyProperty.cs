using System;
using System.Collections.Generic;
using System.Linq;

namespace Exygy2
{
    public class ExygyPage
    {
        public List<ExygyProperty> properties { get; set; }

        public List<string> validAmenities { get; set; }

        public int recordCount { get; set; }
        public int page { get; set; }
        public int recordsPerPage { get; set; }

        public IEnumerable<int> validPages
        {
            get
            {
                return Enumerable.Range(1, (int)Math.Ceiling((double)recordCount / (double)recordsPerPage));
            }
        }

        public ExygyPage()
        {
            properties = new List<ExygyProperty>();

            validAmenities = new List<string>();
        }
    }


    public class ExygyProperty
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string pictureUrl { get; set; }
        public List<ExygyPropertyUnitType> unitsTypes { get; set; } 
        public double avgUnitSqft { get; set; }
        public int minMinOccupancy { get; set; }
        public int maxMaxOccupancy { get; set; }

        public ExygyProperty()
        {
            unitsTypes = new List<ExygyPropertyUnitType>();
        }
    }

    public class ExygyPropertyUnitType
    {
        public string unitType { get; set; }
        public int avgUnitSqft { get; set; }
        public int minMinOccupancy { get; set; }
        public int maxMaxOccupancy { get; set; }
    }
}
