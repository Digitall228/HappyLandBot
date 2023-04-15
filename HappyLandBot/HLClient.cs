using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyLandBot
{
    public class HLClient
    {
        public string username { get; set; }
        public string password { get; set; }
        public HttpRequest request { get; set; }
        public string url { get; set; }
        public string token { get; set; }
        public HLClient(string Username, string Password, string Url = "https://api-eu.happyland.finance/api/farm-happyland-finace")
        {
            username = Username;
            password = Password;
            url = Url;

            request = new HttpRequest();
            request.UserAgentRandomize();
        }
        public bool Auth()
        {
            token = GetUserToken();
            if (!string.IsNullOrEmpty(token))
            {
                request["authorization"] = $"Bearer {token}";

                Logger.logAdd?.Invoke($"[{username}] Logged in successfully", ConsoleColor.Green);

                return true;
            }
            else
            {
                return false;
            }
        }
        public HttpResponse SendRequest(Func<HttpResponse> action)
        {
            try
            {
                return action?.Invoke();
            }
            catch(Exception ex)
            {
                Logger.logAdd($"HANDLED ERROR: {ex.Message}", ConsoleColor.Red);
                Thread.Sleep(5000);
                return SendRequest(action);
            }
        }
        public string GetUserToken()
        {
            try
            {
                string data = $@"{{""username"":""{Convert.ToBase64String(Encoding.UTF8.GetBytes(username))}"",""password"":""{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}""}}";
                string response = request.Post(url + "/login", data, "application/json; charset=utf-8").ToString();

                JObject json = JObject.Parse(response);
                if ((string)json["message"] == "success")
                {
                    return (string)json["data"]["token"];
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while authing\nResponse={json}", ConsoleColor.Red);
                    return string.Empty;
                }
            }
            catch(Exception ex)
            {
                Logger.logAdd?.Invoke($"ERROR while authing: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return string.Empty;
            }
        }
        public JArray GetListUserLands()
        {
            try
            {
                string data = $@"{{""landTokenId"":""""}}";
                string response = request.Post(url + "/getListUserLand", data, "application/json; charset=utf-8").ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JArray)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetListUserLands();
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while parsing lands\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while parsing lands: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public bool SaveLastVisitedTime(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = request.Post(url + "/saveLastVisitedTime", data, "application/json; charset=utf-8").ToString();

                JObject json = JObject.Parse(response);
                if ((string)json["data"]["content"] == "Successfully")
                {
                    return true;
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return SaveLastVisitedTime(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while saving last visited time\nResponse={json}", ConsoleColor.Red);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while saving last visited time: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return false;
            }
        }
        public JObject GetPlayerInfo(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";

                string response = SendRequest(() => request.Post(url + "/getPlayerInfo", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetPlayerInfo(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting player info\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting player info: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray GetListUserPlots(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";

                string response = SendRequest(() => request.Post(url + "/getListUserPlot", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JArray)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetListUserPlots(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting player info\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting player info: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject GetDataAnimalsShop(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/getDataAnimalsShop", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetDataAnimalsShop(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting animals shop data\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting animals shop data: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject BuyAnimal(string landTokenId, int animalId, int quantity)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""id"":{animalId},""quantity"":{quantity}}}";
                string response = SendRequest(() => request.Post(url + "/buyAnimal", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"]}", ConsoleColor.Green);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return BuyAnimal(landTokenId, animalId, quantity);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while buying animal\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while buying animal: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject GetDataAnimal(string landTokenId, string animalId, int typeAnimal)
        {
            try
            {
                string data = $@"{{""animalId"":""{animalId}"",""landTokenId"":""{landTokenId}"",""typeAnimal"":""{typeAnimal}""}}";
                string response = SendRequest(() => request.Post(url + "/getDataAnimal", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetDataAnimal(landTokenId, animalId, typeAnimal);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting animal data\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting animal data: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject FeedAnimal(string landTokenId, string animalType, string[] listAnimalsIds)
        {
            try
            {
                JArray listAnimalId = new JArray();
                for (int i = 0; i < listAnimalsIds.Length; i++)
                {
                    listAnimalId.Add(listAnimalsIds[i]);
                }

                JObject data = new JObject();
                data["landTokenId"] = landTokenId;
                data["listAnimalId"] = listAnimalId;
                data["supportItemId"] = null;
                data["type"] = "feed";
                data["typeAnimal"] = animalType;

                string stringData = JsonConvert.SerializeObject(data);

                string response = SendRequest(() => request.Post(url + "/getDataAnimalCare", stringData, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 106)
                {
                    Logger.logAdd($"[{username}] Successfully fed {animalType}", ConsoleColor.Green);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return FeedAnimal(landTokenId, animalType, listAnimalsIds);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while feeding animal\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while feeding animal: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject BuyPlot(string landTokenId, string rarity)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"", ""rarity"":""{rarity}""}}";
                string response = SendRequest(() => request.Post(url + "/buyPlot", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] Successfully puchased a plot", ConsoleColor.Green);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return BuyPlot(landTokenId, rarity);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while purchasing a plot\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while purchasing a plot: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject DeployPlot(string landTokenId, string tokenId, int[] position)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""tokenId"":""{tokenId}"",""positionId"":[{position[0]},{position[1]}]}}";
                string response = SendRequest(() => request.Post(url + "/selectPlot", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] Successfully deployed a plot on [{position[0]},{position[1]}]", ConsoleColor.Green);
                    return (JObject)json["data"]["land"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return DeployPlot(landTokenId, tokenId, position);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while deploying a plot\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while deploying a plot: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject GetDataSeedsShop(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/getDataSeedsShop", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetDataSeedsShop(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting seeds shop data\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting seeds shop data: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject BuySeed(string landTokenId, int seedId, int quantity)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""id"":{seedId},""quantity"":{quantity}}}";
                string response = SendRequest(() => request.Post(url + "/buySeed", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"]}", ConsoleColor.Green);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return BuySeed(landTokenId, seedId, quantity);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while buying seed\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while buying seed: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray GetListPlantSeeds(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/getListPlantSeeds", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JArray)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetListPlantSeeds(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting player info\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting player info: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray PlantSeed(string landTokenId, int seedId, List<int[]> positions)
        {
            try
            {
                JArray listPositionId = new JArray();
                JArray positionId = new JArray();
                for (int i = 0; i < positions.Count; i++)
                {
                    positionId.Add(positions[i]);
                    listPositionId.Add(positionId);
                }

                JObject data = new JObject();
                data["landTokenId"] = landTokenId;
                data["type"] = "plant";
                data["seedId"] = seedId;
                data["listPositionId"] = listPositionId;
                data["supportItemId"] = null;

                string stringData = JsonConvert.SerializeObject(data);

                string response = SendRequest(() => request.Post(url + "/getDataPlantCare", stringData, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 102)
                {
                    Logger.logAdd($"[{username}] Successfully planted {seedId} seed", ConsoleColor.Green);
                    return (JArray)json["data"]["farm"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return PlantSeed(landTokenId, seedId, positions);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while planting\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while planting: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray WaterPlant(string landTokenId, List<int[]> positions)
        {
            try
            {
                JArray listPositionId = new JArray();
                JArray positionId = new JArray();
                for (int i = 0; i < positions.Count; i++)
                {
                    positionId.Add(positions[i]);
                    listPositionId.Add(positionId);
                }

                JObject data = new JObject();
                data["landTokenId"] = landTokenId;
                data["listPositionId"] = listPositionId;
                data["seedId"] = null;
                data["supportItemId"] = null;
                data["type"] = "watered";

                string stringData = JsonConvert.SerializeObject(data);

                string response = SendRequest(() => request.Post(url + "/getDataPlantCare", stringData, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["data"]["content"][0]["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"][0]["message"]}", ConsoleColor.Green);
                    return (JArray)json["data"]["farm"];
                }
                else if ((int)json["data"]["content"][0]["errorCode"] == 908)
                {
                    Logger.logAdd($"[{username}] Don't need watering", ConsoleColor.DarkYellow);
                    return null;
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return WaterPlant(landTokenId, positions);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while planting\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while planting: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray CatchWorm(string landTokenId, List<int[]> positions)
        {
            try
            {
                JArray listPositionId = new JArray();
                JArray positionId = new JArray();
                for (int i = 0; i < positions.Count; i++)
                {
                    positionId.Add(positions[i]);
                    listPositionId.Add(positionId);
                }

                JObject data = new JObject();
                data["landTokenId"] = landTokenId;
                data["listPositionId"] = listPositionId;
                data["seedId"] = null;
                data["supportItemId"] = null;
                data["type"] = "worm";

                string stringData = JsonConvert.SerializeObject(data);

                string response = SendRequest(() => request.Post(url + "/getDataPlantCare", stringData, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["data"]["content"][0]["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"][0]["message"]}", ConsoleColor.Green);
                    return (JArray)json["data"]["farm"];
                }
                else if ((int)json["data"]["content"][0]["errorCode"] == 915)
                {
                    Logger.logAdd($"[{username}] There are no worms in this tree", ConsoleColor.DarkYellow);
                    return null;
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return CatchWorm(landTokenId, positions);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while catching worms\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while catching worms: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public int GetTimeCurrent(string landTokenId, string playerToken)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"", ""id"":""{playerToken}""}}";

                string response = SendRequest(() => request.Post(url + "/getTimeCurrent", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (int)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetTimeCurrent(landTokenId, playerToken);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting current time\nResponse={json}", ConsoleColor.Red);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting current time: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return 0;
            }
        }
        public JObject GetUserSupportItems(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/getUserSupportItem", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetUserSupportItems(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting support items\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting support items: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject ProccessTreeDead(string landTokenId, int[] position)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""positionId"":[{position[0]},{position[1]}]}}";
                string response = SendRequest(() => request.Post(url + "/processTreeDead", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] Cleaned up dead tree", ConsoleColor.Magenta);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return ProccessTreeDead(landTokenId, position);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while proccessing dead tree\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while proccessing dead tree: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject ProccessAnimalDead(string landTokenId, string animalId, string animalType)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""animalId"":""{animalId}"",""typeAnimal"":""{animalType}""}}";

                string response = SendRequest(() => request.Post(url + "/processAnimalDead", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] Cleaned up dead animal", ConsoleColor.Magenta);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return ProccessAnimalDead(landTokenId, animalId, animalType);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while proccessing dead animal\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while proccessing dead animal: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject CollectAgricultural(string landTokenId, string animalType, string animalId = "")
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""typeAnimal"":""{animalType}""}}";
                if (animalType == "animalFish")
                {
                    data = $@"{{""landTokenId"":""{landTokenId}"",""typeAnimal"":""{animalType}"",""animalId"":""{animalId}""}}";
                }
                string response = SendRequest(() => request.Post(url + "/collectAgricultural", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"]} from {animalType}", ConsoleColor.DarkYellow);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return CollectAgricultural(landTokenId, animalType, animalId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while collecting agricultural\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while collecting agricultural: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JArray HarvestPlant(string landTokenId, List<int[]> positions)
        {
            try
            {
                JArray listPositionId = new JArray();
                JArray positionId = new JArray();
                for (int i = 0; i < positions.Count; i++)
                {
                    positionId.Add(positions[i]);
                    listPositionId.Add(positionId);
                }

                JObject data = new JObject();
                data["landTokenId"] = landTokenId;
                data["listPositionId"] = listPositionId;
                data["seedId"] = null;
                data["supportItemId"] = null;
                data["type"] = "harvest";

                string stringData = JsonConvert.SerializeObject(data);

                string response = SendRequest(() => request.Post(url + "/getDataPlantCare", stringData, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["data"]["content"][0]["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"][0]["message"]}", ConsoleColor.Green);
                    return (JArray)json["data"]["farm"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return HarvestPlant(landTokenId, positions);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while planting\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while planting: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject GetUserStore(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/getUserStore", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetUserStore(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting user store\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting user store: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject UpgradeUserStore(string landTokenId)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}""}}";
                string response = SendRequest(() => request.Post(url + "/updateUserStore", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"]}", ConsoleColor.Magenta);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return UpgradeUserStore(landTokenId);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting user store\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting user store: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject GetDataLandPosition(string landTokenId, int[] position)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""positionId"":[{position[0]},{position[1]}]}}";
                string response = SendRequest(() => request.Post(url + "/getDataLandPosition", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return GetDataLandPosition(landTokenId, position);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while getting user store\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while getting user store: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
        public JObject SellProducts(string landTokenId, string productType, int quantity)
        {
            try
            {
                string data = $@"{{""landTokenId"":""{landTokenId}"",""id"":""{productType}"",""quantity"":{quantity}}}";
                string response = SendRequest(() => request.Post(url + "/sellProduct", data, "application/json; charset=utf-8")).ToString();

                JObject json = JObject.Parse(response);
                if ((int)json["errorCode"] == 0)
                {
                    Logger.logAdd($"[{username}] {json["data"]["content"]} for {json["data"]["takeHpw"]} HPW and {json["data"]["takeHpl"]} HPL", ConsoleColor.Magenta);
                    return (JObject)json["data"];
                }
                else if ((int)json["errorCode"] == 24)
                {
                    Auth();
                    return SellProducts(landTokenId, productType, quantity);
                }
                else
                {
                    Logger.logAdd?.Invoke($"[{username}] Something gone wrong while selling products\nResponse={json}", ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.logAdd?.Invoke($"[{username}] ERROR while selling products: {ex.Message}, \nStackTrace: {ex.StackTrace}", ConsoleColor.Red);
                return null;
            }
        }
    }
}
