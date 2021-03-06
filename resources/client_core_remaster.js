﻿
var anim_menu = API.createMenu("Animation List", "Category", 0, 0, 6);
var anim_sub_menu = API.createMenu("-null_category-", "Name(s): ", 0, 0, 6);
var catalog_menu = API.createMenu("-null_catalog-", "-null_types-", 0, 0, 6);
var confirm_menu = API.createMenu("Purchase", "Confirm purchase:", 0, 0, 6);
var showroom_menu = API.createMenu("Vehicle Control", "Part(s): ", 0, 0, 3);
anim_menu.ResetKey(menuControl.Back);
catalog_menu.ResetKey(menuControl.Back);
anim_sub_menu.ResetKey(menuControl.Back);
confirm_menu.ResetKey(menuControl.Back);
showroom_menu.ResetKey(menuControl.Back);

anim_menu.AddItem(API.createMenuItem("Sitting", "Name(s): sit."));
anim_menu.AddItem(API.createMenuItem("Standing", "Name(s): clean, clipboard1, clipboard2."));
anim_menu.AddItem(API.createMenuItem("Phone", "Name(s): phone1, phone2, phone3, phone4."));
anim_menu.AddItem(API.createMenuItem("Ground", "Name(s): checkbody1, checkbody2."));
anim_menu.AddItem(API.createMenuItem("Leaning", "Name(s): lean, leanfoot, leancar."));
anim_menu.AddItem(API.createMenuItem("Surrender", "Name(s): handsup, handsupknees."));
anim_menu.AddItem(API.createMenuItem("Smoking", "Name(s): smoke1, smoke2."));
anim_menu.AddItem(API.createMenuItem("Drinking", "Name(s): coffee1, coffee2."));
anim_menu.AddItem(API.createMenuItem("Social", "Name(s): guitar, drums."));
anim_menu.AddItem(API.createMenuItem("Stop", "Stop playing animation."));
anim_menu.AddItem(API.createMenuItem("Exit", "Close menu."));

showroom_menu.AddItem(API.createMenuItem("Hood", "Open/close Hood."));
showroom_menu.AddItem(API.createMenuItem("Trunk", "Open/close Trunk."));
showroom_menu.AddItem(API.createMenuItem("Door1", "Open/close Door1."));
showroom_menu.AddItem(API.createMenuItem("Door2", "Open/close Door2."));
showroom_menu.AddItem(API.createMenuItem("Door3", "Open/close Door3."));
showroom_menu.AddItem(API.createMenuItem("Door4", "Open/close Door4."));
showroom_menu.AddItem(API.createMenuItem("Exit", "Close menu."));

var dealership_cars = [];

dealership_cars.push({ name: "Karin Beejay XL", price: 45000, dealership: "dealership_1", veh_name: "bjxl" });
dealership_cars.push({ name: "Obey Tailgater", price: 55000, dealership: "dealership_1", veh_name: "tailgater" });
dealership_cars.push({ name: "Benefactor Feltzer", price: 60000, dealership: "dealership_1", veh_name: "Feltzer2" });
dealership_cars.push({ name: "Grotti Carbonizzare", price: 235000, dealership: "dealership_1", veh_name: "carbonizzare" });
dealership_cars.push({ name: "Obey 9F", price: 195000, dealership: "dealership_1", veh_name: "ninef" });
dealership_cars.push({ name: "Benefactor Dubsta", price: 110000, dealership: "dealership_1", veh_name: "dubsta" });

var text_labels = [];

function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,")
}

var oldcam = API.getActiveCamera();
var inside_car_showroom = false;

