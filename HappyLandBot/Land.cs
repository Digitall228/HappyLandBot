using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyLandBot
{
    public class Land
    {
        public JObject landInfo { get; set; }
        public string landTokenId { get; set; }
        public JArray farms { get; set; }
        public JArray cattleFarms { get; set; }
    }
}
