//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using System;
using System.Collections.Generic;
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;


namespace Landis.Extension.DeerBrowse
{
    public class Event
    {
        
        private int sitesDamaged;
        private int cohortsKilled;
        private int biomassRemoved;
        private int biomassKilled;
        private int population;
        private int[] cohortsKilledSpp;
        private int[] biomassRemovedSpp;

        private int[] zoneSitesDamaged;
        private int[] zoneCohortsKilled;
        private int[] zoneBiomassKilled;
        private int[] zoneBiomassRemoved;
        private int[] zonePopulation;
        private int[][] zoneCohortsKilledSpp;
        private int[][] zoneBiomassRemovedSpp;



        //private ActiveSite currentSite; // current site where cohorts are being damaged

        //---------------------------------------------------------------------

        static Event()
        {
           
        }

        //---------------------------------------------------------------------
        public int SitesDamaged
        {
            get {
                return sitesDamaged;
            }
        }
        //---------------------------------------------------------------------
        public int[] ZoneSitesDamaged
        {
            get
            {
                return zoneSitesDamaged;
            }
        }
        //---------------------------------------------------------------------
        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public int[] ZoneCohortsKilled
        {
            get
            {
                return zoneCohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public int BiomassKilled
        {
            get {
                return biomassKilled;
            }
        }
        //---------------------------------------------------------------------
        public int BiomassRemoved
        {
            get
            {
                return biomassRemoved;
            }
        }
        //---------------------------------------------------------------------
        public int[] ZoneBiomassKilled
        {
            get
            {
                return zoneBiomassKilled;
            }
        }
        //---------------------------------------------------------------------
        public int[] ZoneBiomassRemoved
        {
            get
            {
                return zoneBiomassRemoved;
            }
        }
        //---------------------------------------------------------------------
        public int Population
        {
            get
            {
                return population;
            }
        }
        //---------------------------------------------------------------------
        public int[] ZonePopulation
        {
            get
            {
                return zonePopulation;
            }
        }
        //---------------------------------------------------------------------
        public int[] CohortsKilledSpp
        {
            get
            {
                return cohortsKilledSpp;
            }
        }
        //---------------------------------------------------------------------
        public int[][] ZoneCohortsKilledSpp
        {
            get
            {
                return zoneCohortsKilledSpp;
            }
        }
        //---------------------------------------------------------------------
        public int[] BiomassRemovedSpp
        {
            get
            {
                return biomassRemovedSpp;
            }
        }
        //---------------------------------------------------------------------
        public int[][] ZoneBiomassRemovedSpp
        {
            get
            {
                return zoneBiomassRemovedSpp;
            }
        }
        //---------------------------------------------------------------------
        public static void Initialize()
        {
           
        }

        //---------------------------------------------------------------------

        public static Event Initiate(IInputParameters parameters)
        {
                Event browseEvent = new Event();

                browseEvent.CalculateForage(parameters);

                // Calculate population
                PopulationZones.CalculatePopulation(parameters);

                // Calculate HSI
                HabitatSuitability.CalculateHSI(parameters);

                // Calculate site-level population
                PopulationZones.CalculateLocalPopulation(parameters);
            
                //Calculate Browse to remove
                PopulationZones.CalculateBrowseToRemove(parameters);
                
                browseEvent.DisturbSites(parameters);

                return browseEvent;
        }


        //---------------------------------------------------------------------

        private Event()
        {           
            this.sitesDamaged = 0;
            this.population = 0;
            this.cohortsKilled = 0;
            this.biomassRemoved = 0;
            this.biomassKilled = 0;
            this.cohortsKilledSpp = new int[PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                this.cohortsKilledSpp[species.Index] = 0;
            }
            this.biomassRemovedSpp = new int[PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                this.biomassRemovedSpp[species.Index] = 0;
            }
            this.zoneSitesDamaged = new int[PopulationZones.Dataset.Count];
            this.zonePopulation = new int[PopulationZones.Dataset.Count];
            this.zoneCohortsKilled = new int[PopulationZones.Dataset.Count];
            this.zoneBiomassRemoved = new int[PopulationZones.Dataset.Count];
            this.zoneBiomassKilled = new int[PopulationZones.Dataset.Count];
            this.zoneCohortsKilledSpp = new int[PopulationZones.Dataset.Count][];
            this.zoneBiomassRemovedSpp = new int[PopulationZones.Dataset.Count][];
            for (int i = 0; i < PopulationZones.Dataset.Count; i++)
            {
                this.zoneSitesDamaged[i] = 0;
                this.zonePopulation[i] = 0;
                this.zoneCohortsKilled[i] = 0;
                this.zoneBiomassRemoved[i] = 0;
                this.zoneBiomassKilled[i] = 0;
                this.zoneCohortsKilledSpp[i] = new int[PlugIn.ModelCore.Species.Count];
                this.zoneBiomassRemovedSpp[i] = new int[PlugIn.ModelCore.Species.Count];
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    this.zoneCohortsKilledSpp[i][species.Index] = 0;
                    this.zoneBiomassRemovedSpp[i][species.Index] = 0;
                }
            }
        }
         //---------------------------------------------------------------------
        //Go through all active sites and damage them according to the
        // site's browse to be removed.
        private void DisturbSites(IInputParameters parameters)
        {
            PlugIn.ModelCore.UI.WriteLine("   Disturbing Sites");
            foreach (IPopulationZone popZone in PopulationZones.Dataset)
            {
                //double totalPop = 0;
                //double totalFirstPass = 0;
                //double totalSecondPass = 0;
                //double totalRemoval = 0;
                //double totalBrowse = 0;
                //double totalForage = 0;

                foreach (Location siteLocation in PopulationZones.Dataset[popZone.Index].PopulationZoneSites)
                {
                    double siteTotalRemoval = 0;
                    ActiveSite site = (ActiveSite)PlugIn.ModelCore.Landscape.GetSite(siteLocation);

                    //totalPop += SiteVars.LocalPopulation[site];
                    double siteTotalToBrowse = Math.Round(SiteVars.TotalBrowse[site]);
                    //totalBrowse += siteTotalToBrowse;

                    //Browse - allocate browse to cohorts
                    if (siteTotalToBrowse > 0)
                    {
                        List<ICohort> siteCohortList = new List<ICohort>();
                        foreach (ISpecies species in PlugIn.ModelCore.Species)
                        {
                            ISpeciesCohorts cohortList = SiteVars.BiomassCohorts[site][species];
                            if (cohortList != null)
                            {
                                foreach (ICohort cohort in cohortList)
                                {
                                    siteCohortList.Add(cohort);
                                }
                            }
                        }
                        double forageRemoved = 0.0;
                        //int prefIndex = 0;
                        //Browse - compile browse by preference class
                        double[] forageByPrefClass = new double[parameters.PreferenceList.Count];
                        double[] propBrowseList = new double[siteCohortList.Count];
                        //Browse - calculate first pass removal
                        // first pass removes forage at rate equal to preference

                        double[] firstPassRemovalList = new double[siteCohortList.Count];
                        double firstPassRemoval = 0;
                        double firstPassRemovalInt = 0;
                        int cohortLoop = 0;
                        foreach (Landis.Library.BiomassCohorts.ICohort cohort in siteCohortList)
                        {
                            ISppParameters sppParms = parameters.SppParameters[cohort.Species.Index];
                            double browsePref = sppParms.BrowsePref;

                            double availForage = cohort.ForageInReach;
                            //totalForage += availForage;
                            firstPassRemoval += availForage * browsePref;
                            firstPassRemovalInt += Math.Round(availForage * browsePref);
                            firstPassRemovalList[cohortLoop] = availForage * browsePref;

                            int prefIndex = 0;
                            foreach (double prefValue in parameters.PreferenceList)
                            {
                                if (browsePref == prefValue)
                                {
                                    forageByPrefClass[prefIndex] += (availForage - (availForage * browsePref));
                                    break;
                                }
                                prefIndex++;
                            }

                            cohortLoop++;
                        }

                        // First pass adjustment
                        // if first pass exceeds removal then adjust downward for each cohort
                        double[] adjFirstPassRemovalList = new double[siteCohortList.Count];
                        double[] remainingBrowseList = new double[siteCohortList.Count];
                        double adjFirstPassRemoval = firstPassRemoval;
                        double adjFirstPassRemovalInt = firstPassRemovalInt;
                        if (firstPassRemoval > siteTotalToBrowse)
                        {
                            adjFirstPassRemoval = 0;
                            adjFirstPassRemovalInt = 0;
                            int removalIndex = 0;
                            foreach (var i in firstPassRemovalList)
                            {
                                double firstRemoval = firstPassRemovalList[removalIndex];
                                double adjFirstRemoval = firstRemoval * siteTotalToBrowse / firstPassRemoval;
                                adjFirstPassRemovalList[removalIndex] = adjFirstRemoval;
                                adjFirstPassRemoval += adjFirstRemoval;
                                adjFirstPassRemovalInt += Math.Round(adjFirstRemoval);
                                removalIndex++;
                            }
                        }
                        else
                        {
                            adjFirstPassRemovalList = firstPassRemovalList;
                        }
                        int adjIndex = 0;
                        foreach (var i in adjFirstPassRemovalList)
                        {
                            remainingBrowseList[adjIndex] = siteCohortList[adjIndex].ForageInReach - adjFirstPassRemovalList[adjIndex];
                            adjIndex++;
                        }
                        double unallocatedBrowse = siteTotalToBrowse - adjFirstPassRemovalInt;
                        //double totalUnallocatedBrowse = unallocatedBrowse;
                        //totalFirstPass += adjFirstPassRemoval;

                        //Browse - allocate second pass of browse removal to cohorts
                        // second pass removes all of most preferred before moving down in preference
                        double[] secondPassRemovalList = new double[siteCohortList.Count];
                        double[] finalRemovalList = adjFirstPassRemovalList;
                        forageRemoved = adjFirstPassRemovalInt;
                        int prefLoop = 0;
                        foreach (double prefValue in parameters.PreferenceList)
                        {
                            cohortLoop = 0;
                            double prefClassRemoved = 0;
                            foreach (Landis.Library.BiomassCohorts.Cohort cohort in siteCohortList)
                            {
                                ISppParameters sppParms = parameters.SppParameters[cohort.Species.Index];
                                double browsePref = sppParms.BrowsePref;

                                if (browsePref == prefValue)
                                {
                                    double finalRemoval = adjFirstPassRemovalList[cohortLoop];
                                    if (forageRemoved < siteTotalToBrowse)
                                    {
                                        double availForage = cohort.ForageInReach;
                                        //int prefIndex = preferenceList.IndexOf(browsePref);
                                        double prefClassForage = forageByPrefClass[prefLoop];
                                        double secondPassRemoval = 0;
                                        if (prefClassForage > 0)
                                        {
                                            secondPassRemoval = unallocatedBrowse * ((availForage - adjFirstPassRemovalList[cohortLoop]) / prefClassForage);
                                            secondPassRemoval = Math.Min(secondPassRemoval, (availForage - adjFirstPassRemovalList[cohortLoop]));
                                        }
                                        secondPassRemovalList[cohortLoop] = secondPassRemoval;
                                        finalRemoval += secondPassRemoval;
                                        forageRemoved -= Math.Round(adjFirstPassRemovalList[cohortLoop]);
                                        forageRemoved += Math.Round(finalRemoval);
                                        //totalSecondPass += secondPassRemoval;
                                        prefClassRemoved += (Math.Round(finalRemoval) - Math.Round(adjFirstPassRemovalList[cohortLoop]));
                                    }
                                    finalRemoval = Math.Round(finalRemoval);
                                    finalRemovalList[cohortLoop] = finalRemoval;


                                    this.biomassRemoved += (int)finalRemoval;
                                    this.zoneBiomassRemoved[popZone.Index] += (int)finalRemoval;
                                    //totalRemoval += finalRemoval;
                                    siteTotalRemoval += finalRemoval;

                                    double propBrowse = 0;
                                    if (siteCohortList[cohortLoop].Forage > 0)
                                        propBrowse = finalRemoval / siteCohortList[cohortLoop].Forage;
                                    propBrowseList[cohortLoop] = propBrowse;
                                    if (propBrowse < 0 || propBrowse > 1)
                                        PlugIn.ModelCore.UI.WriteLine("   Browse Proportion not between 0 and 1");
                                    if (propBrowse > 0)
                                        PartialDisturbance.RecordLastBrowseProp(cohort, propBrowse);
                                    // Growth reduction is called by Biomass Succession and uses LastBrowseProp in its calculation

                                    // Add mortality
                                    // Browse - Mortality caused by browsing
                                    if (parameters.Mortality)
                                    {
                                        double mortThresh = sppParms.MortThresh;
                                        double mortMax = sppParms.MortMax;
                                        double mortProb = CalculateReduction(mortThresh, mortMax, propBrowse);
                                        PlugIn.ModelCore.ContinuousUniformDistribution.Alpha = 0.0;
                                        PlugIn.ModelCore.ContinuousUniformDistribution.Beta = 1.0;
                                        double myRand = PlugIn.ModelCore.ContinuousUniformDistribution.NextDouble();
                                        myRand = PlugIn.ModelCore.ContinuousUniformDistribution.NextDouble();
                                        if (myRand < mortProb)
                                        {
                                            int biomassKilled = cohort.Biomass - (int)finalRemoval;
                                            finalRemoval = cohort.Biomass;
                                            this.biomassKilled += biomassKilled;
                                            this.zoneBiomassKilled[popZone.Index] += biomassKilled;
                                            this.cohortsKilled += 1;
                                            this.cohortsKilledSpp[cohort.Species.Index] += 1;
                                            this.zoneCohortsKilled[popZone.Index] += 1;
                                            this.zoneCohortsKilledSpp[popZone.Index][cohort.Species.Index] += 1;
                                        }
                                    }
                                    if (finalRemoval > 0)
                                    {
                                        int finalRemovalInt = (int)finalRemoval;
                                        PartialDisturbance.RecordBiomassReduction(cohort, finalRemovalInt);
                                        this.biomassRemovedSpp[cohort.Species.Index] += finalRemovalInt;
                                        this.zoneBiomassKilled[popZone.Index] += finalRemovalInt;
                                        this.zoneBiomassRemovedSpp[popZone.Index][cohort.Species.Index] += finalRemovalInt;
                                    }
                                }
                                cohortLoop++;

                            }
                            unallocatedBrowse -= prefClassRemoved;
                            prefLoop++;
                        }
                        this.sitesDamaged += 1;
                        this.zoneSitesDamaged[popZone.Index] += 1;
                    }
                    PartialDisturbance.ReduceCohortBiomass(site);
                    PartialDisturbance.UpdateLastBrowseProp(site);
                    //bool testSiteRemoval = (Math.Round(siteTotalRemoval) == Math.Round(SiteVars.TotalBrowse[site]));
                    //string msgSiteRemove = "";
                    //if (!testSiteRemoval)
                    //    msgSiteRemove = "Does not match";
                }
                //bool testPop = (totalPop == popZone.EffectivePop);
                //bool testForage = (totalForage == PopulationZones.Dataset[popZone.Index].TotalForage);
                //bool testRemoval = (totalBrowse == totalRemoval) & (totalRemoval == (totalFirstPass + totalSecondPass));
                //bool testZoneRemoval = (this.zoneBiomassRemoved[popZone.Index] == PopulationZones.Dataset[popZone.Index].TotalForage);

            }
        }
        //---------------------------------------------------------------------
        public static double CalculateReduction(double threshold, double max, double propBrowse)
        {
            double reduction = 0;
            if (propBrowse > threshold)
            {
                reduction = (max / (1.0 - threshold)) * propBrowse - threshold * (max / (1 - threshold));
            }
            return reduction;

        }
        //---------------------------------------------------------------------
        private void CalculateForage(IInputParameters parameters)
        {
            // Update available forage by cohort
            //   Forage.UpdateCohortForage(parameters);
            PlugIn.ModelCore.UI.WriteLine("   Calculating Site Preference & Forage.");
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                double maxEcoBiomass = SiteVars.EcoregionMaxBiomass[site];
                double biomassThreshold = maxEcoBiomass * parameters.BrowseBiomassThresh;
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    ISppParameters sppParms = parameters.SppParameters[species.Index];
                    double browsePref = sppParms.BrowsePref;
                    ISpeciesCohorts cohortList = SiteVars.BiomassCohorts[site][species];
                    if (cohortList != null)
                    {

                        foreach (ICohort cohort in cohortList)
                        {
                            int newForage = 0;
                            if ((browsePref > 0) || (parameters.CountNonForage))
                            {
                                newForage = (int)Math.Round(cohort.ANPP * parameters.ANPPForageProp);
                                if (cohort.Age == 1)
                                {
                                    if (parameters.UseInitBiomass)
                                        newForage = (int)Math.Round(cohort.Biomass * parameters.ANPPForageProp);
                                    else
                                        newForage = 0;
                                }
                            }
                            cohort.ChangeForage(newForage);
                            if(newForage > 0)
                                PartialDisturbance.RecordForage(cohort, newForage);
                        }
                    }
                    
                }
                PartialDisturbance.UpdateForage(site);

                List<ICohort> siteCohortList = new List<ICohort>();
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    ISpeciesCohorts cohortList = SiteVars.BiomassCohorts[site][species];
                    if (cohortList != null)
                    {
                        foreach (ICohort cohort in cohortList)
                        {
                            siteCohortList.Add(cohort);
                        }
                    }
                }
                List<double> propInReachList = new List<double>(siteCohortList.Count);
                propInReachList = Forage.CalculateCohortPropInReach(siteCohortList, parameters, biomassThreshold);

                int listCount = 0;
                foreach (ICohort cohort in siteCohortList)
                {
                    int newForageinReach = (int)Math.Round(cohort.Forage * propInReachList[listCount]);
                    if(newForageinReach > 0)
                        PartialDisturbance.RecordForageInReach(cohort, newForageinReach);
                    listCount++;
                }
                PartialDisturbance.UpdateForageInReach(site);

                //  Calculate Site BrowsePreference and ForageQuantity
                SitePreference.CalcSiteForage(parameters, site);
            }
        }

    }
}
