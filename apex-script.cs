using SoftwareZen;
using System;

public class ApexScript : GameScript {
    private int _rapidFireFrame = 0;
    private int _superglideFrame = 0;
    private bool _superglideActive = false;
    private int _crouchSpamFrame = 0;
    private bool _crouchSpamActive = false;

    public override void OnFrame() {
        // Rapid Fire
        if (btn_held("XB1_RT") && mod_enabled("rapid_fire")) {
            _rapidFireFrame++;
            int interval = cfg_get_int("apex_rapidfire_ms", 55) / 2;
            int period = Math.Max(interval, 1);
            bool fire = (_rapidFireFrame % (period * 2)) < period;
            btn_set("XB1_RT", fire ? 100 : 0);
        } else {
            _rapidFireFrame = 0;
        }

        // Anti-Recoil — activates while ADS (LT held)
        if (btn_held("XB1_LT") && mod_enabled("anti_recoil")) {
            int strength = cfg_get_int("apex_recoil_strength", 10);
            int ry = stick_y("RIGHT") + strength * 280;
            stick_set("RIGHT", stick_x("RIGHT"), ry);
        }

        // Superglide: frame-perfect crouch+jump on mantle
        if (btn_pressed("XB1_A") && mod_enabled("superglide")) {
            _superglideActive = true;
            _superglideFrame = 0;
        }
        if (_superglideActive) {
            _superglideFrame++;
            int timing = cfg_get_int("superglide_timing", 28) / 2;
            if (_superglideFrame == 1) btn_set("XB1_B", 100);
            if (_superglideFrame == timing) btn_set("XB1_A", 100);
            if (_superglideFrame == timing + 3) {
                btn_set("XB1_B", 0);
                btn_set("XB1_A", 0);
                _superglideActive = false;
            }
        }

        // Crouch spam on reload
        if (btn_held("XB1_X") && mod_enabled("crouch_spam")) {
            if (!_crouchSpamActive) { _crouchSpamActive = true; _crouchSpamFrame = 0; }
        }
        if (_crouchSpamActive && !btn_held("XB1_X")) {
            _crouchSpamActive = false;
            btn_set("XB1_B", 0);
        }
        if (_crouchSpamActive) {
            _crouchSpamFrame++;
            bool down = (_crouchSpamFrame % 5) < 3;
            btn_set("XB1_B", down ? 100 : 0);
            if (_crouchSpamFrame > 50) { _crouchSpamActive = false; btn_set("XB1_B", 0); }
        }
    }
}
