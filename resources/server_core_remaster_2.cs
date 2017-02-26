using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using System.Linq;
using System.IO;



public class server_core_remaster_2 : Script
{
    //Databases
    public MongoClient client = new MongoClient();
    public IMongoDatabase db_players;
    public IMongoDatabase db_vehicles;
    public IMongoCollection<BsonDocument> collection_players;
    public IMongoCollection<BsonDocument> collection_vehicles;

    //Database lists
    public List<ObjectData> object_database = new List<ObjectData>(); //User placed objects (static/nonstatic)
    public List<PlayerData> player_database = new List<PlayerData>();  //Player data
    public List<VehicleData> vehicle_database = new List<VehicleData>(); //Vehicle data
    public List<Blip> blip_database = new List<Blip>(); //Map blip data
    public List<int> RandomIDPlayerPool = new List<int>(); //Player ID recycle pool
    public List<int> RandomIDVehiclePool = new List<int>(); //Vehicle ID recycle pool
    public List<int> RandomIDObjectPool = new List<int>(); //Object ID recycle pool

    //Trackers
    static System.Timers.Timer timer; //Used for paychecks
    public bool timer_debouce = true; //Used for paychecks
    public int dealership_1_spawn_counter = 0; //Spawn locations for after-purchase of car
    public int dealership_dim = 0; //Dealership showroom dimension tracker

    //Used for rng
    public static Random rnd = new Random();

    //Initialisation of server
    public bool is_server_done_loading = false;

    public server_core_remaster_2()
    {
        //Handlers
        API.onResourceStart += OnResourceStartHandler;
        API.onResourceStop += OnResourceStopHandler;
        API.onPlayerConnected += OnPlayerConnectedHandler;
        API.onClientEventTrigger += OnClientEventTriggerHandler;
        API.onChatMessage += OnChatMessageHandler;
        API.onChatCommand += OnChatCommandHandler;
        API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
        API.onPlayerBeginConnect += OnPlayerBeginConnectHandler;

        Blip dealership_1_blip = API.createBlip(new Vector3(-61.70732, -1093.239, 26.4819));
        API.setBlipSprite(dealership_1_blip, 380);
        API.setBlipColor(dealership_1_blip, 47);
        API.setBlipScale(dealership_1_blip, 1.0f);
        API.setBlipShortRange(dealership_1_blip, true);
        blip_database.Add(dealership_1_blip);

        Blip clothes_1_blip = API.createBlip(new Vector3(83.10322, -1391.61, 29.41762));
        API.setBlipSprite(clothes_1_blip, 73);
        API.setBlipColor(clothes_1_blip, 18);
        API.setBlipScale(clothes_1_blip, 1.0f);
        API.setBlipShortRange(clothes_1_blip, true);
        blip_database.Add(clothes_1_blip);

        Blip seveneleven_1_blip = API.createBlip(new Vector3(29.47646, -1345.331, 29.49702));
        API.setBlipSprite(seveneleven_1_blip, 52);
        API.setBlipColor(seveneleven_1_blip, 66);
        API.setBlipScale(seveneleven_1_blip, 1.0f);
        API.setBlipShortRange(seveneleven_1_blip, true);
        blip_database.Add(seveneleven_1_blip);

        Blip seveneleven_2_blip = API.createBlip(new Vector3(-48.81896, -1756.122, 29.42099));
        API.setBlipSprite(seveneleven_2_blip, 52);
        API.setBlipColor(seveneleven_2_blip, 66);
        API.setBlipScale(seveneleven_2_blip, 1.0f);
        API.setBlipShortRange(seveneleven_2_blip, true);
        blip_database.Add(seveneleven_2_blip);

        Blip carwash_1_blip = API.createBlip(new Vector3(50.31694, -1393.075, 29.00219));
        API.setBlipSprite(carwash_1_blip, 100);
        API.setBlipColor(carwash_1_blip, 32);
        API.setBlipScale(carwash_1_blip, 1.0f);
        API.setBlipShortRange(carwash_1_blip, true);
        blip_database.Add(carwash_1_blip);

        Blip cargas_1_blip = API.createBlip(new Vector3(-71.19863, -1757.016, 29.03245));
        API.setBlipSprite(cargas_1_blip, 361);
        API.setBlipColor(cargas_1_blip, 49);
        API.setBlipScale(cargas_1_blip, 1.0f);
        API.setBlipShortRange(cargas_1_blip, true);
        blip_database.Add(cargas_1_blip);

        Blip atm_1_blip = API.createBlip(new Vector3(147.0421, -1034.716, 29.34404));
        API.setBlipSprite(atm_1_blip, 434);
        API.setBlipColor(atm_1_blip, 69);
        API.setBlipScale(atm_1_blip, 1.0f);
        API.setBlipShortRange(atm_1_blip, true);
        blip_database.Add(atm_1_blip);

        Blip bank_1_blip = API.createBlip(new Vector3(149.6064, -1039.721, 29.37407));
        API.setBlipSprite(bank_1_blip, 108);
        API.setBlipColor(bank_1_blip, 69);
        API.setBlipScale(bank_1_blip, 1.0f);
        API.setBlipShortRange(bank_1_blip, true);
        blip_database.Add(bank_1_blip);

        Blip publicphone_1_blip = API.createBlip(new Vector3(187.5654, -1043.799, 29.33121));
        API.setBlipSprite(publicphone_1_blip, 459);
        API.setBlipColor(publicphone_1_blip, 62);
        API.setBlipScale(publicphone_1_blip, 1.0f);
        API.setBlipShortRange(publicphone_1_blip, true);
        blip_database.Add(publicphone_1_blip);


        timer = new System.Timers.Timer(10000);
        timer.Elapsed += timer_Elapsed;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    //Animation flags
    [Flags]
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7
    }
     
