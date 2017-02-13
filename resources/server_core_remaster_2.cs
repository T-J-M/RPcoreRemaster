using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using System.Linq;
using System.IO;



public class server_core_remaster_2 : Script
{
    static System.Timers.Timer timer;
    public bool timer_debouce = true;
    public server_core_remaster_2()
    {
        API.onResourceStart += OnResourceStartHandler;
        API.onPlayerConnected += OnPlayerConnectedHandler;
        API.onClientEventTrigger += OnClientEventTriggerHandler;
        API.onChatMessage += OnChatMessageHandler;
        API.onChatCommand += OnChatCommandHandler;
        API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
        Blip newblip = API.createBlip(new Vector3(-61.70732, -1093.239, 26.4819));
        API.setBlipSprite(newblip, 380);
        API.setBlipColor(newblip, 47);
        API.setBlipScale(newblip, 1.0f);

        blip_database.Add(newblip);

        timer = new System.Timers.Timer(10000);
        timer.Elapsed += timer_Elapsed;
        timer.AutoReset = true;
        timer.Enabled = true;

        
    }

    public void applyPaychecks()
    {
        if(timer_debouce)
        {
            timer_debouce = false;
            for (int i = 0; i < player_database.Count; i++)
            {
                PlayerData temp = player_database[i];
                if (temp.player_logged == true && temp.player_online == true)
                {
                    temp.player_money_bank += temp.player_paycheck;
                    player_database[i] = temp;
                    API.sendChatMessageToPlayer(temp.player_client, "Your paycheck of ~b~$" + temp.player_paycheck.ToString("N0") + " ~w~has come in!");
                }
            }
        }
    }

    [Command("time")]
    public void timeFunc(Client player)
    {
        API.sendChatMessageToPlayer(player, "Current Time: ~b~ " + DateTime.Now.ToString());
    }

    [Command("setpaycheck", GreedyArg = true)]
    public void setPayCheckFunc(Client player, string arg)
    {
        long paycheck = Convert.ToInt64(arg);
        int indx = getPlayerDatabaseIndexByClient(player);
        PlayerData temp = player_database[indx];
        temp.player_paycheck = paycheck;
        player_database[indx] = temp;
        API.sendChatMessageToPlayer(player, "Paycheck applied.");
    }

