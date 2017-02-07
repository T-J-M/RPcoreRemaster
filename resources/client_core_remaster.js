var anim_menu = API.createMenu("Animation List", "Category", 0, 0, 6);
var anim_sub_menu = API.createMenu("-null_category-", "Name(s)", 0, 0, 6);
anim_menu.ResetKey(menuControl.Back);
anim_sub_menu.ResetKey(menuControl.Back);

anim_menu.AddItem(API.createMenuItem("Sitting", "Name(s): sit"));
anim_menu.AddItem(API.createMenuItem("Standing", "Name(s): clean, clipboard1, clipboard2"));
anim_menu.AddItem(API.createMenuItem("Phone", "Name(s): phone1, phone2, phone3"));
anim_menu.AddItem(API.createMenuItem("Ground", "Name(s): checkbody1, checkbody2"));
anim_menu.AddItem(API.createMenuItem("Leaning", "Name(s): lean, leanfoot, leancar"));
anim_menu.AddItem(API.createMenuItem("Surrender", "Name(s): handsup, handsupknees"));
anim_menu.AddItem(API.createMenuItem("Smoking", "Name(s): smoke1, smoke2"));
anim_menu.AddItem(API.createMenuItem("Drinking", "Name(s): coffee1, coffee2"));
anim_menu.AddItem(API.createMenuItem("Social", "Name(s): guitar, drums"));
anim_menu.AddItem(API.createMenuItem("Stop", "Stop playing animation"));
anim_menu.AddItem(API.createMenuItem("Exit", "Close menu"));

var text_labels = [];

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
            break;
        case 'cef_test':
            API.sendChatMessage("cef_test_call");
            mainBrowser = API.createCefBrowser(1000.0, 500.0, true);
            API.waitUntilCefBrowserInit(mainBrowser);
            API.sendChatMessage("RES Y: " + res.Height);
            API.setCefBrowserPosition(mainBrowser, res.Width / 2.0 - (1000.0) / 2.0, res.Height / 2.0 - (500.0) / 2.0);
            API.loadPageCefBrowser(mainBrowser, "bankpin.html");
            API.showCursor(true);
            break;
        case 'create_label':
            text_labels.push(API.createTextLabel(args[0], args[1], 10.0, 1.0));
    }
});

API.onUpdate.connect(function () {
    API.drawMenu(anim_menu);
    API.drawMenu(anim_sub_menu);

    if (API.isPlayerInAnyVehicle(API.getLocalPlayer()) === true) {
        var is_onroof_stuck = API.returnNative("IS_VEHICLE_STUCK_ON_ROOF", 8, API.getPlayerVehicle(API.getLocalPlayer()));
        var is_veh_allwheels = API.returnNative("IS_VEHICLE_ON_ALL_WHEELS", 8, API.getPlayerVehicle(API.getLocalPlayer()));
        if (is_onroof_stuck === true) {
            API.callNative("SET_VEHICLE_OUT_OF_CONTROL", API.getPlayerVehicle(API.getLocalPlayer()), false, false);
        }
        else if (is_veh_allwheels === false) {
            API.callNative("SET_VEHICLE_OUT_OF_CONTROL", API.getPlayerVehicle(API.getLocalPlayer()), false, false);
        }
    }
});

API.onKeyUp.connect(function (sender, e) {
    if (e.KeyCode === Keys.J) {
        API.triggerServerEvent("indicator_left");
    }
    else if (e.KeyCode === Keys.K) {
        API.triggerServerEvent("indicator_right");
    }
    else if (e.KeyCode === Keys.Escape) {
        if (mainBrowser !== null)
            API.destroyCefBrowser(mainBrowser);
        mainBrowser = null;
        API.showCursor(false);
    }
});