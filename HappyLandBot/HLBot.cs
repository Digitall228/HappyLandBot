using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HappyLandBot
{
    public class HLBot
    {
        public HLClient client { get; set; }
        public Land[] lands { get; set; }
        public JObject _playerData { get; set; }
        public JObject playerData
        {
            get { return _playerData; }
            set
            {
                _playerData = value;
                Logger.UpdateTitle($"[{_playerData["level"]} {_playerData["pointExpNow"]}/{_playerData["maxExpNow"]}] {_playerData["username"]}: {_playerData["hpl"]}/{_playerData["hpw"]} ({_playerData["hplReward"]}/{_playerData["hpwReward"]})");
            }
        }
        public HLBot(HLClient Client)
        {
            client = Client;
            lands = new Land[0];

            if (!client.Auth())
            {
                Logger.logAdd($"[{client.username}] Auth failed", ConsoleColor.Red);
                return;
            }

            JArray landsInfo = client.GetListUserLands();
            lands = new Land[landsInfo.Count];

            for (int i = 0; i < landsInfo.Count; i++)
            {
                lands[i] = new Land();
                lands[i].landInfo = (JObject)landsInfo[i];
                lands[i].landTokenId = (string)landsInfo[i]["landTokenId"];
                CollectInfo(lands[i]);
            }

            JObject playerInfo = client.GetPlayerInfo(lands[0].landTokenId);
            playerData = (JObject)playerInfo["dataPlayer"];
        }
        
        public void Monitoring()
        {
            while (true)
            {
                Thread.Sleep(15000);
                for (int i = 0; i < lands.Length; i++)
                {
                    if (CheckCattleFarms(lands[i]) || CheckFarms(lands[i]))
                    {
                        Thread.Sleep(15000);
                        CollectInfo(lands[i]);
                        SellUserStore(lands[i]);
                    }
                }
            }
        }
        public bool CheckFarms(Land land)
        {
            bool is_changed = false;

            JArray farms = land.farms;
            List<int[]> warmedPlants = new List<int[]>();
            List<int[]> needWaterPlants = new List<int[]>();

            for (int i = 0; i < farms.Count; i++)
            {
                if (farms[i]["tree"].Type != JTokenType.Null)
                {
                    if (CheckTime((long)farms[i]["tree"]["nextWater"]))
                    {
                        is_changed = true;
                    }
                    if(CheckTime((long)farms[i]["tree"]["nextWorm"]))
                    {
                        is_changed = true;   
                    }
                    if ((bool)farms[i]["tree"]["isWater"])
                    {
                        int[] position = ParsePlotPosition(farms[i]["positon"]);
                        needWaterPlants.Add(position);
                    }
                    if ((bool)farms[i]["tree"]["isWorm"])
                    {
                        int[] position = ParsePlotPosition(farms[i]["positon"]);
                        warmedPlants.Add(position);
                    }
                    if (CheckTime((long)farms[i]["tree"]["growthAt"]))
                    {
                        is_changed = true;

                        int[] position = ParsePlotPosition(farms[i]["positon"]);
                        JArray resultFarm = client.HarvestPlant(land.landTokenId, new List<int[]>() { position });
                        if (resultFarm != null)
                        {
                            if (PlantSeed(land, (JObject)farms[i]))
                            {
                                Logger.logAdd($"Successfully planted a seed", ConsoleColor.Green);
                            }
                            else
                            {
                                Logger.logAdd($"Failed to plant seed", ConsoleColor.Red);
                            }
                        }
                    }
                    if ((bool)farms[i]["tree"]["isDead"])
                    {
                        is_changed = true;
                        int[] position = ParsePlotPosition(farms[i]["positon"]);
                        client.ProccessTreeDead(land.landTokenId, position);
                    }
                }
                else if (farms[i]["plot"].Type != JTokenType.Null)
                {
                    /*is_changed = true;
                    if (PlantSeed(land, (JObject)farms[i]))
                    {
                        Logger.logAdd($"Successfully planted a seed", ConsoleColor.Green);
                    }
                    else
                    {
                        Logger.logAdd($"Failed to plant seed", ConsoleColor.Red);
                    }*/
                }
            }

            if (warmedPlants.Count > 0)
            {
                is_changed = true;
                client.CatchWorm(land.landTokenId, warmedPlants);
            }
            if(needWaterPlants.Count > 0)
            {
                is_changed = true;
                client.WaterPlant(land.landTokenId, needWaterPlants);
            }

            return is_changed;
        }
        public bool CheckCattleFarms(Land land)
        {
            bool is_changed = false;

            for (int i = 0; i < land.cattleFarms.Count; i++)
            {
                string animalType = (string)land.cattleFarms[i]["typeAnimal"];

                bool needToCollect = false;

                JArray animals = (JArray)land.cattleFarms[i]["data"];
                List<string> starveAnimals = new List<string>();

                for (int j = 0; j < animals.Count; j++)
                {
                    if (CheckTime((long)animals[j]["nextFeed"]))
                    {
                        starveAnimals.Add((string)animals[j]["id"]);
                    }
                    if (CheckTime((long)animals[j]["nextProduct"]))
                    {
                        is_changed = true;
                        if (animalType == "animalFish")
                        {
                            if (client.CollectAgricultural(land.landTokenId, animalType, (string)animals[j]["id"]) != null)
                            {
                                client.BuyAnimal(land.landTokenId, 6, 1);
                            }
                        }
                        else
                        {
                            needToCollect = true;
                        }
                    }
                    if ((bool)animals[j]["isDead"])
                    {
                        is_changed = true;
                        client.ProccessAnimalDead(land.landTokenId, (string)animals[j]["id"], animalType);
                    }
                }

                if (needToCollect)
                {
                    is_changed = true;
                    client.CollectAgricultural(land.landTokenId, animalType);
                }

                if (starveAnimals.Count > 0)
                {
                    is_changed = true;
                    client.FeedAnimal(land.landTokenId, animalType, starveAnimals.ToArray());
                }
            }

            return is_changed;
        }
        public void SellUserStore(Land land)
        {
            JObject userStore = client.GetUserStore(land.landTokenId);
            JArray products = (JArray)userStore["userStore"];

            for (int i = 0; i < products.Count; i++)
            {
                client.SellProducts(land.landTokenId, (string)products[i]["id"], (int)products[i]["count"]);
            }
        }
        public bool PlantSeed(Land land, JObject farm)
        {
            JArray seeds = client.GetListPlantSeeds(land.landTokenId);
            List<int[]> position = new List<int[]>() { ParsePlotPosition(farm["positon"]) };

            if (seeds.Count <= 0)
            {
                if (farm["tree"].Type != JTokenType.Null)
                {
                    JObject seed = client.BuySeed(land.landTokenId, (int)farm["tree"]["properties"]["id"], 1);
                    if (seed != null)
                    {
                        if (client.PlantSeed(land.landTokenId, (int)farm["tree"]["properties"]["id"], position) != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    JObject seed = client.BuySeed(land.landTokenId, 8, 1);
                    if (seed != null)
                    {
                        if (client.PlantSeed(land.landTokenId, 8, position) != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (client.PlantSeed(land.landTokenId, (int)seeds[0]["id"], position) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool BuyPlot(Land land, string rarity)
        {
            if (client.BuyPlot(land.landTokenId, rarity) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CollectInfo(Land land)
        {
            JObject playerInfo = client.GetPlayerInfo(land.landTokenId);
            land.farms = (JArray)playerInfo["Farm"];
            land.cattleFarms = (JArray)playerInfo["CattleFarm"];

            playerData = (JObject)playerInfo["dataPlayer"];
        }
        public int[] ParsePlotPosition(JToken position)
        {
            int[] pos = new int[] { (int)position[0], (int)position[1] };
            return pos;
        }
        public bool CheckTime(long time)
        { 
            if(DateTimeOffset.Now.ToUnixTimeMilliseconds() >= time)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
