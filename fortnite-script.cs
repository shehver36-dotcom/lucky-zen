using SoftwareZen;
using System;

public class FortniteScript : GameScript {
    private int _turboFrame = 0;
    private int _editResetFrame = 0;
    private bool _editResetActive = false;
    private int _aaWobbleFrame = 0;
    private int _aaWobbleDir = 1;
    private int _quickScopeFrame = 0;
    private bool _quickScopeActive = false;

    public override void OnFrame() {
        // Turbo build
        if (btn_held("XB1_B") && mod_enabled("turbo_build")) {
            _turboFrame++;
            int interval = cfg_get_int("turbo_interval", 50) / 2;
            int period = Math.Max(interval, 1);
            bool fire = (_turboFrame % (period * 2)) < period;
            btn_set("XB1_RT", fire ? 100 : 0);
        } else {
            _turboFrame = 0;
        }

        // Double edit reset
        if (btn_pressed("XB1_X") && mod_enabled("edit_reset")) {
            _editResetActive = true;
            _editResetFrame = 0;
        }
        if (_editResetActive) {
            _editResetFrame++;
            if (_editResetFrame == 1) btn_set("XB1_B", 100);    // edit
            if (_editResetFrame == 3) btn_set("XB1_B", 0);
            if (_editResetFrame == 4) btn_set("XB1_RT", 100);   // reset
            if (_editResetFrame == 6) btn_set("XB1_RT", 0);
            if (_editResetFrame == 7) btn_set("XB1_B", 100);    // confirm
            if (_editResetFrame == 9) { btn_set("XB1_B", 0); _editResetActive = false; }
        }

        // Aim assist
        if (btn_held("XB1_LT") && mod_enabled("aim_assist")) {
            int strength = cfg_get_int("fortnite_aa_strength", 3);
            _aaWobbleFrame++;
            if (_aaWobbleFrame > 8) { _aaWobbleFrame = 0; _aaWobbleDir *= -1; }
            stick_set("RIGHT", stick_x("RIGHT") + _aaWobbleDir * strength * 400, stick_y("RIGHT"));
        }

        // Quick scope
        if (btn_pressed("XB1_LT") && mod_enabled("quick_scope")) {
            _quickScopeActive = true;
            _quickScopeFrame = 0;
        }
        if (_quickScopeActive) {
            _quickScopeFrame++;
            if (_quickScopeFrame == 1) btn_set("XB1_LT", 100);
            if (_quickScopeFrame == 4) btn_set("XB1_RT", 100);
            if (_quickScopeFrame == 5) btn_set("XB1_RT", 0);
            if (_quickScopeFrame == 7) { btn_set("XB1_LT", 0); _quickScopeActive = false; }
        }
    }
}
