using SoftwareZen;
using System;

public class CodScript : GameScript {
    private int _rapidFireFrame = 0;
    private int _aaWobbleFrame = 0;
    private int _aaWobbleDir = 1;

    public override void OnFrame() {
        // Rapid Fire
        if (btn_held("XB1_RT") && mod_enabled("rapid_fire")) {
            _rapidFireFrame++;
            int interval = cfg_get_int("rapid_fire_interval", 40) / 2;
            int period = Math.Max(interval, 1);
            bool fire = (_rapidFireFrame % (period * 2)) < period;
            btn_set("XB1_RT", fire ? 100 : 0);
        } else {
            _rapidFireFrame = 0;
        }

        // Anti-Recoil
        if (btn_held("XB1_RT") && mod_enabled("anti_recoil")) {
            int strength = cfg_get_int("anti_recoil_strength", 12);
            stick_set("RIGHT", stick_x("RIGHT"), stick_y("RIGHT") + strength * 256);
        }

        // Aim Assist wobble
        if (btn_held("XB1_LT") && mod_enabled("aim_assist")) {
            int strength = cfg_get_int("aim_assist_strength", 5);
            _aaWobbleFrame++;
            if (_aaWobbleFrame > 8) { _aaWobbleFrame = 0; _aaWobbleDir *= -1; }
            stick_set("RIGHT", stick_x("RIGHT") + _aaWobbleDir * strength * 512, stick_y("RIGHT"));
        }

        // Drop Shot
        if (btn_held("XB1_RT") && btn_pressed("XB1_B") && mod_enabled("drop_shot")) {
            btn_set("XB1_B", 100);
            SetVar("ds_frame", 0);
        }
        if (Var<int>("ds_frame") < 6 && btn_held("XB1_B")) {
            SetVar("ds_frame", Var<int>("ds_frame") + 1);
            if (Var<int>("ds_frame") == 6) btn_set("XB1_B", 0);
        }

        // Auto-Sprint
        if (mod_enabled("auto_sprint") && stick_y("LEFT") > 20000 && !btn_held("XB1_LS")) {
            btn_set("XB1_LS", 100);
        }
    }
}