    void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if(DateTime.Now.Minute == 00)
        {
            applyPaychecks();
        }
        else if(DateTime.Now.Minute != 00 && timer_debouce == false)
        {
            timer_debouce = true;
        }
    }

    [Flags]
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7
    }

    Vector3[] loginscreen_locations = new Vector3[]
    {
        new Vector3(-438.796, 1075.821, 353.000),
        new Vector3(2495.127, 6196.140, 202.541),
        new Vector3(-1670.700, -1125.000, 50.000),
        new Vector3(1655.813, 0.889, 200.0),
        new Vector3(1070.206, -711.958, 70.483),
    };

    public struct DoubleVector3
    {
        public Vector3 pos;
        public Vector3 rot;
        public DoubleVector3(Vector3 p, Vector3 r)
        {
            pos = p;
            rot = r;
        }
    }

    public struct StoreData
    {
        public string name;
        public string store_type_id;
        public string item_category;
        public Vector3 location;

        public StoreData(string n, string item, string type, Vector3 loc)
        {
            name = n;
            item_category = item;
            store_type_id = type;
            location = loc;
        }
    }

    StoreData[] store_locations = new StoreData[]
    {
        new StoreData("~o~Premium Deluxe \nMotorsport", "Vehicle(s):", "dealership_1", new Vector3(-61.70732, -1093.239, 26.4819)),
    };

    DoubleVector3[] dealership_1_spawn_locations = new DoubleVector3[]
    {
        new DoubleVector3(new Vector3(-42.30667, -1116.657, 26.02187), new Vector3(0.05719259, -0.00925794, -0.3153604)),
        new DoubleVector3(new Vector3(-47.85758, -1116.692, 26.02124), new Vector3(0.002204725, -0.0001127388, 2.004499)),
        new DoubleVector3(new Vector3(-50.67634, -1116.675, 26.02167), new Vector3(-0.007012921, 0, 2.671517)),
        new DoubleVector3(new Vector3(-53.56318, -1116.988, 26.02145), new Vector3(-0.009828809, 0.00459637, 0.3640674)),
        new DoubleVector3(new Vector3(-56.30627, -1117.286, 26.02164), new Vector3(-0.02442851, 0.001244397, 0.08249664)),
        new DoubleVector3(new Vector3(-43.70016, -1109.625, 26.02453), new Vector3(-0.1142287, -0.05879881, 71.22818)),
        new DoubleVector3(new Vector3(-52.19479, -1106.735, 26.02584), new Vector3(-0.1178563, -0.06428397, 71.15902)),
    };

    public static Random rnd = new Random();

    public struct ColorData
    {
        public int[] colors;
        public string color_name;

        public ColorData(string name, int[] color_codes)
        {
            colors = color_codes;
            color_name = name;
        }
    }

    public ColorData[] color_names = new ColorData[]
    {
        new ColorData("Black", new int[] {0, 1, 11, 12, 15, 17, 21, 22, 147}),
        new ColorData("Silver", new int[] {2, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 17, 18, 19, 20, 23, 24, 25, 26, 117, 118, 119, 156, 144}),
        new ColorData("Chrome", new int[]  {120 }),
        new ColorData("Red", new int[] {27, 28, 29, 30, 31, 32, 33, 34, 35, 39, 40, 43, 44, 45, 46, 47, 48, 150, 143}),
        new ColorData("Orange", new int[]  {36, 38, 41, 123, 124, 130, 138}),
        new ColorData("Yellow", new int[] {42, 37, 88, 89, 91, 126, 160, 158, 159}),
        new ColorData("Green", new int[]  {49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 92, 125, 128, 133, 139, 151, 155}),
        new ColorData("Blue", new int[]  {61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 73, 74, 75, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 127, 140, 146, 157}),
        new ColorData("Purple", new int[]  {71, 72, 76, 141, 142, 145, 148, 149}),
        new ColorData("Brown", new int[]  {90, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 114, 115, 116, 129, 152, 153, 154}),
        new ColorData("White", new int[]  {111, 112, 113, 121, 122, 131, 132, 134}),
        new ColorData("Pink", new int[]  {135, 136, 137}),
    };

    public int[] common_car_colors = new int[]
    {
        2, 5, 7, 20, 25, 60, 113, 122, 78
    };

    public class ObjectData
    {
        public ObjectId Id { get; set; }
        public int id { get; set; }
        public int obj_id { get; set; }
        public NetHandle obj { get; set; }
        public SphereColShape coll_shape { get; set; }
        public bool is_static { get; set; }
        public string name { get; set; }
        public ObjectData(int id_par, int obj_id_par, NetHandle obj_par, bool is_static_par, string name_par)
        {
            this.id = id_par;
            this.obj_id = obj_id_par;
            this.obj = obj_par;
            this.is_static = is_static_par;
            this.name = name_par;
            this.coll_shape = null;
        }

        public ObjectData()
        {
            this.id = -1;
            this.obj_id = -1;
            this.obj = new NetHandle();
            this.coll_shape = null;
            this.is_static = false;
            this.name = "";
        }
    }


    public class VehicleData
    {
        public ObjectId Id { get; set; }
        public Vehicle vehicle_object;
        public int vehicle_id;
        public string car_model_name;

        public bool vehicle_engine;
        public bool vehicle_locked;
        public int vehicle_primary_color;
        public int vehicle_secondary_color;
        public string vehicle_color;

        public Vector3 vehicle_position;
        public Vector3 vehicle_rotation;

        public string vehicle_license;
        public string vehicle_owner;
        public string vehicle_faction;

        public List<ObjectData> vehicle_inventory;

        public VehicleData(Vehicle hash, int id, string model_name, Vector3 pos, Vector3 rot, string license, string owner, string faction)
        {
            this.vehicle_object = hash;
            this.vehicle_id = id;

            this.car_model_name = model_name;

            this.vehicle_position = pos;
            this.vehicle_rotation = rot;
            this.vehicle_license = license;
            this.vehicle_owner = owner;
            this.vehicle_faction = faction;


            this.vehicle_engine = false;
            this.vehicle_locked = true;
            this.vehicle_primary_color = 0;
            this.vehicle_secondary_color = 0;
            this.vehicle_color = "Black";

            this.vehicle_inventory = new List<ObjectData>();
        }
    }


    public class PlayerData
    {
        public ObjectId Id { get; set; }
        public Client player_client { get; set; }
        public int player_id { get; set; }
        public string player_display_name { get; set; }
        public string player_game_name { get; set; }
        public string player_password { get; set; }

        public bool player_online { get; set; }
        public bool player_logged { get; set; }
        public bool player_registered { get; set; }

        public Vector3 player_position { get; set; }
        public Vector3 player_rotation { get; set; }
        public PedHash player_ped_hash { get; set; }
        public long player_money_bank { get; set; }
        public long player_money_hand { get; set; }
        public long player_paycheck { get; set; }
        public string player_faction { get; set; }

        public int player_vehicles_owned { get; set; }

        public List<ObjectData> player_inventory { get; set; }

        public PlayerData(Client player, int id, PedHash ped_hash, string player_name, string display_name, string password)
        {
            this.player_client = player;
            this.player_id = id;
            this.player_display_name = display_name;
            this.player_game_name = player_name;
            this.player_password = password;

            this.player_online = true;
            this.player_logged = false;
            this.player_registered = false;

            this.player_position = new Vector3(0.0, 0.0, 0.0);
            this.player_rotation = new Vector3(0.0, 0.0, 0.0);
            this.player_ped_hash = ped_hash;
            this.player_money_bank = 0;
            this.player_money_hand = 0;
            this.player_paycheck = 0;
            this.player_faction = "civillian";

            this.player_vehicles_owned = 0;
            this.player_inventory = new List<ObjectData>();
        }
    }

    public struct AnimData
    {
        public string action_name; //Command action
        public Int32 object_id; //Object id (SET TO -1 IF IT HAS NONE)
        public string bone_index; //Bone index for object (SET TO "null" IF THERE IS NONE)
        public Vector3 position_offset; //Object position offset
        public Vector3 rotation_offset; //Object rotation offset
        public int animation_flag; //Animation flags
        public string anim_dict; //Animation dictionary
        public string anim_name; //Animation name

        //Initializer
        public AnimData(string action, Int32 obj_id, string bone_indx, Vector3 posOff, Vector3 rotOff, int anim_flag, string anim_d, string anim_n)
        {
            action_name = action;
            object_id = obj_id;
            bone_index = bone_indx;
            position_offset = posOff;
            rotation_offset = rotOff;
            animation_flag = anim_flag;
            anim_dict = anim_d;
            anim_name = anim_n;
        }
    }

    public AnimData[] anim_names = new AnimData[]
    {
        new AnimData("clean", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "switch@franklin@cleaning_car", "001946_01_gc_fras_v2_ig_5_base"),
        new AnimData("clipboard1", -969349845, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_clipboard@male@idle_a", "idle_a"),
        new AnimData("clipboard2", -969349845, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_clipboard@male@idle_b", "idle_d"),
        new AnimData("phone1", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@female@idle_a", "idle_a"),
        new AnimData("phone2", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@male@idle_a", "idle_a"),
        new AnimData("phone3", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@female@idle_a", "idle_b"),
        new AnimData("checkbody1", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "amb@medic@standing@kneel@idle_a", "idle_a"),
        new AnimData("checkbody2", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "amb@medic@standing@tendtodead@idle_a", "idle_a"),
        new AnimData("leancar", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "switch@michael@sitting_on_car_bonnet", "sitting_on_car_bonnet_loop"),
        new AnimData("sit", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "switch@michael@sitting", "idle"),
        new AnimData("leanfoot", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a"),
        new AnimData("lean", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_a", "idle_a"),
        new AnimData("handsupknees", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.StopOnLastFrame), "busted", "idle_2_hands_up"),
        new AnimData("handsup", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "missfbi5ig_20b", "hands_up_scientist"),
        new AnimData("smoke1", 175300549, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_aa_smoke@male@idle_a", "idle_a"),
        new AnimData("smoke2", 175300549, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_leaning@female@smoke@idle_a", "idle_a"),
        new AnimData("coffee1", -163314598, "PH_R_Hand", new Vector3(0.0, 0.0, -0.1), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_aa_coffee@idle_a", "idle_a"),
        new AnimData("coffee2", -163314598, "PH_R_Hand", new Vector3(0.0, 0.0, -0.1), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_drinking@coffee@male@idle_a", "idle_a"),
        new AnimData("guitar", -708789241, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_musician@guitar@male@idle_a", "idle_b"),
        new AnimData("drums", 591916419, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_musician@bongos@male@idle_a", "idle_a"),
    };

    public List<ObjectData> object_database = new List<ObjectData>();

    public List<PlayerData> player_database = new List<PlayerData>();
    public List<VehicleData> vehicle_database = new List<VehicleData>();
    public List<Blip> blip_database = new List<Blip>();

    public List<int> RandomIDPlayerPool = new List<int>();
    public List<int> RandomIDVehiclePool = new List<int>();
    public List<int> RandomIDObjectPool = new List<int>();

    public int getPlayerDatabaseIndexByClient(Client player)
    {
        for (int i = 0; i < player_database.Count; i++)
            if (player_database[i].player_client == player)
                return i;
        return -1;
    }

    public int getPlayerDatabaseIndexByDisplayName(string name)
    {
        for (int i = 0; i < player_database.Count; i++)
            if (player_database[i].player_display_name.ToLower() == name)
                return i;
        return -1;
    }

    public bool doesIDPlayerPoolExist(int id)
    {
        for (int i = 0; i < RandomIDPlayerPool.Count; i++)
            if (RandomIDPlayerPool[i] == id)
                return true;
        return false;
    }

    public bool doesIDVehiclePoolExist(int id)
    {
        for (int i = 0; i < RandomIDVehiclePool.Count; i++)
            if (RandomIDVehiclePool[i] == id)
                return true;
        return false;
    }

    public bool doesIDObjectPoolExist(int id)
    {
        for (int i = 0; i < RandomIDObjectPool.Count; i++)
            if (RandomIDObjectPool[i] == id)
                return true;
        return false;
    }

    public int getRandomIDPlayerPool()
    {
        int x = RandomIDPlayerPool[0];
        RandomIDPlayerPool.RemoveAt(0);
        return x;
    }

    public int getRandomIDVehiclePool()
    {
        int x = RandomIDVehiclePool[0];
        RandomIDVehiclePool.RemoveAt(0);
        return x;
    }

    public int getRandomIDObjectPool()
    {
        int x = RandomIDObjectPool[0];
        RandomIDObjectPool.RemoveAt(0);
        return x;
    }


    public void OnResourceStartHandler()
    {
        API.consoleOutput("server_core initialising...");
        for (int i = 0; i < 1000; i++)
        {
            int temp = rnd.Next(1, 100000);
            while (doesIDPlayerPoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDPlayerPool.Add(temp);

            temp = rnd.Next(1, 100000);
            while (doesIDVehiclePoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDVehiclePool.Add(temp);

            temp = rnd.Next(1, 100000);
            while (doesIDObjectPoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDObjectPool.Add(temp);
        }
        API.consoleOutput("server_core initialised.");

        WebClient webClient = new WebClient();
        string IP = webClient.DownloadString("http://api.ipify.org/");
        API.consoleOutput("IP: " + IP);

        PlayerData plr = new PlayerData(null, -1, (PedHash)123, "bigballer", "Test_Mcbutt", "123");
        MongoClient client = new MongoClient();
        var db = client.GetDatabase("PlayerDatabase");
        var collection = db.GetCollection<BsonDocument>("PlayerData");
        var bsonObj = plr.ToBsonDocument();
        collection.InsertOne(bsonObj);

        var filter = new BsonDocument();

        using (var cursor = collection.FindSync<BsonDocument>(filter))
        {
            while(cursor.MoveNext())
            {
                var batch = cursor.Current;
                foreach(var document in batch)
                {
                    var obj = BsonSerializer.Deserialize<PlayerData>(document);
                    API.consoleOutput("BSON OBJ: " + document["player_display_name"].ToString());
                    API.consoleOutput("Actual OBJ: " + obj.player_display_name);
                }
            }
        }

        API.requestIpl("shr_int");
        API.removeIpl("fakeint");

    }

    public int getPlayerCount()
    {
        int online = 0;
        for(int i = 0; i < player_database.Count; i++)
            if (player_database[i].player_online)
                online++;
        return online;
    }

    public string getVehicleName(NetHandle veh)
    {
        if(!veh.IsNull)
        {
            return API.getVehicleDisplayName((VehicleHash)API.getEntityModel(veh)).ToLower();
        }
        else
        {
            return "null";
        }
    }

    public int getVehicleIndexByVehicle(NetHandle veh)
    {
        for(int i = 0; i < vehicle_database.Count; i++)
            if (vehicle_database[i].vehicle_object == veh)
                return i;
        return -1;
    }

    public static bool isStringValid(string s)
    {
        foreach (char c in s)
            if (!Char.IsLetter(c))
                return false;
        return true;
    }

    public static string replaceCharAtIndex(int i, char value, string word)
    {
        char[] letters = word.ToCharArray();
        letters[i] = value;
        return string.Join("", letters);
    }

    //Calculate distance between two vectors
    public float vecdist(Vector3 v1, Vector3 v2)
    {
        Vector3 vf = new Vector3();
        vf.X = v1.X - v2.X;
        vf.Y = v1.Y - v2.Y;
        vf.Z = v1.Z - v2.Z;
        return (float)Math.Sqrt(vf.X * vf.X + vf.Y * vf.Y + vf.Z * vf.Z);
    }

    public double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    //Rotate vector around another vector
    Vector3 rotatedVector(Vector3 src, Vector3 center, double angle, double radius)
    {
        Vector3 res = new Vector3(0.0, 0.0, 0.0);
        res.X = Convert.ToSingle(Math.Cos(DegreeToRadian(angle)) * radius + center.X);
        res.Y = Convert.ToSingle(Math.Sin(DegreeToRadian(angle)) * radius + center.Y);
        res.Z = center.Z;

        return res;
    }

    public void OnPlayerConnectedHandler(Client player)
    {
        API.sendChatMessageToPlayer(player, "~b~raptube69's RP server script");
        API.sendChatMessageToPlayer(player, "~w~Players Online: ~b~" + getPlayerCount());
        API.consoleOutput("Player (" + player.name + ") has connected.");

        bool found_player = false;
        int index = -1;
        for(int i = 0; i < player_database.Count; i++)
        {
            if(player_database[i].player_game_name == player.name)
            {
                found_player = true;
                index = i;
            }
        }

        if(found_player)
        {
            API.consoleOutput("Player (" + player.name + ") exists in database.");
            PlayerData player_data = player_database[index];
            player_data.player_client = player;
            player_data.player_id = getRandomIDPlayerPool();
            player_data.player_online = true;
            player_database[index] = player_data;
            API.sendChatMessageToPlayer(player, "Please ~b~login ~w~using /login [password].");
        }
        else
        {
            API.consoleOutput("Player (" + player.name + ") does not exist in database.");
            PlayerData player_data = new PlayerData(player, getRandomIDPlayerPool(), API.pedNameToModel("Mani"), player.name, "test_mcbutt", "null_password");
            player_database.Add(player_data);
            API.sendChatMessageToPlayer(player, "Please ~b~register ~w~using /register [firstname_lastname] [password]");
        }

        int rnd_location = rnd.Next(0, loginscreen_locations.Length);
        API.setPlayerSkin(player, API.pedNameToModel("Mani"));
        API.setEntityPosition(player, loginscreen_locations[rnd_location]);
        API.setEntityPositionFrozen(player, true);
        API.setEntityTransparency(player, 0);
        API.setEntityInvincible(player, true);
        API.setEntityCollisionless(player, true);
        API.setPlayerNametag(player, "");


        List<NetHandle> vehs = new List<NetHandle>();
        vehs = API.getAllVehicles();

        for(int i = 0; i < vehs.Count; i++)
        {
            if (API.getEntitySyncedData(vehs[i], "indicator_right") != null)
                API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 0, API.getEntitySyncedData(vehs[i], "indicator_right"));
            if (API.getEntitySyncedData(vehs[i], "indicator_left") != null)
                API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 1, API.getEntitySyncedData(vehs[i], "indicator_left"));
            if (API.getEntitySyncedData(vehs[i], "trunk") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 5, vehs[i], API.getEntitySyncedData(vehs[i], "trunk"));
            if (API.getEntitySyncedData(vehs[i], "hood") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 4, vehs[i], API.getEntitySyncedData(vehs[i], "hood"));
            if (API.getEntitySyncedData(vehs[i], "door1") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 0, vehs[i], API.getEntitySyncedData(vehs[i], "door1"));
            if (API.getEntitySyncedData(vehs[i], "door2") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 1, vehs[i], API.getEntitySyncedData(vehs[i], "door2"));
            if (API.getEntitySyncedData(vehs[i], "door3") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 2, vehs[i], API.getEntitySyncedData(vehs[i], "door3"));
            if (API.getEntitySyncedData(vehs[i], "door4") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 3, vehs[i], API.getEntitySyncedData(vehs[i], "door4"));
        }
    }

    public void ExplodeAllTiresShape(ColShape shape, NetHandle entity)
    {
        ExplodeAllTires(entity);
    }

    public void ExplodeAllTires(NetHandle entity)
    {
        if (API.getEntityType(entity) == EntityType.Vehicle)
        {
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_0_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_0_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_0_popped", true);
                    }
                }
            }
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_1_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_1_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 1, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_1_popped", true);
                    }
                }
            }
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_2_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_2_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 2, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_2_popped", true);
                    }
                }
            }
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_3_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_3_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 3, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_3_popped", true);
                    }
                }
            }
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_4_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_4_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 4, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_4_popped", true);
                    }
                }
            }
            if (randomBool())
            {
                if (API.getEntitySyncedData(entity, "tyre_5_popped") != null)
                {
                    if (API.getEntitySyncedData(entity, "tyre_5_popped") == false)
                    {
                        API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 5, true, 1000.0);
                        API.setEntitySyncedData(entity, "tyre_5_popped", true);
                    }
                }
            }
        }
    }

    public void setRepairedTyres(NetHandle entity)
    {
        if(API.getEntityType(entity) == EntityType.Vehicle)
        {
            API.setEntitySyncedData(entity, "tyre_0_popped", false);
            API.setEntitySyncedData(entity, "tyre_1_popped", false);
            API.setEntitySyncedData(entity, "tyre_2_popped", false);
            API.setEntitySyncedData(entity, "tyre_3_popped", false);
            API.setEntitySyncedData(entity, "tyre_4_popped", false);
            API.setEntitySyncedData(entity, "tyre_5_popped", false);
        }
    }


    public void OnPlayerDisconnectedHandler(Client player,string reason)
    {
        API.consoleOutput("Player (" + player.name + ") has disconnected. Reason: " + reason);

        int index = getPlayerDatabaseIndexByClient(player);
        NetHandle temp = new NetHandle();
        if (API.getEntitySyncedData(player, "anim_obj") != null)
            temp = API.getEntitySyncedData(player, "anim_obj");
        API.deleteEntity(temp);

        PlayerData player_data = player_database[index];
        RandomIDPlayerPool.Add(player_data.player_id);
        player_data.player_id = -1;
        player_data.player_client = null;
        player_data.player_online = false;
        player_data.player_logged = false;
        player_data.player_position = API.getEntityPosition(player);
        player_data.player_rotation = API.getEntityRotation(player);
        player_database[index] = player_data;

    }

    public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    public void OnChatCommandHandler(Client player, string command, CancelEventArgs e)
    {
        int index = getPlayerDatabaseIndexByClient(player);
        

        if(command.StartsWith("/login"))
        {
            if(player_database[index].player_registered == false)
            {
                API.sendChatMessageToPlayer(player, "You are not ~b~registered! ~w~Register using /register [firstname_lastname] [password].");
                e.Cancel = true;
            }
            else if(player_database[index].player_logged == true)
            {
                API.sendChatMessageToPlayer(player, "You are already ~b~logged ~w~in!");
                e.Cancel = true;
            }
        }
        else if(command.StartsWith("/register"))
        {
            if(player_database[index].player_registered == true || player_database[index].player_logged == true)
            {
                API.sendChatMessageToPlayer(player, "You are already ~b~registered~w~!");
                e.Cancel = true;
            }
        }
        else
        {
            if (player_database[index].player_registered == false)
            {
                API.sendChatMessageToPlayer(player, "You are not ~b~registered~w~! Register using /register [firstname_lastname] [password].");
                e.Cancel = true;
            }
            else if (player_database[index].player_logged == false)
            {
                API.sendChatMessageToPlayer(player, "You are not ~b~logged ~w~in! Login using /login [password].");
                e.Cancel = true;
            }
        }
    }
    private static readonly object syncLock = new object();
    public static bool randomBool()
    {
        lock (syncLock)
        { // synchronize
            int prob = rnd.Next(100);
            return prob <= 70;
        }
    }

    public int dealership_1_spawn_counter = 0;
    public int dealership_dim = 0;

    public void OnClientEventTriggerHandler(Client player, string eventName, params object[] args)
    {
        if(eventName == "indicator_left")
        {
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left") != null)
                API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, API.getPlayerVehicle(player), 1, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left"));
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left") != null)
                API.setEntitySyncedData(API.getPlayerVehicle(player), "indicator_left", !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left"));
        }
        else if (eventName == "indicator_right")
        {
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right") != null)
                API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, API.getPlayerVehicle(player), 0, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right"));
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right") != null)
                API.setEntitySyncedData(API.getPlayerVehicle(player), "indicator_right", !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right"));
        }
        else if(eventName == "do_anim")
        {
            API.sendChatMessageToPlayer(player, "do_anim_call: " + args[0]);
            animFunc(player, (string)args[0], true);
        }
        else if(eventName == "purchase_car")
        {
            int index = getPlayerDatabaseIndexByClient(player);
            if (player_database[index].player_money_bank - Convert.ToInt64(args[1]) >= 0)
            {
                long prc = Convert.ToInt64(args[1]);
                API.sendChatMessageToPlayer(player, "Purchased ~b~" + args[2] + "~w~ for ~b~$" + prc.ToString("N0") + "~w~!");

                PlayerData temp = player_database[index];
                temp.player_money_bank -= prc;
                player_database[index] = temp;

                if((string)args[3] == "dealership_1")
                {
                    int color_indx = rnd.Next(0, common_car_colors.Length);
                    spawnCar(player, (string)args[0], true, dealership_1_spawn_locations[dealership_1_spawn_counter].pos, dealership_1_spawn_locations[dealership_1_spawn_counter].rot, common_car_colors[color_indx], common_car_colors[color_indx], true, 0);
                    dealership_1_spawn_counter++;
                    if (dealership_1_spawn_counter >= dealership_1_spawn_locations.Length)
                    {
                        dealership_1_spawn_counter = 0;
                    }
                }
                //spawn car for player
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You cannot afford this vehicle!");
            }
        }
        else if(eventName == "explode")
        {
            //API.popVehicleTyre(API.getPlayerVehicle(player), 1, true);
            //API.sendNativeToPlayer(player, GTANetworkServer.Hash.STEER_UNLOCK_BIAS, API.getPlayerVehicle(player), true);
            //API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_STEER_BIAS, API.getPlayerVehicle(player), 0.0);
            ExplodeAllTires(API.getPlayerVehicle(player));
        }
        else if(eventName == "spawn_car_diff_dim")
        {
            dealership_dim++;
            spawnCar(player, (string)args[0], true, (Vector3)args[1], (Vector3)args[2], 0, 0, false, dealership_dim);
        }
        else if(eventName == "delete_car")
        {
            //API.deleteEntity((NetHandle)args[0]);
            NetHandle plr_veh = API.getPlayerVehicle(player);
            API.deleteEntity(plr_veh);

            API.setEntityDimension(player, 0);
            API.setEntityPosition(player, new Vector3(-61.70732, -1093.239, 26));
            dealership_dim--;
        }
    }

    [Command("cef")]
    public void cefTestFunc(Client player)
    {
        API.triggerClientEvent(player, "cef_test");
    }

    [Command("myid")]
    public void getMyID(Client player)
    {
        int index = getPlayerDatabaseIndexByClient(player);
        API.sendChatMessageToPlayer(player, "Your ID is: ~b~" + player_database[index].player_id + ".");
    }

    [Command("mytag")]
    public void getMyTag(Client player)
    {
        int index = getPlayerDatabaseIndexByClient(player);
        API.sendChatMessageToPlayer(player, "Your Tag is: ~b~" + player_database[index].player_display_name + ".");
    }

    [Command("pos")] //debug
    public void getPos(Client player)
    {
        Vector3 vec = API.getEntityPosition(player);
        API.sendChatMessageToPlayer(player, "X: " + vec.X + " Y: " + vec.Y + " Z: " + vec.Z);
    }

    [Command("carpos")]
    public void getCarPos(Client player)
    {
        Vector3 vec = API.getEntityPosition(API.getPlayerVehicle(player));
        API.sendChatMessageToPlayer(player, "X: " + vec.X + " Y: " + vec.Y + " Z: " + vec.Z);
    }

    [Command("carrot")]
    public void getCarRot(Client player)
    {
        Vector3 vec = API.getEntityRotation(API.getPlayerVehicle(player));
        API.sendChatMessageToPlayer(player, "X: " + vec.X + " Y: " + vec.Y + " Z: " + vec.Z);
    }

    [Command("setfaction", GreedyArg = true)]
    public void setFactionCommand(Client player, string fac)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (fac.ToLower() == "police" || fac.ToLower() == "civillian")
        {
            PlayerData temp = player_database[indx];
            temp.player_faction = fac.ToLower();
            player_database[indx] = temp;
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Faction is not valid!");
        }
    }

    [Command("me", GreedyArg = true)]
    public void meCommand(Client player, string msg)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        string msgr = "* " + player_database[indx].player_display_name + " " + msg;
        var players = API.getPlayersInRadiusOfPlayer(30.0f, player);
        foreach (Client c in players)
        {
            API.sendChatMessageToPlayer(c, "~#CC99FF~", msgr);
        }
    }

    [Command("login", "Usage: /login [password].", SensitiveInfo = true, GreedyArg = true)]
    public void loginFunc(Client player, string password)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (password == player_database[indx].player_password)
        {
            API.sendChatMessageToPlayer(player, "Welcome, ~b~" + player_database[indx].player_display_name + "~w~! TEST(" + API.getPlayerNametag(player) + ")");
            //Change player data and log him in
            PlayerData plr_temp = player_database[indx];
            plr_temp.player_logged = true;
            API.setEntityPosition(player, plr_temp.player_position);
            API.setEntityRotation(player, plr_temp.player_rotation);
            API.setEntityPositionFrozen(player, false);
            API.setEntityTransparency(player, 255);
            API.setEntityInvincible(player, false);
            API.setEntityCollisionless(player, false);
            player_database[indx] = plr_temp;

            API.setPlayerNametag(player, plr_temp.player_display_name);

            for (int i = 0; i < store_locations.Length; i++)
            {
                API.sendChatMessageToPlayer(player, "store_location_added");
                API.triggerClientEvent(player, "create_label", store_locations[i].name, store_locations[i].location, store_locations[i].store_type_id);
            }

            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            for (int i = 0; i < vehs.Count; i++)
            {
                if (API.getEntitySyncedData(vehs[i], "indicator_right") != null)
                    API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 0, API.getEntitySyncedData(vehs[i], "indicator_right"));
                if (API.getEntitySyncedData(vehs[i], "indicator_left") != null)
                    API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 1, API.getEntitySyncedData(vehs[i], "indicator_left"));
                if (API.getEntitySyncedData(vehs[i], "trunk") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 5, vehs[i], API.getEntitySyncedData(vehs[i], "trunk"));
                if (API.getEntitySyncedData(vehs[i], "hood") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 4, vehs[i], API.getEntitySyncedData(vehs[i], "hood"));
                if (API.getEntitySyncedData(vehs[i], "door1") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 0, vehs[i], API.getEntitySyncedData(vehs[i], "door1"));
                if (API.getEntitySyncedData(vehs[i], "door2") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 1, vehs[i], API.getEntitySyncedData(vehs[i], "door2"));
                if (API.getEntitySyncedData(vehs[i], "door3") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 2, vehs[i], API.getEntitySyncedData(vehs[i], "door3"));
                if (API.getEntitySyncedData(vehs[i], "door4") != null)
                    API.triggerClientEvent(player, "sync_vehicle_door_state", 3, vehs[i], API.getEntitySyncedData(vehs[i], "door4"));
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Wrong credentials!");
        }
    }

    [Command("register", "Usage: /register [firstname_lastname] [password].", SensitiveInfo = true, GreedyArg = true)]
    public void registerFunc(Client player, string info)
    {
        int indx = getPlayerDatabaseIndexByClient(player);

        char[] delimiter = { ' ' };
        string[] words = info.Split(delimiter);

        string name = "";
        string password = "";

        name = words[0].ToLower();
 
        if (words.Length == 2)
        {
            password = words[1];
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Please format your information correctly. (1)");
            return;
        }

        char[] name_delimiter = { '_' };
        string[] names = name.Split(name_delimiter);

        string firstname = names[0];
        if (!isStringValid(firstname))
        {
            API.sendChatMessageToPlayer(player, "Please format your name correctly. (2)");
            return;
        }

        string lastname = "";
        if(names.Length == 2)
        {
            lastname = names[1].ToLower();
            if(!isStringValid(lastname))
            {
                API.sendChatMessageToPlayer(player, "Please format your name correctly. (3)");
                return;
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Please format your name correctly. (4)");
            return;
        }


        firstname = replaceCharAtIndex(0, Char.ToUpper(firstname[0]), firstname);
        lastname = replaceCharAtIndex(0, Char.ToUpper(lastname[0]), lastname);

        PlayerData plr_temp = player_database[indx];
        plr_temp.player_password = password;
        plr_temp.player_display_name = firstname + "_" + lastname;
        plr_temp.player_registered = true;
        plr_temp.player_position = new Vector3(-1034.600, -2733.600, 13.800);
        plr_temp.player_rotation = new Vector3(0.0, 0.0, 0.0);
        player_database[indx] = plr_temp;
        API.sendChatMessageToPlayer(player, "You have been registered! Use /login ~b~(password) ~w~to login.");
    }

    [Command("logout")]
    public void logoutFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        
        //Stop player animation and delete objects
        API.stopPlayerAnimation(player);
        NetHandle temp = new NetHandle();
        if (API.getEntitySyncedData(player, "anim_obj") != null)
            temp = API.getEntitySyncedData(player, "anim_obj");
        API.deleteEntity(temp);

        API.sendChatMessageToPlayer(player, "You have been logged out!");
        PlayerData plr_temp = player_database[indx];
        plr_temp.player_position = API.getEntityPosition(player);
        plr_temp.player_rotation = API.getEntityRotation(player);
        plr_temp.player_logged = false;
        player_database[indx] = plr_temp;


        Random rnd = new Random();
        int temprnd = rnd.Next(0, loginscreen_locations.Length);
        API.setEntityPosition(player, loginscreen_locations[temprnd]);
        API.setEntityPositionFrozen(player, true);
        API.setEntityTransparency(player, 0);
        API.setEntityInvincible(player, true);
        API.setEntityCollisionless(player, true);
        API.setPlayerNametag(player, "");
        API.triggerClientEvent(player, "delete_all_labels");
    }

    [Command("spawncar", GreedyArg = true)]
    public void spawnCarFunc(Client player, string carname)
    {
        spawnCar(player, carname, false, new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), 0, 0, true, 0);
    }

    public void spawnCar(Client player, string carname, bool forcePos, Vector3 pos, Vector3 rot, int color1, int color2, bool owned, int dim)
    {
        if(owned)
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            API.sendChatMessageToPlayer(player, "Spawned car!");
            Vehicle hash;
            if (forcePos)
                hash = API.createVehicle(API.vehicleNameToModel(carname), pos, rot, 0, 0, dim);
            else
                hash = API.createVehicle(API.vehicleNameToModel(carname), API.getEntityPosition(player), API.getEntityRotation(player), 0, 0, dim);
            //API.setVehicleNumberPlate(hash, "TJM");
            VehicleData temp = new VehicleData(hash, getRandomIDVehiclePool(), carname, API.getEntityPosition(hash), API.getEntityRotation(hash), "tjm000", player_database[indx].player_display_name, "civillian");
            API.setEntitySyncedData(temp.vehicle_object, "id", (int)temp.vehicle_id);
            API.setEntitySyncedData(temp.vehicle_object, "owner", (string)temp.vehicle_owner);
            API.setEntitySyncedData(temp.vehicle_object, "plate", (string)temp.vehicle_license);
            API.setEntitySyncedData(temp.vehicle_object, "engine", false);
            API.setEntitySyncedData(temp.vehicle_object, "indicator_right", false);
            API.setEntitySyncedData(temp.vehicle_object, "indicator_left", false);
            API.setEntitySyncedData(temp.vehicle_object, "locked", true);
            API.setEntitySyncedData(temp.vehicle_object, "trunk", false);
            API.setEntitySyncedData(temp.vehicle_object, "hood", false);
            API.setEntitySyncedData(temp.vehicle_object, "door1", false);
            API.setEntitySyncedData(temp.vehicle_object, "door2", false);
            API.setEntitySyncedData(temp.vehicle_object, "door3", false);
            API.setEntitySyncedData(temp.vehicle_object, "door4", false);
            API.setEntitySyncedData(temp.vehicle_object, "attached", false);
            API.setVehicleNumberPlate(temp.vehicle_object, (string)temp.vehicle_license);
            API.setVehicleEngineStatus(temp.vehicle_object, false);
            API.setVehicleLocked(temp.vehicle_object, true);
            API.setVehiclePrimaryColor(temp.vehicle_object, color1);
            API.setVehicleSecondaryColor(temp.vehicle_object, color2);
            vehicle_database.Add(temp);
            PlayerData plr = player_database[indx];
            plr.player_vehicles_owned++;
            player_database[indx] = plr;
            API.setPlayerIntoVehicle(player, hash, -1);
        }
        else
        {
            Vehicle hash = API.createVehicle(API.vehicleNameToModel(carname), pos, rot, 0, 0, dim);
            API.setEntityDimension(player, dim);
            API.setPlayerIntoVehicle(player, hash, -1);
            API.setEntityPositionFrozen(hash, true);
            //API.setEntityDimension(hash, dim);
            //API.setEntityDimension(player, dim);
            //API.triggerClientEvent(player, "track_dim_car", hash);
        }
    }

    [Command("stats")]
    public void statsFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        API.sendChatMessageToPlayer(player, "~b~|-------STATS-------|");
        API.sendChatMessageToPlayer(player, "ID: ~b~" + player_database[indx].player_id);
        API.sendChatMessageToPlayer(player, "Full Name: ~b~" + player_database[indx].player_display_name + "   |   ~w~Faction: ~b~" + player_database[indx].player_faction);
        //API.sendChatMessageToPlayer(player, "Phone #: ~b~" + player_database[indx].phone_number);
        API.sendChatMessageToPlayer(player, "Money (Bank): ~b~$" + player_database[indx].player_money_bank.ToString("N0") + "  |  ~w~Money (Hand): ~b~$" + player_database[indx].player_money_hand.ToString("N0"));

        API.sendChatMessageToPlayer(player, "Vehicle(s) Owned: ~b~" + player_database[indx].player_vehicles_owned);
        API.sendChatMessageToPlayer(player, "~b~|-------------------|");
    }

    [Command("engine")]
    public void engineFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            API.sendChatMessageToPlayer(player, "You are not in a vehicle!");
        }
        else
        {
            if (API.getPlayerVehicleSeat(player) != -1)
            {
                API.sendChatMessageToPlayer(player, "You are not the driver!");
            }
            else
            {
                for(int i = 0; i < vehicle_database.Count; i++)
                {
                    if(vehicle_database[i].vehicle_object == API.getPlayerVehicle(player))
                    {
                        if (vehicle_database[i].vehicle_owner == player_database[indx].player_display_name)
                        {
                            API.setVehicleEngineStatus(API.getPlayerVehicle(player), !API.getVehicleEngineStatus(API.getPlayerVehicle(player))); //Just inverse the engine state
                            if (API.getVehicleEngineStatus(API.getPlayerVehicle(player)) == true)
                            {
                                meCommand(player, "has turned the engine on.");
                                VehicleData temp = vehicle_database[i];
                                API.setEntitySyncedData(temp.vehicle_object, "engine", true);
                                temp.vehicle_engine = true;
                                vehicle_database[i] = temp;
                                
                            }
                            else
                            {
                                meCommand(player, "has turned the engine off.");
                                VehicleData temp = vehicle_database[i];
                                API.setEntitySyncedData(temp.vehicle_object, "engine", false);
                                temp.vehicle_engine = false;
                                vehicle_database[i] = temp;
                            }
                        }
                        else
                            API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
                        break;
                    }
                }
            }
        }
    }

    [Command("close", "Usage: /close ~b~(car part)", GreedyArg = true)]
    public void doorCloseFunc(Client player, string action)
    {
        List<NetHandle> vehs = new List<NetHandle>();
        vehs = API.getAllVehicles();
        float smallestDist = 100.0f;
        NetHandle closestveh = new NetHandle();
        bool found = false;
        for (int i = 0; i < vehs.Count; i++)
        {
            float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
            if (vr < smallestDist)
            {
                smallestDist = vr;
                closestveh = vehs[i];
                found = true;
            }
        }

        if (found)
        {
            if (smallestDist < 3.5f)
            {
                //Can be done regardless if car is locked or not
                if (action == "trunk")
                {
                    API.setVehicleDoorState(closestveh, 5, false);
                    API.setEntitySyncedData(closestveh, "trunk", false);
                }
                else if (action == "hood")
                {
                    API.setVehicleDoorState(closestveh, 4, false);
                    API.setEntitySyncedData(closestveh, "hood", false);
                }
                else if (action == "door1")
                {
                    API.setVehicleDoorState(closestveh, 0, false);
                    API.setEntitySyncedData(closestveh, "door1", false);
                }
                else if (action == "door2")
                {
                    API.setVehicleDoorState(closestveh, 1, false);
                    API.setEntitySyncedData(closestveh, "door2", false);
                }
                else if (action == "door3")
                {
                    API.setVehicleDoorState(closestveh, 2, false);
                    API.setEntitySyncedData(closestveh, "door3", false);
                }
                else if (action == "door4")
                {
                    API.setVehicleDoorState(closestveh, 3, false);
                    API.setEntitySyncedData(closestveh, "door4", false);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "Part does not exist!");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "No car found nearby.");
        }
    }

    [Command("open", "Usage: /open ~b~(car part)", GreedyArg = true)]
    public void doorOpenFunc(Client player, string action)
    {
        List<NetHandle> vehs = new List<NetHandle>();
        vehs = API.getAllVehicles();
        float smallestDist = 100.0f;
        NetHandle closestveh = new NetHandle();
        bool found = false;
        for (int i = 0; i < vehs.Count; i++)
        {
            float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
            if (vr < smallestDist)
            {
                smallestDist = vr;
                closestveh = vehs[i];
                found = true;
            }
        }

        if (found)
        {
            if (smallestDist < 3.5f)
            {
                //if (getOwnerNameByVehicleID(API.getEntitySyncedData(closestveh, "id")) == PlayerDatabase[indx].player_fake_name)
                //{
                if (API.getEntitySyncedData(closestveh, "locked") == true)
                {
                    API.sendChatMessageToPlayer(player, "This vehicle is locked!");
                }
                else
                {
                    if (action == "trunk")
                    {
                        API.setVehicleDoorState(closestveh, 5, true);
                        API.setEntitySyncedData(closestveh, "trunk", true);
                    }
                    else if (action == "hood")
                    {
                        API.setVehicleDoorState(closestveh, 4, true);
                        API.setEntitySyncedData(closestveh, "hood", true);
                    }
                    else if (action == "door1")
                    {
                        API.setVehicleDoorState(closestveh, 0, true);
                        API.setEntitySyncedData(closestveh, "door1", true);
                    }
                    else if (action == "door2")
                    {
                        API.setVehicleDoorState(closestveh, 1, true);
                        API.setEntitySyncedData(closestveh, "door2", true);
                    }
                    else if (action == "door3")
                    {
                        API.setVehicleDoorState(closestveh, 2, true);
                        API.setEntitySyncedData(closestveh, "door3", true);
                    }
                    else if (action == "door4")
                    {
                        API.setVehicleDoorState(closestveh, 3, true);
                        API.setEntitySyncedData(closestveh, "door4", true);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "Part does not exist!");
                    }
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "No car found nearby.");
        }
    }

    [Command("unlock")]
    public void unlockFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        List<NetHandle> vehs = new List<NetHandle>();
        vehs = API.getAllVehicles();
        float smallestDist = 100.0f;
        NetHandle closestveh = new NetHandle();
        bool found = false;
        for (int i = 0; i < vehs.Count; i++)
        {
            float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
            if (vr < smallestDist)
            {
                if (API.getEntitySyncedData(vehs[i], "attached") != null)
                {
                    if (API.getEntitySyncedData(vehs[i], "attached") == false)
                    {
                        smallestDist = vr;
                        closestveh = vehs[i];
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            if (smallestDist < 3.0f)
            {
                for (int i = 0; i < vehicle_database.Count; i++)
                {
                    if (vehicle_database[i].vehicle_object == closestveh)
                    {
                        if (vehicle_database[i].vehicle_owner == player_database[indx].player_display_name)
                        {
                            API.setVehicleLocked(closestveh, false);
                            VehicleData temp = vehicle_database[i];
                            temp.vehicle_locked = false;
                            vehicle_database[i] = temp;
                            meCommand(player, "has unlocked the vehicle.");
                            API.setEntitySyncedData(closestveh, "locked", false);
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
                        }
                        break;
                    }
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "No car found nearby.");
        }
    }

    [Command("lock")]
    public void lockFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        List<NetHandle> vehs = new List<NetHandle>();
        vehs = API.getAllVehicles();
        float smallestDist = 100.0f;
        NetHandle closestveh = new NetHandle();
        bool found = false;
        for (int i = 0; i < vehs.Count; i++)
        {
            float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player)); //Get distance between car and player
            if (vr < smallestDist)
            {
                if(API.getEntitySyncedData(vehs[i], "attached") != null)
                {
                    if(API.getEntitySyncedData(vehs[i], "attached") == false)
                    {
                        smallestDist = vr;
                        closestveh = vehs[i];
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            if (smallestDist < 3.0f)
            {
                for (int i = 0; i < vehicle_database.Count; i++)
                {
                    if (vehicle_database[i].vehicle_object == closestveh)
                    {
                        if (vehicle_database[i].vehicle_owner == player_database[indx].player_display_name)
                        {
                            API.setVehicleLocked(closestveh, true);
                            VehicleData temp = vehicle_database[i];
                            temp.vehicle_locked = true;
                            vehicle_database[i] = temp;
                            meCommand(player, "has locked the vehicle.");
                            API.setEntitySyncedData(closestveh, "locked", true);
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
                        }
                        break;
                    }
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "No car found nearby.");
        }
    }

    [Command("seatbelt")]
    public void seatbeltFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (API.isPlayerInAnyVehicle(player))
        {
            API.setPlayerSeatbelt(player, !API.getPlayerSeatbelt(player));
            if (API.getPlayerSeatbelt(player))
            {
                meCommand(player, " put on their seatbelt.");
            }
            else
            {
                meCommand(player, " took off their seatbelt.");
            }
        }
    }

    [Command("anim", "Usage: /anim ~b~(name)", GreedyArg = true)]
    public void animFunc(Client player, string action, bool loop = true)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            if (action == "stop") //stop animation
            {
                API.stopPlayerAnimation(player);
                NetHandle temp = new NetHandle();
                if (API.getEntitySyncedData(player, "anim_obj") != null)
                    temp = API.getEntitySyncedData(player, "anim_obj");
                API.deleteEntity(temp);
            }
            else if (action == "help")
            {
                API.triggerClientEvent(player, "anim_list"); //Send trigger to client to display animation list
            }
            else
            {
                for (int i = 0; i < anim_names.Length; i++)
                {
                    if (anim_names[i].action_name == action)
                    {
                        API.stopPlayerAnimation(player);
                        NetHandle temp = new NetHandle();
                        if (API.getEntitySyncedData(player, "anim_obj") != null)
                            temp = API.getEntitySyncedData(player, "anim_obj");
                        API.deleteEntity(temp);
                        if (anim_names[i].object_id != -1)
                        {
                            API.setEntitySyncedData(player, "anim_obj", API.createObject(anim_names[i].object_id, API.getEntityPosition(player), API.getEntityRotation(player)));
                            API.attachEntityToEntity(API.getEntitySyncedData(player, "anim_obj"), player, anim_names[i].bone_index, anim_names[i].position_offset, anim_names[i].rotation_offset);
                        }
                        if(loop)
                            API.playPlayerAnimation(player, anim_names[i].animation_flag, anim_names[i].anim_dict, anim_names[i].anim_name);
                        else
                            API.playPlayerAnimation(player, 0, anim_names[i].anim_dict, anim_names[i].anim_name);

                        break;
                    }
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }
    }


    

    [Command("attachcar")]
    public void attachCarCommand(Client player)
    {
        NetHandle player_veh = API.getPlayerVehicle(player);
        if(getVehicleName(player_veh) == "flatbed")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            float smallestDist = 100.0f;
            NetHandle closestveh = new NetHandle();
            bool found = false;
            for (int i = 0; i < vehs.Count; i++)
            {
                if (vehs[i] != player_veh && vehicle_database[getVehicleIndexByVehicle(closestveh)].vehicle_owner == player_database[indx].player_display_name)
                {
                    float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player_veh)); //Get distance between car and player
                    if (vr < smallestDist)
                    {
                        smallestDist = vr;
                        closestveh = vehs[i];
                        found = true;
                    }
                }
            }

            if (found) //Found SOME car
            {
                if (smallestDist < 10.0f) //Close enough?
                {
                    /*NetHandle obj = API.createObject(-1036807324, API.getEntityPosition(player), API.getEntityRotation(player));
                    API.attachEntityToEntity(obj, closestveh, "wheel_lf", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                    Vector3 pos = API.getEntityPosition(obj);
                    API.sendChatMessageToPlayer(player, "X: " + pos.X + " Y: " + pos.Y + " Z: " + pos.Z);

                    NetHandle obj2 = API.createObject(-1036807324, API.getEntityPosition(player), API.getEntityRotation(player));
                    API.attachEntityToEntity(obj2, closestveh, "roof", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                    Vector3 pos2 = API.getEntityPosition(obj2);
                    API.sendChatMessageToPlayer(player, "X: " + pos2.X + " Y: " + pos2.Y + " Z: " + pos2.Z);*/

                    float whatever = API.fetchNativeFromPlayer<float>(player, GTANetworkServer.Hash.GET_ENTITY_HEIGHT_ABOVE_GROUND, closestveh);
                    API.sendChatMessageToPlayer(player, "height: " + whatever);
                    API.attachEntityToEntity(closestveh, player_veh, "bodyshell", new Vector3(0.0, -2.0, Convert.ToDouble(whatever) + 0.425), new Vector3(0.0, 0.0, 0.0));
                    API.sendChatMessageToPlayer(player, "Car has been attached.");
                    API.setEntitySyncedData(closestveh, "attached", true);
                    API.setEntitySyncedData(player_veh, "attachee", closestveh);

                    API.sendChatMessageToPlayer(player, getVehicleName(player_veh));

                    //API.triggerClientEvent(player, "trythis", closestveh, player_veh);
                    /*double height = Math.Abs(bone1position - bone2position);
                    API.sendChatMessageToPlayer(player, "b1z: " + bone1position);
                    API.sendChatMessageToPlayer(player, "b2z:" + bone2position);
                    API.sendChatMessageToPlayer(player, "height: " + height);
                    API.attachEntityToEntity(vehicle_database[closestveh].vehicle_object, vehicle_database[player_veh].vehicle_object, "bodyshell", new Vector3(0.0, -2.0, height), new Vector3(0.0, 0.0, 0.0));
                    API.setEntitySyncedData(vehicle_database[player_veh].vehicle_object, "attached", closestveh);
                    API.setEntityCollisionless(vehicle_database[closestveh].vehicle_object, false);*/

                }
                else
                {
                    API.sendChatMessageToPlayer(player, "No suitable car found nearby.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No suitable car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot attach cars to this vehicle!");
        }
    }

    [Command("detach")]
    public void detachCarCommand(Client player)
    {
        if(API.isPlayerInAnyVehicle(player))
        {
            if(getVehicleName(API.getPlayerVehicle(player)) == "flatbed")
            {
                if (API.getEntitySyncedData(API.getPlayerVehicle(player), "attachee") != null)
                    API.detachEntity(API.getEntitySyncedData(API.getPlayerVehicle(player), "attachee"));
                if (API.getEntitySyncedData(API.getEntitySyncedData(API.getPlayerVehicle(player), "attachee"), "attached") != null)
                    API.setEntitySyncedData(API.getEntitySyncedData(API.getPlayerVehicle(player), "attachee"), "attached", false);
                API.sendChatMessageToPlayer(player, "Car has been detached.");
            }
        }
    }

    [Command("license", "Usage: /license ~b~(plate #)", GreedyArg = true)]
    public void carinfoFunc(Client player, string license)
    {
        //API.triggerClientEvent(player, "vehicle_draw_text", temp.vehicle_hash, temp.vehicle_id, temp.owner_name, temp.license_plate);
        license = license.ToUpper(); //Get lowercase string for lazy people
        int indx = getPlayerDatabaseIndexByClient(player);
        if (player_database[indx].player_faction == "police")
        {
            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            bool found = false;
            for (int i = 0; i < vehs.Count; i++)
            {
                if (API.getVehicleNumberPlate(vehs[i]).ToLower() == license)
                {
                    //Display data about car
                    API.sendChatMessageToPlayer(player, "Vehicle is ~g~registered ~w~in database!");
                    var model = API.getEntityModel(vehs[i]);
                    int carindx = getVehicleIndexByVehicle(vehs[i]);
                    string plr_nm = vehicle_database[carindx].vehicle_owner;
                    API.sendChatMessageToPlayer(player, "Model: ~b~" + getVehicleName(vehs[i]) + " ~w~-~b~|~w~- Color: ~b~" + vehicle_database[carindx].vehicle_color);
                    API.sendChatMessageToPlayer(player, "License Plate: ~b~" + license);
                    API.sendChatMessageToPlayer(player, "Owner: ~b~" + plr_nm);
                    found = true;
                }
            }
            if (found == false)
            {
                //BUSTED
                API.sendChatMessageToPlayer(player, "Vehicle is not ~r~registered ~w~in database!");
            }
        }
    }

    [Command("inventory")]
    public void inventoryFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        API.sendChatMessageToPlayer(player, "~y~--Inventory--");
        if (player_database[indx].player_inventory.Count == 0)
        {
            API.sendChatMessageToPlayer(player, "~y~[EMPTY]");
        }
        else
        {
            for (int i = 0; i < player_database[indx].player_inventory.Count; i++)
            {
                API.sendChatMessageToPlayer(player, "~y~" + player_database[indx].player_inventory[i].name);
            }
        }
    }

    [Command("check", "Usage: /check ~b~(car part)", GreedyArg = true)]
    public void checkFunc(Client player, string action)
    {
        action = action.ToLower();
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            float smallestDist = 100.0f;
            NetHandle closestveh = new NetHandle();
            bool found = false;
            for (int i = 0; i < vehs.Count; i++)
            {
                float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
                if (vr < smallestDist)
                {
                    smallestDist = vr;
                    closestveh = vehs[i];
                    found = true;
                }
            }

            if (found)
            {
                if (smallestDist < 3.5f)
                {
                    if (action == "trunk")
                    {
                        if (API.getVehicleDoorState(closestveh, 5) == true || API.isVehicleDoorBroken(closestveh, 5) == true)
                        {
                            API.sendChatMessageToPlayer(player, "~y~--Trunk Inventory--");
                            VehicleData temp_vehdata = vehicle_database[getVehicleIndexByVehicle(closestveh)];
                            if (temp_vehdata.vehicle_inventory.Count == 0)
                            {
                                API.sendChatMessageToPlayer(player, "~y~[EMPTY]");
                            }
                            else
                            {
                                for (int i = 0; i < temp_vehdata.vehicle_inventory.Count; i++)
                                {
                                    API.sendChatMessageToPlayer(player, "~y~" + temp_vehdata.vehicle_inventory[i].name);
                                }
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Car trunk is closed!");
                        }
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "No car found nearby.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "No car found nearby.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("put", "Usage: /put ~b~(object name) (car part)", GreedyArg = true)]
    public void putFunc(Client player, string action)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            char[] delimiter = { ' ' };
            string[] words = action.Split(delimiter);

            string item_name;
            string loc_name = "";

            item_name = words[0];
            if (words.Length > 1)
            {
                loc_name = words[1];
                loc_name = loc_name.ToLower();
            }

            if (loc_name == "trunk")
            {
                List<NetHandle> vehs = new List<NetHandle>();
                vehs = API.getAllVehicles();
                float smallestDist = 100.0f;
                NetHandle closestveh = new NetHandle();
                bool found = false;
                for (int i = 0; i < vehs.Count; i++)
                {
                    float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
                    if (vr < smallestDist)
                    {
                        smallestDist = vr;
                        closestveh = vehs[i];
                        found = true;
                    }
                }

                if (found)
                {
                    if (smallestDist < 3.5f)
                    {
                        if (API.getVehicleDoorState(closestveh, 5) == true || API.isVehicleDoorBroken(closestveh, 5) == true)
                        {
                            for (int i = 0; i < player_database[indx].player_inventory.Count; i++)
                            {
                                if (player_database[indx].player_inventory[i].name == item_name)
                                {
                                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                                    animFunc(player, "clean", false);
                                    VehicleData temp_vehdata = vehicle_database[getVehicleIndexByVehicle(closestveh)];
                                    ObjectData temp = player_database[indx].player_inventory[i];
                                    //temp.obj = API.createObject(temp.obj_id, API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player));
                                    //API.setEntityPosition(temp.obj, rotatedVector(API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player).Z + 90.0));
                                    //API.consoleOutput("Player Z ROT: " + API.getEntityRotation(player).Z);
                                    //API.setEntitySyncedData(temp.obj, "del", true);
                                    //API.setEntityPositionFrozen(temp.obj, true);
                                    //API.setEntityCollisionless(temp.obj, true);
                                    //worldObjectPool.Add(temp);


                                    //boot
                                    //API.attachEntityToEntity(temp.obj, temp_vehdata.vehicle_hash, loc_name, new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                                    vehicle_database[getVehicleIndexByVehicle(closestveh)].vehicle_inventory.Add(temp);
                                    player_database[indx].player_inventory.RemoveAt(i);
                                    API.sendChatMessageToPlayer(player, "Placed in trunk: " + temp.id);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Car trunk is closed!");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "No car found nearby.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "No car found nearby.");
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("take", "Usage: /take ~b~(object name) (car part)", GreedyArg = true)]
    public void takeFunc(Client player, string action)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            char[] delimiter = { ' ' };
            string[] words = action.Split(delimiter);

            string item_name;
            string loc_name = "";

            item_name = words[0];
            if (words.Length > 1)
            {
                loc_name = words[1];
                loc_name = loc_name.ToLower();
            }


            if (loc_name == "trunk")
            {
                List<NetHandle> vehs = new List<NetHandle>();
                vehs = API.getAllVehicles();
                float smallestDist = 100.0f;
                NetHandle closestveh = new NetHandle();
                bool found = false;
                for (int i = 0; i < vehs.Count; i++)
                {
                    float vr = vecdist(API.getEntityPosition(vehs[i]), API.getEntityPosition(player));
                    if (vr < smallestDist)
                    {
                        smallestDist = vr;
                        closestveh = vehs[i];
                        found = true;
                    }
                }

                if (found)
                {
                    if (smallestDist < 3.5f)
                    {
                        if (API.getVehicleDoorState(closestveh, 5) == true)
                        {
                            VehicleData temp_vehdata = vehicle_database[getVehicleIndexByVehicle(closestveh)];
                            for (int i = 0; i < temp_vehdata.vehicle_inventory.Count; i++)
                            {
                                if (temp_vehdata.vehicle_inventory[i].name == item_name)
                                {
                                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                                    animFunc(player, "clean", false);
                                    //randevouz
                                    ObjectData temp = temp_vehdata.vehicle_inventory[i];
                                    //temp.obj = API.createObject(temp.obj_id, API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player));
                                    //API.setEntityPosition(temp.obj, rotatedVector(API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player).Z + 90.0));
                                    //API.consoleOutput("Player Z ROT: " + API.getEntityRotation(player).Z);
                                    //API.setEntitySyncedData(temp.obj, "del", true);
                                    //API.setEntityPositionFrozen(temp.obj, true);
                                    //API.setEntityCollisionless(temp.obj, true);
                                    //worldObjectPool.Add(temp);
                                    /*API.deleteEntity(temp.obj);
                                    int x = 0;
                                    for(; x < worldObjectPool.Count; x++)
                                    {
                                        if(worldObjectPool[x].id == temp.id)
                                        {
                                            worldObjectPool.RemoveAt(x);
                                            break;
                                        }
                                    }*/
                                    player_database[indx].player_inventory.Add(temp);
                                    vehicle_database[getVehicleIndexByVehicle(closestveh)].vehicle_inventory.RemoveAt(i);
                                    //PlayerDatabase[indx].inventory.RemoveAt(i);
                                    API.sendChatMessageToPlayer(player, "Took from trunk: " + temp.id);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Car trunk is closed!");
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "No car found nearby.");
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "No car found nearby.");
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("addmoney", "Usage: /addmoney [amount] [firstname_lastname]", GreedyArg = true)]
    public void addMoneyFunc(Client player, string name_amount)
    {
        char[] delimiter = { ' ' };
        string[] words = name_amount.Split(delimiter);

        long amount;
        string name = "";

        amount = Convert.ToInt64(words[0]);
        if(words.Length == 2)
        {
            name = words[1];
            name = name.ToLower();
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Incorrect format.");
            return;
        }

        int indx = getPlayerDatabaseIndexByDisplayName(name);
        if (indx != -1)
        {
            PlayerData plr_temp = player_database[indx];
            plr_temp.player_money_bank += amount;
            player_database[indx] = plr_temp;
            API.sendChatMessageToPlayer(player, "Money added: " + amount.ToString("N0"));
        }
    }

    [Command("teleport", GreedyArg = true)]
    public void teleportFunc(Client player, string coords)
    {
        char[] delimiter = { ' ' };

        string[] words = coords.Split(delimiter);

        double x;
        double y;
        double z;

        x = Convert.ToDouble(words[0]);
        y = Convert.ToDouble(words[1]);
        z = Convert.ToDouble(words[2]);

        Vector3 telepos = new Vector3(x, y, z);
        API.setEntityPosition(player, telepos);
    }

    [Command("additem", "Usage: /additem [id] [name]", GreedyArg = true)]
    public void addItemFunc(Client player, string id)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        char[] delimiter = { ' ' };
        string[] words = id.Split(delimiter);

        string id_new;
        string name_new = "";

        id_new = words[0];
        if(words.Length == 2)
        {
            name_new = words[1];
            name_new = name_new.ToLower();
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Incorrect format.");
        }

        ObjectData temp = new ObjectData();
        temp.id = getRandomIDObjectPool();
        temp.name = name_new;
        temp.obj_id = Convert.ToInt32(id_new);
        temp.is_static = false;
        player_database[indx].player_inventory.Add(temp);
        API.sendChatMessageToPlayer(player, "Object added to inventory: " + temp.id);
    }

    [Command("removeitem", "Usage: /removeitem [name]", GreedyArg = true)]
    public void removeItemFunc(Client player, string item)
    {
        item = item.ToLower();
        int indx = getPlayerDatabaseIndexByClient(player);
        for (int i = 0; i < player_database[indx].player_inventory.Count; i++)
        {
            if (player_database[indx].player_inventory[i].name == item)
            {
                RandomIDObjectPool.Add(player_database[indx].player_inventory[i].id);
                API.sendChatMessageToPlayer(player, "Destroyed object: " + player_database[indx].player_inventory[i].id);
                player_database[indx].player_inventory.RemoveAt(i);
                break;
            }
        }
    }

    [Command("place", "Usage: /place ~b~(object name)", GreedyArg = true)]
    public void spawnItemFunc(Client player, string item)
    {
        item = item.ToLower();
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            for (int i = 0; i < player_database[indx].player_inventory.Count; i++)
            {
                if (player_database[indx].player_inventory[i].name == item)
                {
                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                    animFunc(player, "checkbody2", false);
                    //int tempid = RandomIDObjectPool[0];
                    // RandomIDObjectPool.RemoveAt(0);
                    ObjectData temp = player_database[indx].player_inventory[i];
                    temp.obj = API.createObject(temp.obj_id, API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player));
                    API.setEntityPosition(temp.obj, rotatedVector(API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player).Z + 90.0, 0.75));
                    API.consoleOutput("Player Z ROT: " + API.getEntityRotation(player).Z);
                    API.setEntityPositionFrozen(temp.obj, true);
                    API.setEntityCollisionless(temp.obj, true);
                    player_database[indx].player_inventory.RemoveAt(i);

                    if (temp.name == "spike")
                    {

                        //Vector3 pos = rotatedVector(API.getEntityPosition(temp.obj), API.getEntityPosition(temp.obj), API.getEntityRotation(temp.obj).Z + 270.0, 4.75); //left
                        //Vector3 pos2 = rotatedVector(API.getEntityPosition(temp.obj), API.getEntityPosition(temp.obj), API.getEntityRotation(temp.obj).Z + 270, -1.0); //right
                        Vector3 pos3 = rotatedVector(API.getEntityPosition(temp.obj), API.getEntityPosition(temp.obj), API.getEntityRotation(temp.obj).Z + 270.0, 1.75); //midle
                        //NetHandle obj = API.createObject(-1036807324, pos, new Vector3(0.0, 0.0, 0.0));
                        //NetHandle obj2 = API.createObject(-1036807324, pos2, new Vector3(0.0, 0.0, 0.0));
                        NetHandle obj3 = API.createObject(-1036807324, pos3, new Vector3(0.0, 0.0, 0.0));
                        
                        //API.setEntityCollisionless(obj, true);
                        //API.setEntityPositionFrozen(obj, true);
                        //API.setEntityCollisionless(obj2, true);
                        //API.setEntityPositionFrozen(obj2, true);
                        API.setEntityCollisionless(obj3, true);
                        API.setEntityPositionFrozen(obj3, true);

                        //temp.coll = API.createSphereColShape(pos, 4.0f);
                        temp.coll_shape = API.createSphereColShape(pos3, 2.0f);
                        temp.coll_shape.onEntityEnterColShape += ExplodeAllTiresShape;
                    }
                    object_database.Add(temp);
                    API.sendChatMessageToPlayer(player, "Placed down object: " + temp.id);
                    break;
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("pickup")]
    public void deleteItemFunc(Client player)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        if (!API.isPlayerInAnyVehicle(player))
        {
            float smallestDist = 100.0f;
            ObjectData closestObj = new ObjectData();
            int obj_indx = 0;
            bool found = false;
            for (int i = 0; i < object_database.Count; i++)
            {
                float vr = vecdist(API.getEntityPosition(object_database[i].obj), API.getEntityPosition(player));
                if (vr < smallestDist)
                {
                    if(object_database[i].is_static == false)
                    {
                        smallestDist = vr;
                        closestObj = object_database[i];
                        obj_indx = i;
                        //worldObjectPool.RemoveAt(i);
                        found = true;
                    }
                }
            }

            if (found)
            {
                if (smallestDist < 2.5f)
                {
                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                    animFunc(player, "checkbody2", false);
                    if(object_database[obj_indx].coll_shape != null)
                    {
                        API.deleteColShape(object_database[obj_indx].coll_shape);
                    }
                    object_database.RemoveAt(obj_indx);
                    player_database[indx].player_inventory.Add(closestObj);
                    API.deleteEntity(closestObj.obj);
                    API.sendChatMessageToPlayer(player, "Picked up object: " + closestObj.id);
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("catalog")]
    public void purchaseFunc(Client player)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist)
                {
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
                API.triggerClientEvent(player, "catalog_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "There is no store nearby!");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }

    }

    [Command("repair")]
    public void repairFunc(Client player)
    {
        if(API.isPlayerInAnyVehicle(player))
        {
            API.repairVehicle(API.getPlayerVehicle(player));
            setRepairedTyres(API.getPlayerVehicle(player));
        }
    }

    [Command("setcolor", GreedyArg = true)]
    public void setColorFunc(Client player, string colorname)
    {
        if(API.isPlayerInAnyVehicle(player))
        {
            for (int i = 0; i < color_names.Length; i++)
            {
                if (color_names[i].color_name == colorname)
                {
                    int rndclr = rnd.Next(0, color_names[i].colors.Length);
                    API.setVehiclePrimaryColor(API.getPlayerVehicle(player), color_names[i].colors[rndclr]);
                    API.setVehicleSecondaryColor(API.getPlayerVehicle(player), color_names[i].colors[rndclr]);
                    break;
                }
            }
        }
    }

    [Command("trythis")]
    public void testFunc(Client player)
    {
        NetHandle veh = API.getPlayerVehicle(player);

    }
}