function confirmButton(name, price, veh_name)
{
    inside_car_showroom = true;
    var vec3 = new Vector3(-43.59027, -1098.547, 26.00982);
    var rot3 = new Vector3(-0.06739808, -0.004466934, 91.13767);
    //var cam3 = new Vector3(0.0, 0.0, 0.0);
    API.triggerServerEvent("spawn_car_diff_dim", veh_name, vec3, rot3);
    //dealership_1_display_car = API.createVehicle(API.vehicleNameToModel(veh_name), vec3, 0.0);
    //API.setEntityRotation(dealership_1_display_car, rot3);
    //API.setEntityPositionFrozen(dealership_1_display_car, true);
    //API.setPlayerIntoVehicle(dealership_1_display_car, -1);
    API.setGameplayCameraActive();
    //API.setCameraRotation(API.getActiveCamera(), cam3);
    /*oldcam = API.getActiveCamera();
    var newcam = API.createCamera(API.getEntityPosition(API.getLocalPlayer()), API.getEntityRotation(API.getLocalPlayer()));
    API.setActiveCamera(newcam);

    var offset = new Vector3(3.0, -5.0, 2.0);
    API.attachCameraToEntity(newcam, dealership_1_display_car, offset);
    var offset2 = new Vector3(0.0, 0.0, 0.0);
    API.pointCameraAtEntity(newcam, dealership_1_display_car, offset2);*/
    catalog_menu.Visible = false;
    confirm_menu.Clear();
    confirm_menu.AddItem(API.createMenuItem("Purchase for $" + formatNumber(price) + "?", name));
    confirm_menu.AddItem(API.createMenuItem("Vehicle Control", "Open/close vehicle parts."));
    confirm_menu.AddItem(API.createMenuItem("Cancel", "Cancel purchase."));
    confirm_menu.Visible = true;

}

confirm_menu.OnItemSelect.connect(function (sender, item, index) {
    if(item.Text === "Cancel")
    {
        confirm_menu.Visible = false;
        catalog_menu.Visible = true;
        API.triggerServerEvent("delete_car");
        //API.deleteEntity(dealership_1_display_car);
        //var newpos = new Vector3(-61.70732, -1093.239, 25.5);
        //API.setEntityPosition(API.getLocalPlayer(), newpos);
        //API.setActiveCamera(oldcam);
        inside_car_showroom = false;
    }
    else if(item.Text === "Vehicle Control")
    {
        showroom_menu.Visible = true;
        confirm_menu.Visible = false;
    }
    else
    {
        for (var i = 0; i < dealership_cars.length; i++) {
            if (dealership_cars[i].name === confirm_menu.MenuItems[index].Description) {
                API.triggerServerEvent("delete_car");
                API.triggerServerEvent("purchase_car", dealership_cars[i].veh_name, dealership_cars[i].price, dealership_cars[i].name, dealership_cars[i].dealership);
                //API.setEntityPosition(API.getLocalPlayer(), newpos);
                //API.setActiveCamera(oldcam);
                inside_car_showroom = false;
                break;
            }
        }
        confirm_menu.Visible = false;
    }
});

showroom_menu.OnItemSelect.connect(function (sender, item, index) {
    if(item.Text === "Exit")
    {
        confirm_menu.Visible = true;
        showroom_menu.Visible = false;
    }
    else
    {
        if(item.Text === "Hood")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 4, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 4));
        }
        else if(item.Text === "Trunk")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 5, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 5));
        }
        else if(item.Text === "Door1")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 0, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 0));
        }
        else if(item.Text === "Door2")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 1, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 1));
        }
        else if(item.Text === "Door3")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 2, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 2));
        }
        else if(item.Text === "Door4")
        {
            API.setVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 3, !API.getVehicleDoorState(API.getPlayerVehicle(API.getLocalPlayer()), 3));
        }
    }
});

catalog_menu.OnItemSelect.connect(function (sender, item, index) {
    if(item.Text == "Exit")
    {
        catalog_menu.Visible = false;
    }
    else
    {
        for (var i = 0; i < dealership_cars.length; i++) {
            if (dealership_cars[i].name === item.Text) {
                confirmButton(dealership_cars[i].name, dealership_cars[i].price, dealership_cars[i].veh_name);
                break;
            }
        }
    }
});

