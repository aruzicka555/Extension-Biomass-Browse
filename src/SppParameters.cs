//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.DeerBrowse
{
    /// <summary>
    /// Extra Spp Paramaters
    /// </summary>
    public interface ISppParameters
    {
        double BrowsePref{ get; set; }
        double GrowthReductThresh { get; set; }
        double GrowthReductMax { get; set; }
        double MortThresh { get; set; }
        double MortMax { get; set; }        
    }
}


namespace Landis.Extension.DeerBrowse
{
    public class SppParameters
        : ISppParameters
    {
        private double browsePref;
        private double growthReductThresh;
        private double growthReductMax;
        private double mortThresh;
        private double mortMax;


        //---------------------------------------------------------------------
        public double BrowsePref
        {
            get
            {
                return browsePref;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                browsePref = value;
            }
        }
        //---------------------------------------------------------------------
        public double GrowthReductThresh
        {
            get
            {
                return growthReductThresh;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                growthReductThresh = value;
            }
        }
        //---------------------------------------------------------------------
        public double GrowthReductMax
        {
            get
            {
                return growthReductMax;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                growthReductMax = value;
            }
        }
        //---------------------------------------------------------------------
        public double MortThresh
        {
            get
            {
                return mortThresh;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mortThresh = value;
            }
        }
        //---------------------------------------------------------------------
        public double MortMax
        {
            get
            {
                return mortMax;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or > 0.");
                if (value > 1)
                    throw new InputValueException(value.ToString(),
                        "Value must be = or < 1.");
                mortMax = value;
            }
        }
        //---------------------------------------------------------------------
        public SppParameters()
        {
            this.browsePref = 0.0;
            this.mortThresh = 0.0;
            this.mortMax = 0.0;
            this.growthReductThresh = 0.0;
            this.growthReductMax = 0.0;
        }
 
    }
}