    //Location used for login screen
    Vector3[] loginscreen_locations = new Vector3[]
    {
        new Vector3(-438.796, 1075.821, 353.000),
        new Vector3(2495.127, 6196.140, 202.541),
        new Vector3(-1670.700, -1125.000, 50.000),
        new Vector3(1655.813, 0.889, 200.0),
        new Vector3(1070.206, -711.958, 70.483),
    };

    Vector3[] registerspawn_locations = new Vector3[]
    {
        new Vector3(-1034.600, -2733.600, 13.800),
        new Vector3(-1037.256, -2736.902, 20.16927),
        new Vector3(-1093.557, -2746.308, 21.3594),
        new Vector3(-1043.668, -2729.368, 20.16929),
        new Vector3(-1026.843, -2739.599, 20.16929),
        new Vector3(-1019.212, -2738.315, 13.75679),
        new Vector3(-1032.755, -2742.37, 13.83),
        new Vector3(-1048.364, -2734.464, 13.85622),
        new Vector3(-1052.918, -2719.434, 13.75663),
    };

    //Used for dealership spawn locations
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

    //Store (dealership) data
    public struct StoreData
    {
        public string name;
        public string store_type_id;
        public string item_category;
        public Vector3 location;
        public string command;

        public StoreData(string cmd, string n, string item, string type, Vector3 loc)
        {
            name = n;
            item_category = item;
            store_type_id = type;
            location = loc;
            command = cmd;
        }
    }

    //List of stores
    StoreData[] store_locations = new StoreData[]
    {
        new StoreData("catalog", "Premium Deluxe \nMotorsport \n/catalog", "Vehicle(s):", "dealership_1", new Vector3(-61.70732, -1093.239, 26.4819)),
        new StoreData("catalog", "Discount Store \nClothing \n/catalog", "Skin(s):", "clothes_1", new Vector3(83.10322, -1391.61, 29.41762)),
        new StoreData("shop", "Convenience Store \n/shop", "Item(s):", "seveneleven_1", new Vector3(29.47646, -1345.331, 29.49702)),
        new StoreData("shop", "Convenience Store \n/shop", "Item(s):", "seveneleven_2", new Vector3(-48.81896, -1756.122, 29.42099)),
        new StoreData("wash", "Car Wash \n/wash", "null", "carwash_1", new Vector3(50.31694, -1393.075, 29.00219)),
        new StoreData("fill", "Car Gas \n/fill", "null", "cargas_1", new Vector3(-71.19863, -1757.016, 29.03245)),
        new StoreData("atm", "Fleeca ATM \n/atm", "null", "atm_1", new Vector3(147.0421, -1034.716, 29.34404)),
        new StoreData("bank", "Fleeca Bank \n/bank", "null", "bank_1", new Vector3(149.6064, -1039.721, 29.37407)),
        new StoreData("use", "Public Phone \n/use", "null", "publicphone_1", new Vector3(187.5654, -1043.799, 29.33121)),
    };

    //Store spawn locations
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

    //Used for vehicle color (Categorise colors under a color name)
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

    //List of categorised colors
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

    //Common cars used for dealership purchases
    public int[] common_car_colors = new int[]
    {
        2, 5, 7, 20, 25, 60, 113, 122, 78
    };