anim_menu.OnItemSelect.connect(function (sender, item, index) {
    if (item.Text === "Exit") {
        anim_menu.Visible = false;
    }
    else if (item.Text === "Stop") {
        API.triggerServerEvent("do_anim", "stop");
    }
    else {
        anim_menu.Visible = false;
        API.setMenuTitle(anim_sub_menu, item.Text);
        anim_sub_menu.Clear();
        if (item.Text === "Sitting") {
            anim_sub_menu.AddItem(API.createMenuItem("sit", "Animation"));
        }
        else if (item.Text === "Standing") {
            anim_sub_menu.AddItem(API.createMenuItem("clean", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("clipboard1", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("clipboard2", "Animation"));
        }
        else if (item.Text === "Phone") {
            anim_sub_menu.AddItem(API.createMenuItem("phone1", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("phone2", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("phone3", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("phone4", "Animation"));
        }
        else if (item.Text === "Ground") {
            anim_sub_menu.AddItem(API.createMenuItem("checkbody1", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("checkbody2", "Animation"));
        }
        else if (item.Text === "Leaning") {
            anim_sub_menu.AddItem(API.createMenuItem("lean", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("leanfoot", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("leancar", "Animation"));
        }
        else if (item.Text === "Surrender") {
            anim_sub_menu.AddItem(API.createMenuItem("handsup", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("handsupknees", "Animation"));
        }
        else if (item.Text === "Smoking") {
            anim_sub_menu.AddItem(API.createMenuItem("smoke1", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("smoke2", "Animation"));
        }
        else if (item.Text === "Drinking") {
            anim_sub_menu.AddItem(API.createMenuItem("coffee1", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("coffee2", "Animation"));
        }
        else if (item.Text === "Social") {
            anim_sub_menu.AddItem(API.createMenuItem("guitar", "Animation"));
            anim_sub_menu.AddItem(API.createMenuItem("drums", "Animation"));
        }
        anim_sub_menu.AddItem(API.createMenuItem("Stop", "Animation"));
        anim_sub_menu.AddItem(API.createMenuItem("Exit", "Animation"));
        anim_sub_menu.Visible = true;
    }
});

anim_sub_menu.OnItemSelect.connect(function (sender, item, index) {
    if (item.Text === "Exit") {
        anim_sub_menu.Visible = false;
        anim_menu.Visible = true;
    }
    else if (item.Text === "Stop") {
        API.triggerServerEvent("do_anim", "stop");
    }
    else {
        API.triggerServerEvent("do_anim", item.Text);
    }
});

var mainBrowser = null;
var res = API.getScreenResolution();

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case 'anim_list':
            API.sendChatMessage("anim_list_call");
            //API.showCursor(true);
            anim_menu.Visible = true;
            catalog_menu.Visible = false;
            break;
        case 'catalog_list':
            API.sendChatMessage("catalog_list_call");
            catalog_menu.Clear();
            API.setMenuTitle(catalog_menu, args[0]);
            catalog_menu.Title.Scale = 0.75;
            API.setMenuSubtitle(catalog_menu, args[1]);

            for (var i = 0; i < dealership_cars.length; i++)
            {
                if(dealership_cars[i].dealership === args[2])
                {
                    catalog_menu.AddItem(API.createMenuItem(dealership_cars[i].name, "$" + formatNumber(dealership_cars[i].price)));
                }
            }

            catalog_menu.AddItem(API.createMenuItem("Exit", "Exit dealership list."));

            catalog_menu.Visible = true;
            anim_menu.Visible = false;
            anim_sub_menu.Visible = false;
            break;
        case 'create_label':
            var label = API.createTextLabel(args[0], args[1], 10.0, 1.0);
            text_labels.push(label);
            break;
        case 'delete_all_labels':
            var length = text_labels.length;
            for (var i = 0; i < length; i++)
                API.deleteEntity(text_labels[i]);
            text_labels = [];
            break;
        case 'sync_vehicle_door_state':
            API.setVehicleDoorState(args[1], args[0], args[2]);
            break;
        case 'sync_vehicle_door_damage':
            if (args[2] === true)
                API.callNative('0xD4D4F6A4AB575A33', args[1], args[0], true);
            break;
        case 'atm_list':
            exitBrowser();
            API.sendChatMessage("cef_atm_call");
            mainBrowser = API.createCefBrowser(900.0, 500.0, true);
            API.waitUntilCefBrowserInit(mainBrowser);
            API.sendChatMessage("RES Y: " + res.Height);
            API.setCefBrowserPosition(mainBrowser, res.Width / 2.0 - (900.0) / 2.0, res.Height / 2.0 - (500.0) / 2.0);
            API.loadPageCefBrowser(mainBrowser, "bankpinlogin.html");
            API.showCursor(true);

            break;
        case 'bank_list':
            exitBrowser();
            API.sendChatMessage("cef_bank_call");
            mainBrowser = API.createCefBrowser(800.0, 575.0, true);
            API.waitUntilCefBrowserInit(mainBrowser);
            API.sendChatMessage("RES Y: " + res.Height);
            API.setCefBrowserPosition(mainBrowser, res.Width / 2.0 - (800.0) / 2.0, res.Height / 2.0 - (575.0) / 2.0);
            API.loadPageCefBrowser(mainBrowser, "banklogin.html");
            API.showCursor(true);
            API.sleep(100);
            API.triggerServerEvent("getBankInfo");

            break;
        case 'close_cef':
            if (mainBrowser !== null)
                API.destroyCefBrowser(mainBrowser);
            mainBrowser = null;
            API.showCursor(false);
            break;
        case 'failed_pin':
            if (mainBrowser !== null)
                mainBrowser.call("failpin");
            break;
        case 'success_pin':
            if (mainBrowser !== null)
                mainBrowser.call("successpin");
                loginBank();
            break;
        case 'pulled_bankdata':
            API.sendChatMessage("pulleddata");
            API.sendChatMessage(args[0]);
            API.sendChatMessage(args[1]);
            applyBankData(args[0], args[1]);
            break;
        case 'update_sum':
            API.sendChatMessage("updatedsum");
            API.sendChatMessage(args[0]);
            API.sendChatMessage(args[1]);
            applyBankData(args[0], args[1]);
            break;
        case 'bring_phone':
            exitBrowser();
            API.sendChatMessage("cef_phone_call");
            mainBrowser = API.createCefBrowser(200.0, 392.0, true);
            API.waitUntilCefBrowserInit(mainBrowser);
            API.sendChatMessage("RES Y: " + res.Height);
            API.setCefBrowserPosition(mainBrowser, res.Width * (2/3) - (200.0), res.Height - (392.0));
            API.loadPageCefBrowser(mainBrowser, "phonehtml.html");
            API.showCursor(true);
            API.sleep(100);
            API.triggerServerEvent("play_phone_anim");
            break;
    }
});

var call_midair = false;
var max_height = 0.0;

var oldTime = API.getGameTime();
var isClosing = true;

var outofcontrol_called = false;
var overridecontrol_called = false;
var is_phone_on = false;

API.onUpdate.connect(function () {
    API.drawMenu(anim_menu);
    API.drawMenu(anim_sub_menu);
    API.drawMenu(catalog_menu);
    API.drawMenu(confirm_menu);
    API.drawMenu(showroom_menu);
    if (is_phone_on && mainBrowser !== null)
    {
        //var str = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate() + " " + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
        var d = new Date();
        mainBrowser.call("updateTime", pad(d.getHours()) + ":" + pad(d.getMinutes()) + ":" + pad(d.getSeconds()) + "");
    }

    if (API.isPlayerInAnyVehicle(API.getLocalPlayer()) === true) {
        var is_onroof_stuck = API.returnNative("IS_VEHICLE_STUCK_ON_ROOF", 8, API.getPlayerVehicle(API.getLocalPlayer()));
        var is_veh_allwheels = API.returnNative("IS_VEHICLE_ON_ALL_WHEELS", 8, API.getPlayerVehicle(API.getLocalPlayer()));
        if (is_onroof_stuck === true) {
            API.callNative("SET_VEHICLE_OUT_OF_CONTROL", API.getPlayerVehicle(API.getLocalPlayer()), false, false);
            outofcontrol_called = true;
        }
        else if (is_veh_allwheels === false) {
            API.callNative("SET_VEHICLE_OUT_OF_CONTROL", API.getPlayerVehicle(API.getLocalPlayer()), false, false);
            outofcontrol_called = true;
            var newheight = API.returnNative("GET_ENTITY_HEIGHT_ABOVE_GROUND", 7, API.getPlayerVehicle(API.getLocalPlayer()));
            if (newheight > max_height) {
                max_height = newheight;
            }
            else {
                call_midair = true;
            }
        }

        if (is_veh_allwheels === true && call_midair === true) {
            var currheight = API.returnNative("GET_ENTITY_HEIGHT_ABOVE_GROUND", 7, API.getPlayerVehicle(API.getLocalPlayer()));
            var diff = Math.abs(max_height - currheight);
            if (diff > 2.0) {
                API.triggerServerEvent("explode");
            }
            API.sendChatMessage("Difference: " + diff);
            max_height = 0.0;
            call_midair = false;
        }

        if (outofcontrol_called && is_veh_allwheels && overridecontrol_called == false)
        {
            outofcontrol_called = false;
            API.setPlayerIntoVehicle(API.getPlayerVehicle(API.getLocalPlayer()), -1);
        }
    }
});


API.onKeyUp.connect(function (sender, e) {
    if(!API.isChatOpen())
    {
        if (API.isPlayerInAnyVehicle(API.getLocalPlayer())) {
            if (API.getPlayerVehicleSeat(API.getLocalPlayer()) == -1) {
                if (e.KeyCode === Keys.J) {
                    API.triggerServerEvent("indicator_left");
                }
                else if (e.KeyCode === Keys.K) {
                    API.triggerServerEvent("indicator_right");
                }
                else if (e.KeyCode === Keys.F && inside_car_showroom === true) {
                    confirm_menu.Visible = false;
                    catalog_menu.Visible = true;
                    API.triggerServerEvent("delete_car");
                    var newpos = new Vector3(-61.70732, -1093.239, 25.5);
                    API.triggerServerEvent("set_player_pos", newpos);
                    inside_car_showroom = false;
                }
            }
        }
    }
    if (e.KeyCode === Keys.M) {
        if (mainBrowser !== null)
            API.destroyCefBrowser(mainBrowser);
        mainBrowser = null;
        API.showCursor(false);
    }
    if(e.KeyCode === Keys.Up && is_phone_on === false)
    {
        is_phone_on = true;
        exitBrowser();
        API.sendChatMessage("cef_phone_call");
        mainBrowser = API.createCefBrowser(200.0, 392.0, true);
        API.waitUntilCefBrowserInit(mainBrowser);
        API.sendChatMessage("RES Y: " + res.Height);
        API.setCefBrowserPosition(mainBrowser, res.Width * (2 / 3) - (200.0), res.Height - (392.0));
        API.loadPageCefBrowser(mainBrowser, "phonehtml.html");
        API.showCursor(true);
        API.sleep(100);
        API.triggerServerEvent("play_phone_anim");
    }
    else if (e.KeyCode === Keys.Down && is_phone_on === true)
    {
        is_phone_on = false;
        if (mainBrowser !== null)
            mainBrowser.call("ClosePhone");
        API.showCursor(false);
        API.triggerServerEvent("stop_phone_anim");
        API.sleep(750);
        exitBrowser();
        API.sleep(100);
    }
});

API.onKeyDown.connect(function (sender, e) {
    if(!API.isChatOpen())
    {
        if(API.isPlayerInAnyVehicle(API.getLocalPlayer()))
        {
            if(API.getPlayerVehicleSeat(API.getLocalPlayer()) == -1)
            {
                if(e.KeyCode === Keys.F)
                {
                    overridecontrol_called = true;
                }
            }
        }
        else if(overridecontrol_called)
        {
            overridecontrol_called = false;
        }
    }
});

function pad(d) {
    return (d < 10) ? '0' + d.toString() : d.toString();
}

function checkPinNumber(val)
{
    API.sendChatMessage("Pin Detected: " + val);
    API.triggerServerEvent("check_bank_pin", val);
}

function exitBrowser()
{
    if (mainBrowser !== null)
        API.destroyCefBrowser(mainBrowser);
    mainBrowser = null;
    API.showCursor(false);
}

function loginBank()
{
    API.triggerServerEvent("fetch_bankdata");
}

function applyBankData(name, sum)
{
    if (mainBrowser !== null)
    {
        mainBrowser.call("changeName", name + "", sum + "");
    }
}

function depositAmount(amount)
{
    if (amount !== "null" && amount !== null)
    {
        API.triggerServerEvent("depositThis", amount);
    }
}

function withdrawAmount(amount)
{
    if (amount !== "null" && amount !== null) {
        API.triggerServerEvent("withdrawThis", amount);
    }
}

function limitAmount(amount)
{
    if (amount !== "null" && amount !== null) {
        API.triggerServerEvent("limitThis", amount);
    }
}

function phoneEventHandler(strn)
{
    API.sendChatMessage("phone_app_call: " + strn);
}