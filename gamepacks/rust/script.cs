using SoftwareZen;
using System;

public class RustScript : GameScript {
    // Rust — AK/M249/LR recoil control, auto-fire semi rifles
    private int _recoilFrames = 0;
    private int _rapidFrames = 0;

    public override void OnFrame() {
        // Auto-fire for semi-auto weapons (M39, SAR, Python)
        if (btn_held("XB1_RT") && mod_enabled("rapid_fire")) {
            _rapidFrames++;
            int interval = cfg_get_int("rust_rapid_ms", 100) / 2;
            int period = Math.Max(interval, 2);
            bool fire = (_rapidFrames % (period * 2)) < period;
            btn_set("XB1_RT", fire ? 100 : 0);
        } else { _rapidFrames = 0; }

        // Recoil compensation (AK/M249/LR-300 patterns)
        if (btn_held("XB1_RT") && mod_enabled("anti_recoil")) {
            _recoilFrames++;
            int vert = cfg_get_int("rust_recoil_v", 18);
            int horiz = cfg_get_int("rust_recoil_h", 0);
            // Rust recoil is mostly vertical with slight right drift
            int rx = stick_x("RIGHT");
            int ry = stick_y("RIGHT") + vert * 280;
            stick_set("RIGHT", rx + horiz * 60, ry);
        } else { _recoilFrames = 0; }

        // Auto-crouch while shooting (harder to hit)
        if (btn_held("XB1_RT") && mod_enabled("crouch_spam")) {
            _recoilFrames++;
            bool crouch = (_recoilFrames % 10) < 5;
            btn_set("XB1_B", crouch ? 100 : 0);
        }

        // Auto-heal (hold Y)
        if (btn_held("XB1_Y") && mod_enabled("auto_heal")) {
            btn_set("XB1_Y", 100);
        }
    }
}
