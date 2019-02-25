﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeasonPredict
{
    public class Player : Person
    {
        public List<Season> SeasonList { get; private set; }
        public Season ExpectedSeason { get; private set; }

        public void Add(Season s) => SeasonList.Add(s);
        public void Remove(Season s) => SeasonList.Remove(s);

        public Player(List<Season> seasonsToDuplicate)
        {
            SeasonList = new List<Season>();
            ExpectedSeason = new Season();
            FullName = "";

            foreach (Season s in seasonsToDuplicate)
            {
                Add(Season.Duplicate(s));
            }
            CalculateExpectedSeason();
        }

        public Player(Player p, string name, string id) : this(p.SeasonList)
        {
            Id = id;
            FullName = name;
        }

        /// <summary>
        /// Complete player duplicator: calls the 3-parameter constructor
        /// </summary>
        /// <param name="p"></param>
        /// <returns>The duplication of the player passed as a parameter</returns>
        public static Player Duplicate(Player p) => new Player(p, p.FullName, p.Id);
        
        /// <summary>
        /// Calculates an estimation of the player's next season by computing a weighted average with the most important season being the most recent
        /// </summary>
        public void CalculateExpectedSeason()
        {
            double total = 0.0;
            List<double> weightsList = new List<double>();
            int i = 0, averageGames = (int)SeasonList.Average(p => p.GamesPlayed);

            for (i = 0; i < SeasonList.Count; i++)
            {
                if(SeasonList.Count > 5)//If there are enough seasons to eliminate the ones below games played average
                {
                    if (SeasonList[i].GamesPlayed >= averageGames)//If above games played average
                    {
                        AddWeight(weightsList, i);
                    }
                    else//Eliminate season with below average games played
                    {
                        Remove(SeasonList[i]);
                        i--;//Stay at the same index since the next one is moved back
                    }
                }
                else
                {
                    AddWeight(weightsList, i);
                }
            }
            //Total of all absolute weights used to calculate relative weight of each season in next step
            total = weightsList.Sum();

            for (i = 0; i < weightsList.Count;i++)
            {
                IncrementSeasonWeight(weightsList, i, total);
            }

            if (ExpectedSeason.GamesPlayed > 82)
                ExpectedSeason.GamesPlayed = 82;

            ExpectedSeason.CalculatePoints();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weightList">Current list of weights each season has on the overall calculation</param>
        /// <param name="i">Current season index</param>
        /// <param name="total">Sum of all season weights</param>
        private void IncrementSeasonWeight(List<double> weightList, int i, double total)
        {
            weightList[i] /= total;//Making this season's weight into percentage
            ExpectedSeason.Assists += (int) Math.Round((SeasonList[i].Assists * weightList[i]));
            ExpectedSeason.Goals += (int) Math.Round((SeasonList[i].Goals * weightList[i]));
            ExpectedSeason.GamesPlayed += (int) Math.Round((SeasonList[i].GamesPlayed * weightList[i]));
        }

        private void AddWeight(List<double> weightsList, int i)
        {
            if (i == 0)
            {
                weightsList.Add((double)(SeasonList.Count - i) * 0.5f);
            }
            else if (i == 1)
                weightsList.Add((double)(SeasonList.Count - i) / (double)(SeasonList.Count));
            else
                weightsList.Add((double)(SeasonList.Count - i) / (double)(SeasonList.Count * (i + 1)));
        }

        public override string ToString()
        {
            if (SeasonList.Count > 2)
            {
                return FullName
                     + "\nAssists: " + ExpectedSeason.Assists
                     + "\nGoals: " + ExpectedSeason.Goals
                     + "\nPoints: " + ExpectedSeason.Points
                     + "\nGames played: " + ExpectedSeason.GamesPlayed;
            }
            else
            {
                return "Insufficient number of seasons.";
            }
        }
    }


    //------------------------------------------------------------------------------------------------
    //Objects needed for deserialization of the JSON persons/stats coming from the NHL's API
    public class Stat2
    {
        public int Assists { get; set; }
        public int Goals { get; set; }
        public int Games { get; set; }
    }
    public class Split
    {
        public Stat2 Stat { get; set; }
        public string Season { get; set; }
    }
    public class Stat
    {
        public List<Split> Splits { get; set; }
    }
    public class StatsList
    {
        public List<Stat> Stats { get; set; }
        private int Assists => Stats[0].Splits[0].Stat.Assists;
        private int Goals => Stats[0].Splits[0].Stat.Goals;
        private int Games => Stats[0].Splits[0].Stat.Games;
        public Season Season => new Season(Assists, Goals, Games);
    }

    public class Position
    {
        public string Code { get; set; }
    }
    public class Person
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public bool Active { get; set; }

        public Person()
        {
            Active = true;
        }
    }
    public class PersonList
    {
        public List<Person> People { get; set; }
    }
}
