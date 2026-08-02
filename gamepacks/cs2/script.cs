using SoftwareZen;
using System;

public class CS2Script : GameScript {
    // CS2 — recoil patterns, rapid fire, bunny hop, quick switch
    private int _recoilFrames = 0;
    private int _bhopFrames = 0;
    private bool _bhopActive = false;

    public override void OnFrame() {
        // Rapid Fire for pistols (USP, Glock, Deagle)
        if (btn_held("XB1_RT") && mod_enabled("rapid_fire")) {
            int interval = cfg_get_int("cs2_rapid_ms", 60) / 2;
            _recoilFrames++;
            bool fire = (_recoilFrames % Math.Max(interval * 2, 2)) < interval;
            btn_set("XB1_RT", fire ? 100 : 0);
        }

        // Recoil Control — weapon-specific patterns
        if (btn_held("XB1_RT") && mod_enabled("anti_recoil")) {
            // AK-47 pattern: pull down + slight right
            int vert = cfg_get_int("cs2_recoil_v", 15);
            int horiz = cfg_get_int("cs2_recoil_h", 3);
            int rx = stick_x("RIGHT") + horiz * 80;
            int ry = stick_y("RIGHT") + vert * 300;
            _recoilFrames++;
            if (_recoilFrames > 15) {
                // After initial burst, horizontal compensation kicks in
                rx += (_recoilFrames - 15) * 15;
            }
            stick_set("RIGHT", rx, ry);
        } else { _recoilFrames = 0; }

        // Bunny Hop — hold jump to auto-bhop
        if (btn_held("XB1_A") && mod_enabled("bunny_hop")) {
            _bhopFrames++;
            int bhopTiming = cfg_get_int("cs2_bhop_timing", 20);
            if (_bhopFrames % bhopTiming < bhopTiming / 2) {
                btn_set("XB1_A", 100);
                // Strafe assist — slight left/right alternating
                int strafeDir = ((_bhopFrames / bhopTiming) % 2 == 0) ? 1 : -1;
                stick_set("LEFT", strafeDir * 15000, stick_y("LEFT"));
            } else {
                btn_set("XB1_A", 0);
            }
        } else { _bhopFrames = 0; }

        // Quick Switch — YY to cancel animation (AWP/scout)
        if (btn_pressed("XB1_Y") && mod_enabled("quick_switch")) {
            btn_set("XB1_Y", 0);
        }
        // Double-tap Y for quick switch
        if (btn_pressed("XB1_Y") && btn_held("XB1_Y")) {
            // Hold Y briefly then release
        }

        // Counter-strafe assist — auto-tap opposite direction when releasing strafe
        if (mod_enabled("counter_strafe")) {
            bool wasMovingLeft = stick_x("LEFT") < -16000;
            bool wasMovingRight = stick_x("LEFT") > 16000;
            if (wasMovingLeft && Math.Abs(stick_x("LEFT")) < 5000) {
                // Just stopped moving left — tap right
                btn_set("XB1_DPAD_RIGHT", 100);
            }
            if (wasMovingRight && Math.Abs(stick_x("LEFT")) < 5000) {
                btn_set("XB1_DPAD_LEFT", 100);
            }
        }

        // Auto-scope after switching to AWP
        if (btn_held("XB1_LT") && mod_enabled("auto_scope")) {
            // Hold scope
        }
    }
}