    //Object data for objects spawned in the world by players
    public class ObjectData
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int id { get; set; }
        public int obj_id { get; set; }
        [BsonIgnore]
        public NetHandle obj { get; set; }
        public SphereColShape coll_shape { get; set; }
        public bool is_static { get; set; }
        public string name { get; set; }
        public ObjectData(int id_par, int obj_id_par, NetHandle obj_par, bool is_static_par, string name_par)
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
            this.id = id_par;
            this.obj_id = obj_id_par;
            this.obj = obj_par;
            this.is_static = is_static_par;
            this.name = name_par;
            this.coll_shape = null;
        }

        public ObjectData()
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
            this.id = -1;
            this.obj_id = -1;
            this.obj = new NetHandle();
            this.coll_shape = null;
            this.is_static = false;
            this.name = "";
        }
    }

    //Vehicle data
    public class VehicleData
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIgnore]
        public Vehicle vehicle_object { get; set; }
        public int vehicle_id { get; set; }
        public string car_model_name { get; set; }

        public bool vehicle_engine { get; set; }
        public bool vehicle_locked { get; set; }
        public int vehicle_primary_color { get; set; }
        public int vehicle_secondary_color { get; set; }
        public string vehicle_color { get; set; }

        public Vector3 vehicle_position { get; set; }
        public Vector3 vehicle_rotation { get; set; }

        public string vehicle_license { get; set; }
        public string vehicle_owner { get; set; }
        public string vehicle_faction { get; set; }

        public int vehicle_gas_amount { get; set; }
        public List<ObjectData> vehicle_inventory { get; set; }

        public VehicleData(Vehicle hash, int id, string model_name, Vector3 pos, Vector3 rot, string license, string owner, string faction)
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
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

            this.vehicle_gas_amount = 100;
            this.vehicle_inventory = new List<ObjectData>();
        }

        public VehicleData()
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
        }
    }

    //Player data
    public class PlayerData
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonIgnore]
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

        public string bank_pin { get; set; }
        public long max_atm_withdrawl { get; set;}

        public List<ObjectData> player_inventory { get; set; }

        public PlayerData(Client player, int id, PedHash ped_hash, string player_name, string display_name, string password)
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
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
            this.bank_pin = "1234";
            this.max_atm_withdrawl = 100;
        }

        public PlayerData()
        {
            this.Id = new ObjectId();
            this.Id = ObjectId.GenerateNewId();
        }
    }
    
    //Animation data
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

    //List of animations
    public AnimData[] anim_names = new AnimData[]
    {
        new AnimData("clean", -1, "null", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop), "switch@franklin@cleaning_car", "001946_01_gc_fras_v2_ig_5_base"),
        new AnimData("clipboard1", -969349845, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_clipboard@male@idle_a", "idle_a"),
        new AnimData("clipboard2", -969349845, "PH_L_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_clipboard@male@idle_b", "idle_d"),
        new AnimData("phone1", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@female@idle_a", "idle_a"),
        new AnimData("phone2", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@male@idle_a", "idle_a"),
        new AnimData("phone3", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_mobile_film_shocking@female@idle_a", "idle_b"),
        new AnimData("phone4", 94130617, "PH_R_Hand", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "cellphone@", "cellphone_text_read_base"),
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


    //Utilities
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

    //Start server
    public void OnResourceStartHandler()
    {
        API.consoleOutput("Server_Core is initialising...");
        //Fill ID pools
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

        //test
        WebClient webClient = new WebClient();
        string IP = webClient.DownloadString("http://api.ipify.org/");
        API.consoleOutput("Public IP: " + IP);

        //Fetch database
        if (client.Cluster.Description.State.ToString() == "Disconnected")
        {
            API.consoleOutput("MongoDB database is offline!");
        }
        else
        {
            API.consoleOutput("Fetching database...");
            db_players = client.GetDatabase("PlayerDatabase");
            collection_players = db_players.GetCollection<BsonDocument>("PlayerData");
            db_vehicles = client.GetDatabase("VehicleDatabase");
            collection_vehicles = db_vehicles.GetCollection<BsonDocument>("VehicleData");


            var filter = new BsonDocument();
            API.consoleOutput("Fetching player data...");
            using (var cursor = collection_players.FindSync<BsonDocument>(filter))
            {
                while (cursor.MoveNext())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        var obj = BsonSerializer.Deserialize<PlayerData>(document);
                        //API.consoleOutput("BSON OBJ: " + document["player_display_name"].ToString());
                        API.consoleOutput("Found user: " + obj.player_game_name + " -> " + obj.Id.ToString());
                        player_database.Add(obj);
                    }
                }
            }
            API.consoleOutput("Complete.");
            API.consoleOutput("Fetching vehicle data...");
            using (var cursor = collection_vehicles.FindSync<BsonDocument>(filter))
            {
                while (cursor.MoveNext())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        var obj = BsonSerializer.Deserialize<VehicleData>(document);
                        //API.consoleOutput("BSON OBJ: " + document["player_display_name"].ToString());
                        API.consoleOutput("Found vehicle: " + obj.car_model_name + " -> " + obj.Id.ToString());
                        spawnExistingCar(ref obj);
                        API.sleep(100);
                        vehicle_database.Add(obj);
                    }
                }
            }
            API.consoleOutput("Complete.");
            API.consoleOutput("Database has been accumulated.");
        }

        API.requestIpl("shr_int"); //Load Deluxe Motorsport interior
        // API.requestIpl("shutter_closed");
        API.removeIpl("fakeint"); //Remove the fake Deluxe Motorsport interior  
        //API.requestIpl("v_carshowroom");
        API.consoleOutput("Server_Core has initialised.");
        is_server_done_loading = true;
    }

    //Server shutdown
    public void OnResourceStopHandler()
    {
        API.consoleOutput("Server_Core is terminating...");

        //Update database
        if (client.Cluster.Description.State.ToString() == "Disconnected")
        {
            API.consoleOutput("MongoDB database is offline!");
        }
        else
        {
            API.consoleOutput("Pushing database...");

            API.consoleOutput("Pushing player data...");
            for (int i = 0; i < player_database.Count; i++)
            {
                PlayerData player_data = player_database[i];
                if(player_data.player_client != null && player_data.player_logged)
                {
                    player_data.player_position = API.getEntityPosition(player_data.player_client);
                    player_data.player_rotation = API.getEntityRotation(player_data.player_client);
                }
                player_data.player_online = false;
                player_data.player_logged = false;
                player_data.player_id = -1;
                API.consoleOutput("_id -> " + player_data.Id.ToString() + " has been pushed into the database.");
                var bsonObj = player_data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq(w => w["_id"], player_data.Id);
                var result = collection_players.ReplaceOne(filter, bsonObj, new UpdateOptions { IsUpsert = true });
            }
            API.consoleOutput("Done.");

            API.consoleOutput("Pushing vehicle data...");
            for (int i = 0; i < vehicle_database.Count; i++)
            {
                VehicleData vehicle_data = vehicle_database[i];
                vehicle_data.vehicle_id = -1;
                vehicle_data.vehicle_locked = true;
                vehicle_data.vehicle_engine = false;
                if(vehicle_data.vehicle_object != null)
                {
                    vehicle_data.vehicle_position = API.getEntityPosition(vehicle_data.vehicle_object);
                    vehicle_data.vehicle_rotation = API.getEntityRotation(vehicle_data.vehicle_object);
                }
                API.consoleOutput("_id -> " + vehicle_data.Id.ToString() + " has been pushed into the database.");
                var bsonObj = vehicle_data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq(w => w["_id"], vehicle_data.Id);
                var result = collection_vehicles.ReplaceOne(filter, bsonObj, new UpdateOptions { IsUpsert = true });
            }
            API.consoleOutput("Done.");
        }

        API.consoleOutput("Server_Core has terminated.");
        API.sleep(2000);
    }
    
    public void OnPlayerBeginConnectHandler(Client player, CancelEventArgs e)
    {
        if(!is_server_done_loading)
        {
            e.Cancel = true;
        }
    }

    //Utilities
    public int getPlayerCount()
    {
        int online = 0;
        for(int i = 0; i < player_database.Count; i++)
            if (player_database[i].player_online)
                online++;
        return online;
    }

    public string getColorName(int id)
    {
        for(int i = 0; i < color_names.Length; i++)
        {
            for(int j = 0; j < color_names[i].colors.Length; j++)
            {
                if (color_names[i].colors[j] == id)
                    return color_names[i].color_name;
            }
        }
        return "null";
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
        for (int i = 0; i < player_database.Count; i++)
        {
            if (player_database[i].player_game_name == player.name)
            {
                found_player = true;
                index = i;
            }
        }
        PlayerData player_data = new PlayerData(null, 01, (PedHash)123, "", "", "");

        if (found_player)
        {
            API.consoleOutput("Player (" + player.name + ") exists in database.");
            player_data = player_database[index];
            player_data.player_client = player;
            player_data.player_id = getRandomIDPlayerPool();
            player_data.player_online = true;
            player_database[index] = player_data;
            API.sendChatMessageToPlayer(player, "Please ~b~login ~w~using /login [password].");
        }
        else
        {
            API.consoleOutput("Player (" + player.name + ") does not exist in database.");
            player_data = new PlayerData(player, getRandomIDPlayerPool(), API.pedNameToModel("Mani"), player.name, "test_mcbutt", "null_password");
            player_database.Add(player_data);
            API.sendChatMessageToPlayer(player, "Please ~b~register ~w~using /register [firstname_lastname] [password]");
        }

        if (client.Cluster.Description.State.ToString() == "Disconnected")
        {
            API.consoleOutput("MongoDB database is offline!");
        }
        else
        {
            API.consoleOutput("_id -> " + player_data.Id.ToString() + " has been pushed into the database.");
            var bsonObj = player_data.ToBsonDocument();
            var filter = Builders<BsonDocument>.Filter.Eq(w => w["_id"], player_data.Id);
            var result = collection_players.ReplaceOne(filter, bsonObj, new UpdateOptions { IsUpsert = true });
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


        //Sync current vehicles to user
        for (int i = 0; i < vehs.Count; i++)
        {
            if (API.getEntitySyncedData(vehs[i], "indicator_right") != null)
                API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 0, API.getEntitySyncedData(vehs[i], "indicator_right"));
            else
                API.setEntitySyncedData(vehs[i], "indicator_right", false);
            if (API.getEntitySyncedData(vehs[i], "indicator_left") != null)
                API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, vehs[i], 1, API.getEntitySyncedData(vehs[i], "indicator_left"));
            else
                API.setEntitySyncedData(vehs[i], "indicator_left", false);
            if (API.getEntitySyncedData(vehs[i], "trunk") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 5, vehs[i], API.getEntitySyncedData(vehs[i], "trunk"));
            else
                API.setEntitySyncedData(vehs[i], "trunk", false);
            if (API.getEntitySyncedData(vehs[i], "hood") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 4, vehs[i], API.getEntitySyncedData(vehs[i], "hood"));
            else
                API.setEntitySyncedData(vehs[i], "hood", false);
            if (API.getEntitySyncedData(vehs[i], "door1") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 0, vehs[i], API.getEntitySyncedData(vehs[i], "door1"));
            else
                API.setEntitySyncedData(vehs[i], "door1", false);
            if (API.getEntitySyncedData(vehs[i], "door2") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 1, vehs[i], API.getEntitySyncedData(vehs[i], "door2"));
            else
                API.setEntitySyncedData(vehs[i], "door2", false);
            if (API.getEntitySyncedData(vehs[i], "door3") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 2, vehs[i], API.getEntitySyncedData(vehs[i], "door3"));
            else
                API.setEntitySyncedData(vehs[i], "door3", false);
            if (API.getEntitySyncedData(vehs[i], "door4") != null)
                API.triggerClientEvent(player, "sync_vehicle_door_state", 3, vehs[i], API.getEntitySyncedData(vehs[i], "door4"));
            else
                API.setEntitySyncedData(vehs[i], "door4", false);
        }

        //Enable doors
        //void SET_STATE_OF_CLOSEST_DOOR_OF_TYPE(Hash type, float x, float y,
        //float z, BOOL locked, float heading, BOOL p6)
        //API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, API.getHashKey("v_ilev_cs_door01"), 82.3186f, -1392.752f, 29.5261f, false, 1.0, false);
        //API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, API.getHashKey("v_ilev_cs_door01_r"), 82.3186f, -1390.476f, 29.5261f, false, 1.0, false);

    }

    //Paycheck function
    public void applyPaychecks()
    {
        if (timer_debouce)
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

    //Used for hourly check
    void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (DateTime.Now.Minute == 00)
        {
            applyPaychecks();
        }
        else if (DateTime.Now.Minute != 00 && timer_debouce == false)
        {
            timer_debouce = true;
        }
    }

    //Car tyre damage
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_0_popped", true);
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_1_popped", true);
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_2_popped", true);
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_3_popped", true);
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_4_popped", true);
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
                else
                {
                    API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_TYRE_BURST, entity, 0, true, 1000.0);
                    API.setEntitySyncedData(entity, "tyre_5_popped", true);
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
        if(player_data.player_logged)
        {
            player_data.player_position = API.getEntityPosition(player);
            player_data.player_rotation = API.getEntityRotation(player);
        }
        RandomIDPlayerPool.Add(player_data.player_id);
        player_data.player_id = -1;
        player_data.player_client = null;
        player_data.player_online = false;
        player_data.player_logged = false;
        player_database[index] = player_data;

        if (client.Cluster.Description.State.ToString() == "Disconnected")
        {
            API.consoleOutput("MongoDB database is offline!");
        }
        else
        {
            API.consoleOutput("_id -> " + player_data.Id.ToString() + " has been pushed into the database.");
            var bsonObj = player_data.ToBsonDocument();
            var filter = Builders<BsonDocument>.Filter.Eq(w => w["_id"], player_data.Id);
            var result = collection_players.ReplaceOne(filter, bsonObj, new UpdateOptions { IsUpsert = true });
        }

    }

    public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
    {
        int indx = getPlayerDatabaseIndexByClient(player);
        string msgr = "(" + player_database[indx].player_id + ")" + player_database[indx].player_display_name + ": " + message;
        var players = API.getPlayersInRadiusOfPlayer(30.0f, player);
        foreach (Client c in players)
        {
            API.sendChatMessageToPlayer(c, msgr);
        }
        e.Cancel = true;
    }

    public void OnChatCommandHandler(Client player, string command, CancelEventArgs e)
    {
        int index = getPlayerDatabaseIndexByClient(player);
        
        //Limit users to /login and /register when they first join

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

    //Used for tyre burst chance
    private static readonly object syncLock = new object();
    public static bool randomBool()
    {
        lock (syncLock)
        { // synchronize
            int prob = rnd.Next(100);
            return prob <= 70;
        }
    }

    //Client triggers
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
        else if(eventName == "explode") //Tyre explosion
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
        else if(eventName == "check_bank_pin")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx == -1)
                API.triggerClientEvent(player, "failed_pin");
            else
            {
                if(player_database[indx].bank_pin == (string)args[0])
                {
                    API.triggerClientEvent(player, "success_pin");
                }
                else
                {
                    API.triggerClientEvent(player, "failed_pin");
                }
            }
        }
        else if(eventName == "fetch_bankdata")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx == -1)
                API.triggerClientEvent(player, "pulled_bankdata", "ERROR", "FETCH_ERROR");
            else
            {
                string name = player_database[indx].player_display_name;
                string mula = "$" + player_database[indx].player_money_bank.ToString("N0");
                API.triggerClientEvent(player, "pulled_bankdata", name, mula);
            }
        }
        else if(eventName == "depositThis")
        {
            long amount = Convert.ToInt64(args[0]);
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx != -1)
            {
                if(amount <= 0)
                {
                    API.sendChatMessageToPlayer(player, "You cannot deposit a negative or zero amount!");
                }
                else if(player_database[indx].player_money_hand - amount < 0)
                {
                    API.sendChatMessageToPlayer(player, "You cannot deposit this amount!");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You deposited ~b~$" + amount.ToString("N0"));
                    player_database[indx].player_money_hand -= amount;
                    player_database[indx].player_money_bank += amount;
                    API.triggerClientEvent(player, "update_sum", player_database[indx].player_display_name, "$" + player_database[indx].player_money_bank.ToString("N0"));
                }
            }
        }
        else if(eventName == "withdrawThis")
        {
            long amount = Convert.ToInt64(args[0]);
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx != -1)
            {
                if (amount <= 0)
                {
                    API.sendChatMessageToPlayer(player, "You cannot withdraw a negative or zero amount!");
                }
                else if (player_database[indx].player_money_bank - amount < 0)
                {
                    API.sendChatMessageToPlayer(player, "You cannot withdraw this amount!");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You withdrew ~b~$" + amount.ToString("N0"));
                    player_database[indx].player_money_hand += amount;
                    player_database[indx].player_money_bank -= amount;
                    API.triggerClientEvent(player, "update_sum", player_database[indx].player_display_name, "$" + player_database[indx].player_money_bank.ToString("N0"));
                }
            }
        }
        else if(eventName == "limitThis")
        {
            long amount = Convert.ToInt64(args[0]);
            int indx = getPlayerDatabaseIndexByClient(player);
            if(indx != -1)
            {
                if(amount < 0)
                {
                    API.sendChatMessageToPlayer(player, "Cannot limit to a negative!");
                }
                else
                {
                    player_database[indx].max_atm_withdrawl = amount;
                    API.sendChatMessageToPlayer(player, "ATM withdrawl limit set!");
                }
            }
        }
        else if(eventName == "getBankInfo")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx != -1)
                API.triggerClientEvent(player, "update_sum", player_database[indx].player_display_name, "$" + player_database[indx].player_money_bank.ToString("N0"));
            else
                API.sendChatMessageToPlayer(player, "WHY");
        }
        else if(eventName == "play_phone_anim")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx != -1)
            {
                animFunc(player, "phone4", true);
            }
        }
        else if(eventName == "stop_phone_anim")
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            if (indx != -1)
            {
                animFunc(player, "stop");
            }
        }
    }

    //Commands
    [Command("cef")]
    public void cefTestFunc(Client player)
    {
        API.triggerClientEvent(player, "cef_test"); //test
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

    [Command("me", "Usage: /me [action].", GreedyArg = true)]
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
            //Change player data and log him in
            PlayerData plr_temp = player_database[indx];
            plr_temp.player_logged = true;
            API.setEntityPositionFrozen(player, false);
            API.setEntityTransparency(player, 255);
            API.setEntityInvincible(player, false);
            API.setEntityCollisionless(player, false);
            API.setEntityPosition(player, plr_temp.player_position);
            API.setEntityRotation(player, plr_temp.player_rotation);
            player_database[indx] = plr_temp;

            API.setPlayerNametag(player, plr_temp.player_display_name);
            API.sendChatMessageToPlayer(player, "Welcome, ~b~" + player_database[indx].player_display_name + "~w~! TEST(" + API.getPlayerNametag(player) + ")");
            for (int i = 0; i < store_locations.Length; i++)
            {
                API.sendChatMessageToPlayer(player, "store_location_added");
                API.triggerClientEvent(player, "create_label", store_locations[i].name, store_locations[i].location, store_locations[i].store_type_id);
            }

            API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, -1148826190, 82.38156, -1390.476, 29.52609, false, 1.0, 0);
            API.sendNativeToPlayer(player, GTANetworkServer.Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, 868499217, 82.38156, -1390.752, 29.52609, false, 1.0, 0);

            //API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_PED_CONFIG_FLAG, player.handle, 223, true);
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

        for(int i = 0; i < player_database.Count; i++)
        {
            if(player_database[i].player_display_name.ToLower() == name)
            {
                API.sendChatMessageToPlayer(player, "Name is already taken!");
                return;
            }
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
        plr_temp.player_position = registerspawn_locations[rnd.Next(0, registerspawn_locations.Length)];
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

    [Command("spawncar", "Usage: /spawncar [name].", GreedyArg = true)]
    public void spawnCarFunc(Client player, string carname)
    {
        spawnCar(player, carname, false, new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), 0, 0, true, 0);
    }

    public void spawnExistingCar(ref VehicleData veh) //Used for database initialisation
    {
        veh.vehicle_object = API.createVehicle(API.vehicleNameToModel(veh.car_model_name), veh.vehicle_position, veh.vehicle_rotation, veh.vehicle_primary_color, veh.vehicle_secondary_color);
        veh.vehicle_id = getRandomIDVehiclePool();
        API.setVehicleNumberPlate(veh.vehicle_object, veh.vehicle_license);
        API.setVehicleEngineStatus(veh.vehicle_object, false);
        API.setVehicleLocked(veh.vehicle_object, true);
        API.setEntitySyncedData(veh.vehicle_object, "id", (int)veh.vehicle_id);
        API.setEntitySyncedData(veh.vehicle_object, "owner", (string)veh.vehicle_owner);
        API.setEntitySyncedData(veh.vehicle_object, "plate", (string)veh.vehicle_license);
        API.setEntitySyncedData(veh.vehicle_object, "engine", false);
        API.setEntitySyncedData(veh.vehicle_object, "indicator_right", false);
        API.setEntitySyncedData(veh.vehicle_object, "indicator_left", false);
        API.setEntitySyncedData(veh.vehicle_object, "locked", true);
        API.setEntitySyncedData(veh.vehicle_object, "trunk", false);
        API.setEntitySyncedData(veh.vehicle_object, "hood", false);
        API.setEntitySyncedData(veh.vehicle_object, "door1", false);
        API.setEntitySyncedData(veh.vehicle_object, "door2", false);
        API.setEntitySyncedData(veh.vehicle_object, "door3", false);
        API.setEntitySyncedData(veh.vehicle_object, "door4", false);
        API.setEntitySyncedData(veh.vehicle_object, "attached", false);

        API.setEntitySyncedData(veh.vehicle_object, "tyre_0_popped", false);
        API.setEntitySyncedData(veh.vehicle_object, "tyre_1_popped", false);
        API.setEntitySyncedData(veh.vehicle_object, "tyre_2_popped", false);
        API.setEntitySyncedData(veh.vehicle_object, "tyre_3_popped", false);
        API.setEntitySyncedData(veh.vehicle_object, "tyre_4_popped", false);
        API.setEntitySyncedData(veh.vehicle_object, "tyre_5_popped", false);

        API.setVehiclePrimaryColor(veh.vehicle_object, veh.vehicle_primary_color);
        API.setVehicleSecondaryColor(veh.vehicle_object, veh.vehicle_secondary_color);

    }

    public void spawnCar(Client player, string carname, bool forcePos, Vector3 pos, Vector3 rot, int color1, int color2, bool owned, int dim)
    {
        if(owned)
        {
            int indx = getPlayerDatabaseIndexByClient(player);
            API.sendChatMessageToPlayer(player, "Spawned car!");
            Vehicle hash = null;
            if (forcePos)
                hash = API.createVehicle(API.vehicleNameToModel(carname), pos, rot, 0, 0, dim);
            else
                hash = API.createVehicle(API.vehicleNameToModel(carname), API.getEntityPosition(player), API.getEntityRotation(player), 0, 0, dim);
            
            if(hash == null)
            {
                API.sendChatMessageToPlayer(player, "Car model name is incorrect.");
                return;
            }
            
            //API.setVehicleNumberPlate(hash, "TJM");
            VehicleData temp = new VehicleData(hash, getRandomIDVehiclePool(), carname, API.getEntityPosition(hash), API.getEntityRotation(hash), "tjm000", player_database[indx].player_display_name, "civillian");
            const string strng = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            char[] chars = new char[6];
            for(int i = 0; i < 6; i++)
            {
                chars[i] = strng[rnd.Next(0, strng.Length)];
            }

            temp.vehicle_license = new string(chars);
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
            API.setEntitySyncedData(temp.vehicle_object, "tyre_0_popped", false);
            API.setEntitySyncedData(temp.vehicle_object, "tyre_1_popped", false);
            API.setEntitySyncedData(temp.vehicle_object, "tyre_2_popped", false);
            API.setEntitySyncedData(temp.vehicle_object, "tyre_3_popped", false);
            API.setEntitySyncedData(temp.vehicle_object, "tyre_4_popped", false);
            API.setEntitySyncedData(temp.vehicle_object, "tyre_5_popped", false);
            API.setVehicleNumberPlate(temp.vehicle_object, (string)temp.vehicle_license);
            API.setVehicleEngineStatus(temp.vehicle_object, false);
            API.setVehicleLocked(temp.vehicle_object, true);
            API.setVehiclePrimaryColor(temp.vehicle_object, color1);
            API.setVehicleSecondaryColor(temp.vehicle_object, color2);
            temp.vehicle_primary_color = color1;
            temp.vehicle_secondary_color = color2;
            temp.vehicle_color = getColorName(color1);
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
        API.sendChatMessageToPlayer(player, "Paycheck: ~b~$" + player_database[indx].player_paycheck.ToString("N0"));
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

    [Command("close", "Usage: /close [car part].", GreedyArg = true)] //Close car parts
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

    [Command("open", "Usage: /open [car part].", GreedyArg = true)] //Open car parts
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

    [Command("anim", "Usage: /anim [name/help/stop].", GreedyArg = true)]
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
            if(indx != -1)
            {
                List<NetHandle> vehs = new List<NetHandle>();
                vehs = API.getAllVehicles();
                float smallestDist = 100.0f;
                NetHandle closestveh = new NetHandle();
                bool found = false;
                for (int i = 0; i < vehs.Count; i++)
                {
                    if (vehs[i] != player_veh)
                    {
                        int veh_indx = getVehicleIndexByVehicle(vehs[i]);
                        if(veh_indx != -1)
                        {
                            if (vehicle_database[veh_indx].vehicle_owner.ToLower() == player_database[indx].player_display_name.ToLower())
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

    [Command("runplate", "Usage: /runplate [plate #].", GreedyArg = true)]
    public void carinfoFunc(Client player, string license)
    {
        //API.triggerClientEvent(player, "vehicle_draw_text", temp.vehicle_hash, temp.vehicle_id, temp.owner_name, temp.license_plate);
        license = license.ToLower(); //Get lowercase string for lazy people
        int indx = getPlayerDatabaseIndexByClient(player);
        if (player_database[indx].player_faction == "police")
        {
            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            bool found = false;
            for (int i = 0; i < vehs.Count; i++)
            {
                if (API.getVehicleNumberPlate(vehs[i]).ToLower() == license.ToLower())
                {
                    //Display data about car
                    API.sendChatMessageToPlayer(player, "Vehicle is ~g~registered ~w~in database!");
                    var model = API.getEntityModel(vehs[i]);
                    int carindx = getVehicleIndexByVehicle(vehs[i]);
                    string plr_nm = vehicle_database[carindx].vehicle_owner;
                    API.sendChatMessageToPlayer(player, "Model: ~b~" + getVehicleName(vehs[i]) + " ~b~=+= ~w~Color: ~b~" + vehicle_database[carindx].vehicle_color);
                    API.sendChatMessageToPlayer(player, "License Plate: ~b~" + license.ToUpper());
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

    [Command("check", "Usage: /check [car part].", GreedyArg = true)]
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

    [Command("put", "Usage: /put [object name] [car part].", GreedyArg = true)]
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

    [Command("take", "Usage: /take [object name] [car part].", GreedyArg = true)]
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

    [Command("addmoney", "Usage: /addmoney [amount] [firstname_lastname].", GreedyArg = true)]
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

    [Command("teleport", "Usage: /teleport [X] [Y] [Z].", GreedyArg = true)]
    public void teleportFunc(Client player, string coords)
    {
        char[] delimiter = { ' ' };

        string[] words = coords.Split(delimiter);

        double x;
        double y;
        double z;

        x = Convert.ToDouble(words[0]);
        if(words.Length == 3)
        {
            y = Convert.ToDouble(words[1]);
            z = Convert.ToDouble(words[2]);
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Coordinates are incorrectly formatted.");
            return;
        }

        Vector3 telepos = new Vector3(x, y, z);
        API.setEntityPosition(player, telepos);
    }

    [Command("addobject", "Usage: /addobject [id] [name].", GreedyArg = true)]
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

    [Command("removeobject", "Usage: /removeobject [name].", GreedyArg = true)]
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

    [Command("place", "Usage: /place [object name].", GreedyArg = true)]
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
    public void catalogFunc(Client player)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && store_locations[i].command == "catalog")
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

    [Command("shop")]
    public void shopFunc(Client player)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && store_locations[i].command == "shop")
                {
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
                API.triggerClientEvent(player, "shop_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);
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

    [Command("atm")]
    public void atmFunc(Client player)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && store_locations[i].command == "atm")
                {
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                int indx = getPlayerDatabaseIndexByClient(player);
                if (indx != -1)
                    API.sendChatMessageToPlayer(player, "Bank Sum: ~b~$" + player_database[indx].player_money_bank.ToString("N0"));
                API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
                API.triggerClientEvent(player, "atm_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "There is no ATM nearby!");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }
    }

    [Command("bank")]
    public void bankFunc(Client player)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && store_locations[i].command == "bank")
                {
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                //int indx = getPlayerDatabaseIndexByClient(player);
                //if (indx != -1)
                    //API.sendChatMessageToPlayer(player, "Bank Sum: ~b~$" + player_database[indx].player_money_bank.ToString("N0"));
                API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
                API.triggerClientEvent(player, "bank_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);

            }
            else
            {
                API.sendChatMessageToPlayer(player, "There is no bank nearby!");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }
    }

    [Command("withdraw", GreedyArg = true)]
    public void withdrawFunc(Client player, string msg)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            long amount = Convert.ToInt64(msg);
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            bool isatm = false;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && (store_locations[i].command == "bank" || store_locations[i].command == "atm"))
                {
                    if (store_locations[i].command == "bank")
                        isatm = false;
                    else
                        isatm = true;
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                int indx = getPlayerDatabaseIndexByClient(player);
                if (indx != -1)
                {
                    if (amount > player_database[indx].max_atm_withdrawl && isatm)
                    {
                        API.sendChatMessageToPlayer(player, "Maximum withdraw limit reached!");
                    }
                    else
                    {
                        if (player_database[indx].player_money_bank - amount >= 0)
                        {
                            player_database[indx].player_money_bank -= amount;
                            player_database[indx].player_money_hand += amount;
                            API.sendChatMessageToPlayer(player, "You withdrew ~b~$" + amount.ToString("N0"));
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Your bank sum is too low!");
                        }
                    }
                }
                //API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
               // API.triggerClientEvent(player, "atm_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "There is no bank/ATM nearby!");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }
    }

    [Command("deposit", GreedyArg = true)]
    public void depositFunc(Client player, string msg)
    {
        if (!API.isPlayerInAnyVehicle(player))
        {
            long amount = Convert.ToInt64(msg);
            Vector3 plr_pos = API.getEntityPosition(player);
            float smallest_dist = 100.0f;
            int store_index = -1;
            bool isatm = false;
            for (int i = 0; i < store_locations.Length; i++)
            {
                float currdist = vecdist(plr_pos, store_locations[i].location);

                if (currdist < smallest_dist && (store_locations[i].command == "bank" || store_locations[i].command == "atm"))
                {
                    if (store_locations[i].command == "bank")
                        isatm = false;
                    else
                        isatm = true;
                    smallest_dist = currdist;
                    store_index = i;
                }
            }

            if (smallest_dist < 2.0f && store_index != -1)
            {
                int indx = getPlayerDatabaseIndexByClient(player);
                if (indx != -1)
                {
                    if (isatm)
                    {
                        API.sendChatMessageToPlayer(player, "You cannot deposit at an ATM. Visit a bank!");
                    }
                    else
                    {
                        if (player_database[indx].player_money_hand - amount >= 0)
                        {
                            player_database[indx].player_money_bank += amount;
                            player_database[indx].player_money_hand -= amount;
                            API.sendChatMessageToPlayer(player, "You deposited ~b~$" + amount.ToString("N0"));
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Your money sum is too low!");
                        }
                    }
                }
                //API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
                // API.triggerClientEvent(player, "atm_list", store_locations[store_index].name, store_locations[store_index].item_category, store_locations[store_index].store_type_id);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "There is no bank nearby!");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that in a vehicle!");
        }
    }

    [Command("exitcef")]
    public void exitCefFunc(Client player)
    {
        API.triggerClientEvent(player, "close_cef");
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

    [Command("setcolor", "Usage: /setcolor [name].", GreedyArg = true)]
    public void setColorFunc(Client player, string colorname)
    {
        if (API.isPlayerInAnyVehicle(player))
        {
            int veh_indx = getVehicleIndexByVehicle(API.getPlayerVehicle(player));
            for (int i = 0; i < color_names.Length; i++)
            {
                if (color_names[i].color_name == colorname)
                {
                    int rndclr = rnd.Next(0, color_names[i].colors.Length);
                    API.setVehiclePrimaryColor(API.getPlayerVehicle(player), color_names[i].colors[rndclr]);
                    API.setVehicleSecondaryColor(API.getPlayerVehicle(player), color_names[i].colors[rndclr]);

                    if(veh_indx != -1)
                    {
                        vehicle_database[veh_indx].vehicle_primary_color = color_names[i].colors[rndclr];
                        vehicle_database[veh_indx].vehicle_secondary_color = color_names[i].colors[rndclr];
                        vehicle_database[veh_indx].vehicle_color = color_names[i].color_name;
                    }

                    break;
                }
            }
        }
    }

    [Command("time")]
    public void timeFunc(Client player)
    {
        API.sendChatMessageToPlayer(player, "Current Time: ~b~ " + DateTime.Now.ToString());
    }

    [Command("setpaycheck", "Usage: /setpaycheck [amount].", GreedyArg = true)]
    public void setPayCheckFunc(Client player, string arg)
    {
        long paycheck = Convert.ToInt64(arg);
        int indx = getPlayerDatabaseIndexByClient(player);
        PlayerData temp = player_database[indx];
        temp.player_paycheck = paycheck;
        player_database[indx] = temp;
        API.sendChatMessageToPlayer(player, "Paycheck applied.");
    }

    [Command("setskin", GreedyArg = true)]
    public void setSkin(Client player, string msg)
    {
        API.setPlayerSkin(player, API.pedNameToModel(msg));
    }

    [Command("phone")]
    public void phoneFunc(Client player)
    {
        API.triggerClientEvent(player, "bring_phone");
    }
}


