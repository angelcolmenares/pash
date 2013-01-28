// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Management.CloudService.Model
{
    public class ArgumentConstants
    {
        public static Dictionary<Location, string> Locations { get; private set; }
        public static Dictionary<string, Location> ReverseLocations { get; private set; }
        public static Dictionary<Slot, string> Slots { get; private set; }

        static ArgumentConstants()
        {
            Locations = new Dictionary<Location, string>()
            {
                { Location.AnywhereAsia, "anywhere asia" },
                { Location.AnywhereEurope, "anywhere europe" },
                { Location.AnywhereUS, "anywhere us" },
                { Location.EastAsia, "east asia" },
                { Location.NorthCentralUS, "north central us" },
                { Location.NorthEurope, "north europe" },
                { Location.SouthCentralUS, "south central us" },
                { Location.SouthEastAsia, "southeast asia" },
                { Location.WestEurope, "west europe" },
                { Location.EastUS, "east us" },
                { Location.WestUS, "west us" }
            };

            ReverseLocations = new Dictionary<string, Location>()
            {
                { "anywhere asia", Location.EastAsia },
                { "anywhere europe", Location.NorthEurope },
                { "anywhere us", Location.SouthCentralUS },
                { "east asia", Location.EastAsia },
                { "north central us", Location.NorthCentralUS },
                { "north europe", Location.NorthEurope },
                { "south central us", Location.SouthCentralUS },
                { "southeast asia", Location.SouthEastAsia },
                { "west europe", Location.WestEurope },
                { "west us", Location.WestUS },
                { "east us", Location.EastUS },
            };
            Slots = new Dictionary<Slot, string>()
            {
                { Slot.Production, "production" },
                { Slot.Staging, "staging" }
            };
        }
    }

    public enum Location
    {
        NorthCentralUS,
        AnywhereUS,
        AnywhereEurope,
        WestEurope,
        SouthCentralUS,
        NorthEurope,
        AnywhereAsia,
        SouthEastAsia,
        EastAsia,
        EastUS,
        WestUS
    }
    
    public enum Slot
    {
        Production,
        Staging
    }
    
    public enum DevEnv
    {
        Local,
        Cloud
    }

    public enum RoleType
    {
        WebRole,
        WorkerRole
    }

    public enum Runtime
    {
        IISNode,
        Node,
        PHP,
        Null
    }
}