using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;


public class server_core_remaster : Script
{
    public const bool __debug = true;

    [Flags]
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7
    }

    /***********************DATA STRUCTURES***********************/
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

    public struct StoreData
    {
        public string name;
        public string store_type_id;
        public Vector3 location;

        public StoreData(string n, string type, Vector3 loc)
        {
            name = n;
            store_type_id = type;
            location = loc;
        }
    }

    StoreData[] store_locations = new StoreData[]
    {
        new StoreData("~o~Premium Deluxe Motorsport \npurchase", "dealership_1", new Vector3(-61.70732, -1093.239, 26.4819)),
    };

    Vector3[] loginscreen_locations = new Vector3[]
    {
        new Vector3(-438.796, 1075.821, 353.000),
        new Vector3(2495.127, 6196.140, 202.541),
        new Vector3(-1670.700, -1125.000, 50.000),
        new Vector3(1655.813, 0.889, 200.0),
        new Vector3(1070.206, -711.958, 70.483),
    };

    public struct VehicleData
    {
        public int vehicle_id;
        public Vehicle vehicle_hash;
        public bool engine_on;
        public bool vehicle_locked;
        public int engine_health;
        public int primary_color;
        public int secondary_color;
        public string color_name;
        public Vector3 position;
        public Vector3 rotation;
        public int dirt_level;
        public string license_plate;
        public string owner_name;
        public string faction;
        public List<ObjectData> inventory;

        public VehicleData(bool value)
        {
            vehicle_id = -1;
            vehicle_hash = null;
            engine_on = false;
            vehicle_locked = true;
            engine_health = 100;
            primary_color = 0;
            secondary_color = 0;
            color_name = "Black";
            position = new Vector3(0.0, 0.0, 0.0);
            rotation = new Vector3(0.0, 0.0, 0.0);
            dirt_level = 0;
            license_plate = "TJM000";
            owner_name = "null";
            faction = "civillian";
            inventory = new List<ObjectData>();
        }
    }

    public struct FineData
    {
        public int id;
        public int amount;
        public bool paid;
        public string dateissued;
        public int timedue;
        public string information;

        public FineData(int d, int mnt, string info, string dateissue, int time)
        {
            id = d;
            amount = mnt;
            information = info;
            paid = false;
            dateissued = dateissue;
            timedue = time;
        }
    }

    public struct ObjectData
    {
        public int id;
        public int obj_id;
        public NetHandle obj;
        public bool deletable;
        public string name;
        public ObjectData(int i, int obj_i, NetHandle o, bool del, string nm)
        {
            id = i;
            obj_id = obj_i;
            obj = o;
            deletable = del;
            name = nm;
        }
    }

    public struct PlayerData
    {
        public int player_id;
        public string player_real_name; //Name of connected user
        public string player_fake_name; //Name displayed in-game
        public string password;
        public bool is_offline;
        public bool is_logged;
        public bool is_registered;
        public bool data_reset; //Used for validation
        //Data
        public Vector3 position;
        public Vector3 rotation;
        public Ped player_ped_hash;
        public int armor;
        public int health;
        public ulong money_in_hand;
        public ulong money_in_bank;
        public ulong pay_check;
        public int vehicles_owned;
        public int paid_fines;
        public int unpaid_fines;
        public string phone_number;
        public string faction;
        public List<VehicleData> vehicles;
        public List<FineData> fines;
        public List<ObjectData> inventory;

        public PlayerData(bool value)
        {
            player_id = -1;
            player_real_name = "";
            player_fake_name = "Test_McTest";
            password = "";
            is_offline = true;
            is_logged = false;
            is_registered = false;
            data_reset = true;
            //----
            position = new Vector3(0.0, 0.0, 0.0);
            rotation = new Vector3(0.0, 0.0, 0.0);
            player_ped_hash = null;
            armor = 0;
            health = 100;
            money_in_hand = 0;
            money_in_bank = 0;
            pay_check = 0;
            vehicles_owned = 0;
            paid_fines = 0;
            unpaid_fines = 0;
            phone_number = "000-0000";
            faction = "civillian";
            vehicles = new List<VehicleData>();
            fines = new List<FineData>();
            inventory = new List<ObjectData>();
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

    /*************************************************************/

    //Helper functions
    string getColorNameByInt(int col)
    {
        for(int i = 0; i < color_names.Length; i++)
            for(int j = 0; j < color_names[i].colors.Length; j++)
                if (color_names[i].colors[j] == col)
                    return color_names[i].color_name;
        return "null";
    }

    public bool doesIDPlayerPoolExist(int id)
    {
        for(int i = 0; i < RandomIDPlayerPool.Count; i++)
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

    public bool doesIDFinePoolExist(int id)
    {
        for (int i = 0; i < RandomIDFinePool.Count; i++)
            if (RandomIDFinePool[i] == id)
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

    public int getPlayerIDByName(string name)
    {
        for(int i = 0; i < plr_database.Count; i++)
            if (plr_database[i].player_real_name == name || plr_database[i].player_fake_name == name)
                return plr_database[i].player_id;
        return -1;
    }

    public VehicleData getVehicleDataByID(int id)
    {
        for(int i = 0; i < plr_database.Count; i++)
            for (int j = 0; j < plr_database[i].vehicles.Count; j++)
                if (plr_database[i].vehicles[j].vehicle_id == id)
                    return plr_database[i].vehicles[j];
        return new VehicleData();
    }

    public void replaceVehicleDataByID(int id, VehicleData data)
    {
        for(int i = 0; i < plr_database.Count; i++)
            for (int j = 0; j < plr_database[i].vehicles.Count; j++)
                if (plr_database[i].vehicles[j].vehicle_id == id)
                    plr_database[i].vehicles[j] = data;
    }

    public int getPlayerIndexByName(string name)
    {
        for (int i = 0; i < plr_database.Count; i++)
            if (plr_database[i].player_fake_name.ToLower() == name.ToLower() || plr_database[i].player_real_name.ToLower() == name.ToLower())
                return i;
        return -1;
    }

    public string getVehicleOwnerNameByID(int id)
    {
        for (int i = 0; i < plr_database.Count; i++)
            for (int j = 0; j < plr_database[i].vehicles.Count; j++)
                if (plr_database[i].vehicles[j].vehicle_id == id)
                    return plr_database[i].vehicles[j].owner_name;
        return "null";
    }

    public int getVehicleIndexByOwnerName(string name, int id)
    {
        int indx = getPlayerIndexByName(name);
        for (int i = 0; i < plr_database[indx].vehicles.Count; i++)
            if (plr_database[indx].vehicles[i].vehicle_id == id)
                return i;
        return -1;
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

    public int getRandomIDFinePool()
    {
        int x = RandomIDFinePool[0];
        RandomIDFinePool.RemoveAt(0);
        return x;
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

    private double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    //Rotate vector around another vector
    Vector3 rotatedVector(Vector3 src, Vector3 center, double angle)
    {
        Vector3 res = new Vector3(0.0, 0.0, 0.0);
        double radius = 0.75;
        res.X = Convert.ToSingle(Math.Cos(DegreeToRadian(angle)) * radius + center.X);
        res.Y = Convert.ToSingle(Math.Sin(DegreeToRadian(angle)) * radius + center.Y);
        res.Z = center.Z;

        return res;
    }

    //Data pools
    public List<ObjectData> worldObjectPool = new List<ObjectData>();
    public List<PlayerData> plr_database = new List<PlayerData>();
    public List<int> RandomIDPlayerPool = new List<int>();
    public List<int> RandomIDVehiclePool = new List<int>();
    public List<int> RandomIDFinePool = new List<int>();
    public List<int> RandomIDObjectPool = new List<int>();

    int plrs_online;

    public List<Blip> map_blips = new List<Blip>();

    public server_core_remaster()
    {
        //Event handlers
        API.onResourceStart += OnResourceStartHandler;
        API.onPlayerBeginConnect += OnPlayerBeginConnectHandler;
        API.onPlayerConnected += OnPlayerConnectedHandler;
        API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
        API.onPlayerFinishedDownload += OnPlayerFinishedDownloadHandler;
        API.onClientEventTrigger += OnClientEventTriggerHandler;
        API.onChatMessage += OnChatMessageHandler;
        API.onChatCommand += OnChatCommandHandler;
        API.onUpdate += OnUpdateHandler;

        Blip newblip = API.createBlip(new Vector3(-61.70732, -1093.239, 26.4819));
        API.setBlipSprite(newblip, 380);
        API.setBlipColor(newblip, 47);
        API.setBlipScale(newblip, 1.0f);

        map_blips.Add(newblip);
    }

    public void OnPlayerDisconnectedHandler(Client player, string reason)
    {
        if (__debug)
            API.consoleOutput("Player (" + player.name + ") has disconnected. Reason: " + reason);

        plrs_online--;

        int id = getPlayerIDByName(player.name);
        if(id != -1)
        {
            RandomIDPlayerPool.Add(id);
            int indx = getPlayerIndexByName(player.name);
            PlayerData plr_temp = plr_database[indx];
            plr_temp.player_id = -1;
            plr_temp.is_offline = true;
            if(plr_temp.is_logged == true)
            {
                plr_temp.position = API.getEntityPosition(player);
                plr_temp.rotation = API.getEntityRotation(player);
                plr_temp.is_logged = false;
            }
            plr_database[indx] = plr_temp;
        }
    }

    public void OnPlayerFinishedDownloadHandler(Client player)
    {
       
    }

    public void OnResourceStartHandler()
    {
        if(__debug)
            API.consoleOutput("RPcore is initialising...");

        //Initialize ID pools only at beginning

        if (__debug)
            API.consoleOutput("Player ID Pool is being created...");
        for (int i = 0; i < 1000; i++)
        {
            Random rnd = new Random();
            int temp = rnd.Next(1, 100000);
            while (doesIDPlayerPoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDPlayerPool.Add(temp);
        }

        if (__debug)
            API.consoleOutput("Vehicle ID Pool is being created...");
        for (int i = 0; i < 1000; i++)
        {
            Random rnd = new Random();
            int temp = rnd.Next(1, 100000);
            while (doesIDVehiclePoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDVehiclePool.Add(temp);
        }

        if (__debug)
            API.consoleOutput("Fine ID Pool is being created...");
        for (int i = 0; i < 1000; i++)
        {
            Random rnd = new Random();
            int temp = rnd.Next(1, 100000);
            while (doesIDFinePoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDFinePool.Add(temp);
        }

        if (__debug)
            API.consoleOutput("Object ID Pool is being created...");
        for (int i = 0; i < 1000; i++)
        {
            Random rnd = new Random();
            int temp = rnd.Next(1, 100000);
            while (doesIDObjectPoolExist(temp))
                temp = rnd.Next(1, 100000);
            RandomIDObjectPool.Add(temp);
        }

        if(__debug)
            API.consoleOutput("RPcore has initialised!");
    }

    public void OnPlayerBeginConnectHandler(Client player, CancelEventArgs e)
    {
        if(__debug)
            API.consoleOutput("Player (" + player.name + ") begun connecting...");
    }

    public void OnPlayerConnectedHandler(Client player)
    {
        plrs_online++;
        API.sendChatMessageToPlayer(player, "~w~--~b~raptube69's RP Server~w~--           v1.0.0");
        API.sendChatMessageToPlayer(player, "~w~Player(s) Online: ~b~" + plrs_online);
        if(__debug)
            API.consoleOutput("Player (" + player.name + ") has connected!");

        bool player_exists = false;
        for(int i = 0; i < plr_database.Count; i++)
        {
            if (plr_database[i].player_real_name == player.name)
            {
                player_exists = true;
                break;
            }
        }

        if(player_exists)
        {
            if (__debug)
                API.consoleOutput("Player (" + player.name + ") already exists.");
            int indx = getPlayerIndexByName(player.name);
            PlayerData plr_temp = plr_database[indx];
            plr_temp.player_id = getRandomIDPlayerPool();
            plr_temp.is_offline = false;
            plr_database[indx] = plr_temp;
            API.sendChatMessageToPlayer(player, "Please ~b~login ~w~using /login ~b~(password)~w~.");
        }
        else
        {
            if (__debug)
                API.consoleOutput("Player (" + player.name + ") is new.");
            PlayerData plr_temp = new PlayerData(true);
            plr_temp.player_real_name = player.name;
            API.setPlayerName(player, plr_temp.player_fake_name);
            plr_temp.player_id = getRandomIDPlayerPool();
            plr_temp.is_offline = false;
            plr_database.Add(plr_temp);
            API.sendChatMessageToPlayer(player, "Please ~b~register ~w~using /register ~b~(password)~w~.");
        }

        Random rnd = new Random();
        int rnd_location = rnd.Next(0, loginscreen_locations.Length);
        API.setPlayerSkin(player, API.pedNameToModel("Mani"));
        API.setEntityPosition(player, loginscreen_locations[rnd_location]);
        API.setEntityPositionFrozen(player, true);
        API.setEntityTransparency(player, 0);
        API.setEntityInvincible(player, true);
        API.setEntityCollisionless(player, true);

        //API.triggerClientEvent(player, "create_label", "~o~Premium Deluxe Motorsport \n\\purchase", new Vector3(-61.70732, -1093.239, 26.4819));
        
    }

    public void OnUpdateHandler()
    {

    }

    public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    public void OnChatCommandHandler(Client player, string command, CancelEventArgs e)
    {
        int indx = getPlayerIndexByName(player.name);
        if(indx != -1)
        {
            if (__debug)
                API.consoleOutput("cmd: " + command);
            if (command.StartsWith("/login"))
            {
                if (plr_database[indx].is_registered == false)
                {
                    API.sendChatMessageToPlayer(player, "You are not ~b~registered~w~! Register using /register ~b~(password)~w~.");
                    e.Cancel = true;
                }
                else if (plr_database[indx].is_logged == true)
                {
                    API.sendChatMessageToPlayer(player, "You are already ~b~logged ~w~in!");
                    e.Cancel = true;
                }
            }
            else if (command.StartsWith("/register"))
            {
                if (plr_database[indx].is_registered == true)
                {
                    API.sendChatMessageToPlayer(player, "You are already ~b~registered~w~!");
                    e.Cancel = true;
                }
                else if (plr_database[indx].is_logged == true)
                {
                    API.sendChatMessageToPlayer(player, "You are already ~b~registered~w~!");
                    e.Cancel = true;
                }
            }
            else
            {
                if (plr_database[indx].is_registered == false)
                {
                    API.sendChatMessageToPlayer(player, "You are not ~b~registered~w~! Register using /register ~b~(password)~w~.");
                    e.Cancel = true;
                }
                else if (plr_database[indx].is_logged == false)
                {
                    API.sendChatMessageToPlayer(player, "You are not ~b~logged ~w~in! Login using /login ~b~(password)~w~.");
                    e.Cancel = true;
                }
            }
        }
    }

    public void OnClientEventTriggerHandler(Client player, string eventName, params object[] arguments)
    {
        if (eventName == "indicator_left")
        {
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left") != null)
                API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, API.getPlayerVehicle(player), 1, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left"));
            //API.sendNativeToAllPlayers(0xB5D45264751B7DF0, API.getPlayerVehicle(player), 1, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left"));
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left") != null)
                API.setEntitySyncedData(API.getPlayerVehicle(player), "indicator_left", !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_left"));
        }
        else if (eventName == "indicator_right")
        {
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right") != null)
                API.sendNativeToAllPlayers(GTANetworkServer.Hash.SET_VEHICLE_INDICATOR_LIGHTS, API.getPlayerVehicle(player), 0, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right"));
            //API.sendNativeToAllPlayers(0xB5D45264751B7DF0, API.getPlayerVehicle(player), 0, !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right"));
            if (API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right") != null)
                API.setEntitySyncedData(API.getPlayerVehicle(player), "indicator_right", !API.getEntitySyncedData(API.getPlayerVehicle(player), "indicator_right"));
        }
        else if (eventName == "do_anim")
        {
            API.sendChatMessageToPlayer(player, "do_anim_call: " + arguments[0]);
            animFunc(player, (string)arguments[0], true);
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
        int id = getPlayerIDByName(player.name);
        API.sendChatMessageToPlayer(player, "Your ID is: ~b~" + id + ".");
    }

    [Command("pos")] //debug
    public void getPos(Client player)
    {
        Vector3 vec = API.getEntityPosition(player);
        API.sendChatMessageToPlayer(player, "X:" + vec.X + "Y:" + vec.Y + "Z:" + vec.Z);
    }

    [Command("setfaction", GreedyArg = true)]
    public void setFactionCommand(Client player, string fac)
    {
        int indx = getPlayerIndexByName(player.name);
        if (fac == "police" || fac == "civillian")
        {
            PlayerData temp = plr_database[indx];
            temp.faction = fac;
            plr_database[indx] = temp;
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Faction is not valid!");
        }
    }

    [Command("me", GreedyArg = true)]
    public void meCommand(Client player, string msg)
    {
        int indx = getPlayerIndexByName(player.name);
        string msgr = ">" + plr_database[indx].player_fake_name + " " + msg;
        var players = API.getPlayersInRadiusOfPlayer(30, player);
        foreach (Client c in players)
        {
            API.sendChatMessageToPlayer(c, "~#CC99FF~", msgr);
        }
    }

    [Command("lock")]
    public void lockFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
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
                smallestDist = vr;
                closestveh = vehs[i];
                found = true;
            }
        }

        if (found) //Found SOME car
        {
            if (smallestDist < 2.5f) //Close enough?
            {
                if (getVehicleOwnerNameByID(API.getEntitySyncedData(closestveh, "id")) == plr_database[indx].player_fake_name)
                {
                    API.setVehicleLocked(closestveh, true);
                    //API.sendChatMessageToPlayer(player, "You have ~b~locked ~w~the car.");
                    meCommand(player, "has locked the vehicle.");
                    API.setEntitySyncedData(closestveh, "locked", true);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
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
        int indx = getPlayerIndexByName(player.name);
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

    [Command("purchase")]
    public void purchaseFunc(Client player)
    {
        Vector3 plr_pos = API.getEntityPosition(player);
        float smallest_dist = 100.0f;
        int store_index = -1;
        for(int i = 0; i < store_locations.Length; i++)
        {
            float currdist = vecdist(plr_pos, store_locations[i].location);

            if(currdist < smallest_dist)
            {
                smallest_dist = currdist;
                store_index = i;
            }
        }

        if(smallest_dist < 1.0f && store_index != -1)
        {
            API.sendChatMessageToPlayer(player, "store call type: " + store_locations[store_index].store_type_id);
        }
        else
        {
            API.sendChatMessageToPlayer(player, "There is no store nearby!");
        }
    }

    [Command("removeitem", GreedyArg = true)]
    public void removeItemFunc(Client player, string item)
    {
        item = item.ToLower();
        int indx = getPlayerIndexByName(player.name);
        for (int i = 0; i < plr_database[indx].inventory.Count; i++)
        {
            if (plr_database[indx].inventory[i].name == item)
            {
                RandomIDObjectPool.Add(plr_database[indx].inventory[i].id);
                API.sendChatMessageToPlayer(player, "Destroyed object: " + plr_database[indx].inventory[i].id);
                plr_database[indx].inventory.RemoveAt(i);
                break;
            }
        }
    }

    [Command("additem", GreedyArg = true)]
    public void addItemFunc(Client player, string id)
    {
        int indx = getPlayerIndexByName(player.name);
        char[] delimiter = { ' ' };
        string[] words = id.Split(delimiter);

        string id_new;
        string name_new;

        id_new = words[0];
        name_new = words[1];
        name_new = name_new.ToLower();

        ObjectData temp = new ObjectData();
        temp.id = getRandomIDObjectPool();
        temp.name = name_new;
        temp.obj_id = Convert.ToInt32(id_new);
        temp.deletable = true;
        plr_database[indx].inventory.Add(temp);
        API.sendChatMessageToPlayer(player, "Object added to inventory: " + temp.id);
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

    [Command("addmoney", GreedyArg = true)]
    public void addMoneyFunc(Client player, string name_amount)
    {
        char[] delimiter = { ' ' };
        string[] words = name_amount.Split(delimiter);

        int amount;
        string name;

        amount = Convert.ToInt32(words[0]);
        name = words[1];
        name = name.ToLower();

        int indx = getPlayerIndexByName(name);
        if(indx != -1)
        {
            PlayerData plr_temp = plr_database[indx];
            plr_temp.money_in_bank += (ulong)amount;
            plr_database[indx] = plr_temp;
            API.sendChatMessageToPlayer(player, "Money added.");
        }
    }

    [Command("take", "Usage: /take ~b~(object name) (car part)", GreedyArg = true)]
    public void takeFunc(Client player, string action)
    {
        int indx = getPlayerIndexByName(player.name);
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
                            VehicleData temp_vehdata = getVehicleDataByID(API.getEntitySyncedData(closestveh, "id"));
                            for (int i = 0; i < temp_vehdata.inventory.Count; i++)
                            {
                                if (temp_vehdata.inventory[i].name == item_name)
                                {
                                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                                    animFunc(player, "clean", false);
                                    //randevouz
                                    ObjectData temp = temp_vehdata.inventory[i];
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
                                    plr_database[indx].inventory.Add(temp);
                                    temp_vehdata.inventory.RemoveAt(i);
                                    replaceVehicleDataByID(API.getEntitySyncedData(closestveh, "id"), temp_vehdata);
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

    [Command("put", "Usage: /put ~b~(object name) (car part)", GreedyArg = true)]
    public void putFunc(Client player, string action)
    {
        int indx = getPlayerIndexByName(player.name);
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
                            for (int i = 0; i < plr_database[indx].inventory.Count; i++)
                            {
                                if (plr_database[indx].inventory[i].name == item_name)
                                {
                                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                                    animFunc(player, "clean", false);
                                    VehicleData temp_vehdata = getVehicleDataByID(API.getEntitySyncedData(closestveh, "id"));
                                    ObjectData temp = plr_database[indx].inventory[i];
                                    //temp.obj = API.createObject(temp.obj_id, API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player));
                                    //API.setEntityPosition(temp.obj, rotatedVector(API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player).Z + 90.0));
                                    //API.consoleOutput("Player Z ROT: " + API.getEntityRotation(player).Z);
                                    //API.setEntitySyncedData(temp.obj, "del", true);
                                    //API.setEntityPositionFrozen(temp.obj, true);
                                    //API.setEntityCollisionless(temp.obj, true);
                                    //worldObjectPool.Add(temp);


                                    //boot
                                    //API.attachEntityToEntity(temp.obj, temp_vehdata.vehicle_hash, loc_name, new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                                    temp_vehdata.inventory.Add(temp);
                                    replaceVehicleDataByID(API.getEntitySyncedData(closestveh, "id"), temp_vehdata);
                                    plr_database[indx].inventory.RemoveAt(i);
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

    [Command("check", "Usage: /check ~b~(car part)", GreedyArg = true)]
    public void checkFunc(Client player, string action)
    {
        action = action.ToLower();
        int indx = getPlayerIndexByName(player.name);
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
                            VehicleData temp_vehdata = getVehicleDataByID(API.getEntitySyncedData(closestveh, "id"));
                            if (temp_vehdata.inventory.Count == 0)
                            {
                                API.sendChatMessageToPlayer(player, "~y~[EMPTY]");
                            }
                            else
                            {
                                for (int i = 0; i < temp_vehdata.inventory.Count; i++)
                                {
                                    API.sendChatMessageToPlayer(player, "~y~" + temp_vehdata.inventory[i].name);
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

    [Command("inventory")]
    public void inventoryFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
        API.sendChatMessageToPlayer(player, "~y~--Inventory--");
        if (plr_database[indx].inventory.Count == 0)
        {
            API.sendChatMessageToPlayer(player, "~y~[EMPTY]");
        }
        else
        {
            for (int i = 0; i < plr_database[indx].inventory.Count; i++)
            {
                API.sendChatMessageToPlayer(player, "~y~" + plr_database[indx].inventory[i].name);
            }
        }
    }

    [Command("place", "Usage: /place ~b~(object name)", GreedyArg = true)]
    public void spawnConeFunc(Client player, string item)
    {
        item = item.ToLower();
        int indx = getPlayerIndexByName(player.name);
        if (!API.isPlayerInAnyVehicle(player))
        {
            for (int i = 0; i < plr_database[indx].inventory.Count; i++)
            {
                if (plr_database[indx].inventory[i].name == item)
                {
                    //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                    animFunc(player, "checkbody2", false);
                    //int tempid = RandomIDObjectPool[0];
                    // RandomIDObjectPool.RemoveAt(0);
                    ObjectData temp = plr_database[indx].inventory[i];
                    temp.obj = API.createObject(temp.obj_id, API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player));
                    API.setEntityPosition(temp.obj, rotatedVector(API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityPosition(player) - new Vector3(0.0, 0.0, 1.0), API.getEntityRotation(player).Z + 90.0));
                    API.consoleOutput("Player Z ROT: " + API.getEntityRotation(player).Z);
                    API.setEntitySyncedData(temp.obj, "del", true);
                    API.setEntityPositionFrozen(temp.obj, true);
                    API.setEntityCollisionless(temp.obj, true);
                    worldObjectPool.Add(temp);


                    plr_database[indx].inventory.RemoveAt(i);
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
    public void deleteConeFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
        if (!API.isPlayerInAnyVehicle(player))
        {
            float smallestDist = 100.0f;
            ObjectData closestObj = new ObjectData();
            int obj_indx = 0;
            bool found = false;
            for (int i = 0; i < worldObjectPool.Count; i++)
            {
                float vr = vecdist(API.getEntityPosition(worldObjectPool[i].obj), API.getEntityPosition(player));
                if (vr < smallestDist)
                {
                    smallestDist = vr;
                    closestObj = worldObjectPool[i];
                    obj_indx = i;
                    //worldObjectPool.RemoveAt(i);
                    found = true;
                }
            }

            if (found)
            {
                if (smallestDist < 2.5f)
                {
                    bool val = API.getEntitySyncedData(closestObj.obj, "del");
                    if (val == true)
                    {
                        //API.playPlayerAnimation(player, 0, "amb@medic@standing@tendtodead@idle_a", "idle_a");
                        animFunc(player, "checkbody2", false);
                        worldObjectPool.RemoveAt(obj_indx);
                        plr_database[indx].inventory.Add(closestObj);
                        API.deleteEntity(closestObj.obj);
                        API.sendChatMessageToPlayer(player, "Picked up object: " + closestObj.id);
                    }
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "You cannot do that!");
        }
    }

    [Command("unlock")]
    public void unlockFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
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
            if (smallestDist < 2.5f)
            {
                if (getVehicleOwnerNameByID(API.getEntitySyncedData(closestveh, "id")) == plr_database[indx].player_fake_name)
                {
                    API.setVehicleLocked(closestveh, false);
                    // API.sendChatMessageToPlayer(player, "You have ~b~unlocked ~w~the car.");
                    meCommand(player, "has unlocked the vehicle.");
                    API.setEntitySyncedData(closestveh, "locked", false);
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
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

    [Command("engine")]
    public void engineFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
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
                if (getVehicleOwnerNameByID(API.getEntitySyncedData(API.getPlayerVehicle(player), "id")) == plr_database[indx].player_fake_name)
                {
                    API.setVehicleEngineStatus(API.getPlayerVehicle(player), !API.getVehicleEngineStatus(API.getPlayerVehicle(player))); //Just inverse the engine state
                    if (API.getVehicleEngineStatus(API.getPlayerVehicle(player)) == true)
                    {
                        meCommand(player, "has turned the engine ON.");
                    }
                    //API.sendChatMessageToPlayer(player, "You have turned the engine ~g~ON.");
                    else
                    {
                        meCommand(player, "has turned the engine OFF.");
                    }
                    //API.sendChatMessageToPlayer(player, "You have turned the engine ~r~OFF.");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You don't own this vehicle!");
                }
            }
        }
    }

    [Command("open", "Usage: /open ~b~(car part)", GreedyArg = true)]
    public void doorOpenFunc(Client player, string action)
    {
        int indx = getPlayerIndexByName(player.name);
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

    [Command("anim", "Usage: /anim ~b~(name)", GreedyArg = true)]
    public void animFunc(Client player, string action, bool loop = true)
    {
        int indx = getPlayerIndexByName(player.name);
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
                        API.playPlayerAnimation(player, anim_names[i].animation_flag, anim_names[i].anim_dict, anim_names[i].anim_name);
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

    [Command("skin", GreedyArg = true)] //debug
    public void skinFunc(Client player, string action)
    {
        API.setPlayerSkin(player, API.pedNameToModel(action));
    }

    //Close car doors
    [Command("close", "Usage: /close ~b~(car part)", GreedyArg = true)]
    public void doorCloseFunc(Client player, string action)
    {
        int indx = getPlayerIndexByName(player.name);
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

    [Command("spawncar", GreedyArg = true)]
    public void spawnCar(Client player, string carname)
    {
        int indx = getPlayerIndexByName(player.name);
        if (indx != -1)
        {
            API.sendChatMessageToPlayer(player, "Spawned car!");
            Vehicle hash = API.createVehicle(API.vehicleNameToModel(carname), API.getEntityPosition(player), API.getEntityRotation(player), 0, 0);
            //API.setVehicleNumberPlate(hash, "TJM");
            VehicleData temp = new VehicleData(true);
            temp.vehicle_id = getRandomIDVehiclePool();
            temp.vehicle_hash = hash;
            temp.position = API.getEntityPosition(hash);
            temp.rotation = API.getEntityRotation(hash);

            temp.owner_name = plr_database[indx].player_fake_name;
            API.setEntitySyncedData(temp.vehicle_hash, "id", (int)temp.vehicle_id);
            API.setEntitySyncedData(temp.vehicle_hash, "owner", (string)temp.owner_name);
            API.setEntitySyncedData(temp.vehicle_hash, "plate", (string)temp.license_plate);
            API.setEntitySyncedData(temp.vehicle_hash, "indicator_right", false);
            API.setEntitySyncedData(temp.vehicle_hash, "indicator_left", false);
            API.setEntitySyncedData(temp.vehicle_hash, "locked", true);
            API.setVehicleNumberPlate(temp.vehicle_hash, (string)temp.license_plate);
            API.setVehicleEngineStatus(temp.vehicle_hash, false);
            API.setVehicleLocked(temp.vehicle_hash, true);
            plr_database[indx].vehicles.Add(temp);
            PlayerData plr = plr_database[indx];
            plr.vehicles_owned++;
            plr_database[indx] = plr;
        }
    }

    [Command("license", "Usage: /license ~b~(plate #)", GreedyArg = true)]
    public void carinfoFunc(Client player, string license)
    {
        //API.triggerClientEvent(player, "vehicle_draw_text", temp.vehicle_hash, temp.vehicle_id, temp.owner_name, temp.license_plate);
        license = license.ToUpper(); //Get lowercase string for lazy people
        int indx = getPlayerIndexByName(player.name);
        if (plr_database[indx].faction == "police")
        {
            List<NetHandle> vehs = new List<NetHandle>();
            vehs = API.getAllVehicles();
            bool found = false;
            for (int i = 0; i < vehs.Count; i++)
            {
                if (API.getEntitySyncedData(vehs[i], "plate") == license)
                {
                    //Display data about car
                    API.sendChatMessageToPlayer(player, "Vehicle is ~g~registered ~w~in database!");
                    var model = API.getEntityModel(vehs[i]);
                    string plr_nm = getVehicleOwnerNameByID(API.getEntitySyncedData(vehs[i], "id"));
                    int plr_indx = getPlayerIndexByName(plr_nm);
                    int veh_indx = getVehicleIndexByOwnerName(plr_nm, API.getEntitySyncedData(vehs[i], "id"));
                    API.sendChatMessageToPlayer(player, "Model: ~b~" + API.getVehicleDisplayName((VehicleHash)model) + " ~w~-~b~|~w~- Color: ~b~" + plr_database[plr_indx].vehicles[veh_indx].color_name);
                    API.sendChatMessageToPlayer(player, "License Plate: ~b~" + license);
                    API.sendChatMessageToPlayer(player, "Owner: ~b~" + getVehicleOwnerNameByID(API.getEntitySyncedData(vehs[i], "id")));
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

    [Command("stats")]
    public void statsFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
        API.sendChatMessageToPlayer(player, "~b~|-------STATS-------|");
        API.sendChatMessageToPlayer(player, "ID: ~b~" + plr_database[indx].player_id);
        API.sendChatMessageToPlayer(player, "Full Name: ~b~" + plr_database[indx].player_fake_name + "   |   ~w~Faction: ~b~" + plr_database[indx].faction);
        API.sendChatMessageToPlayer(player, "Phone #: ~b~" + plr_database[indx].phone_number);
        API.sendChatMessageToPlayer(player, "Money (Bank): ~b~$" + plr_database[indx].money_in_bank.ToString("N0") + "  |  ~w~Money (Hand): ~b~$" + plr_database[indx].money_in_hand.ToString("N0"));
        API.sendChatMessageToPlayer(player, "Paycheck: ~b~$" + plr_database[indx].pay_check);
        API.sendChatMessageToPlayer(player, "Vehicle(s) Owned: ~b~" + plr_database[indx].vehicles_owned);
        API.sendChatMessageToPlayer(player, "~b~|-------------------|");
    }

    [Command("login", "Usage: /login ~b~(password)", SensitiveInfo = true, GreedyArg = true)]
    public void loginFunc(Client player, string password)
    {
        int indx = getPlayerIndexByName(player.name);
        if (password == plr_database[indx].password)
        {
            API.sendChatMessageToPlayer(player, "Welcome, ~b~" + plr_database[indx].player_fake_name + "~w~!");
            if (indx != -1)
            {
                //Change player data and log him in
                PlayerData plr_temp = plr_database[indx];
                plr_temp.is_logged = true;
                if (plr_temp.data_reset == false)
                {
                    API.setEntityPosition(player, plr_temp.position);
                    API.setEntityRotation(player, plr_temp.rotation);
                    API.setEntityPositionFrozen(player, false);
                    API.setEntityTransparency(player, 255);
                    API.setEntityInvincible(player, false);
                    API.setEntityCollisionless(player, false);
                }
                else
                {
                    plr_temp.data_reset = false;
                    API.setEntityPosition(player, new Vector3(-1034.600, -2733.600, 13.800));
                    API.setEntityPositionFrozen(player, false);
                    API.setEntityTransparency(player, 255);
                    API.setEntityInvincible(player, false);
                    API.setEntityCollisionless(player, false);
                }
                plr_database[indx] = plr_temp;

                for (int i = 0; i < store_locations.Length; i++)
                {
                    API.sendChatMessageToPlayer(player, "store_location_added");
                    API.triggerClientEvent(player, "create_label", store_locations[i].name, store_locations[i].location);
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
                        API.setVehicleDoorState(vehs[i], 5, API.getEntitySyncedData(vehs[i], "trunk"));
                    else
                        API.setVehicleDoorState(vehs[i], 5, false);

                    if (API.getEntitySyncedData(vehs[i], "hood") != null)
                        API.setVehicleDoorState(vehs[i], 4, API.getEntitySyncedData(vehs[i], "hood"));
                    else
                        API.setVehicleDoorState(vehs[i], 4, false);

                    if (API.getEntitySyncedData(vehs[i], "door1") != null)
                        API.setVehicleDoorState(vehs[i], 0, API.getEntitySyncedData(vehs[i], "door1"));
                    else
                        API.setVehicleDoorState(vehs[i], 0, false);

                    if (API.getEntitySyncedData(vehs[i], "door2") != null)
                        API.setVehicleDoorState(vehs[i], 1, API.getEntitySyncedData(vehs[i], "door2"));
                    else
                        API.setVehicleDoorState(vehs[i], 1, false);

                    if (API.getEntitySyncedData(vehs[i], "door3") != null)
                        API.setVehicleDoorState(vehs[i], 2, API.getEntitySyncedData(vehs[i], "door3"));
                    else
                        API.setVehicleDoorState(vehs[i], 2, false);

                    if (API.getEntitySyncedData(vehs[i], "door4") != null)
                        API.setVehicleDoorState(vehs[i], 3, API.getEntitySyncedData(vehs[i], "door4"));
                    else
                        API.setVehicleDoorState(vehs[i], 3, false);
                }
            }
        }
        else
        {
            API.sendChatMessageToPlayer(player, "Wrong credentials!");
        }
    }

    [Command("register", "Usage: /register ~b~(password)", SensitiveInfo = true, GreedyArg = true)]
    public void registerFunc(Client player, string password)
    {
        int indx = getPlayerIndexByName(player.name);
        PlayerData plr_temp = plr_database[indx];
        plr_temp.password = password;
        plr_temp.is_registered = true;
        plr_database[indx] = plr_temp;
        API.sendChatMessageToPlayer(player, "You have been registered! Use /login ~b~(password) ~w~to login.");
    }

    //

    [Command("logout")]
    public void logoutFunc(Client player)
    {
        int indx = getPlayerIndexByName(player.name);
        //Log user out, don't recycle ID yet.
        API.stopPlayerAnimation(player);
        NetHandle temp = new NetHandle();
        if (API.getEntitySyncedData(player, "anim_obj") != null)
            temp = API.getEntitySyncedData(player, "anim_obj");
        API.deleteEntity(temp);

        API.sendChatMessageToPlayer(player, "You have been logged out!");
        PlayerData plr_temp = plr_database[indx];
        plr_temp.position = API.getEntityPosition(player);
        plr_temp.rotation = API.getEntityRotation(player);
        plr_temp.is_logged = false;
        plr_database[indx] = plr_temp;
        Random rnd = new Random();
        int temprnd = rnd.Next(0, loginscreen_locations.Length);
        API.setEntityPosition(player, loginscreen_locations[temprnd]);
        API.setEntityPositionFrozen(player, true);
        API.setEntityTransparency(player, 0);
        API.setEntityInvincible(player, true);
        API.setEntityCollisionless(player, true);
        API.triggerClientEvent(player, "delete_all_labels");

        
    }
}
