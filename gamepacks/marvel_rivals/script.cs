using SoftwareZen;
using System;

public class MarvelRivalsScript : GameScript {
    // Marvel Rivals — aim assist, ability combos, auto-melee
    private int _aimFrames = 0;
    private int _comboFrames = 0;
    private bool _comboActive = false;
    private int _comboStep = 0;
    private string _currentCombo = "";

    public override void OnFrame() {
        // Aim Assist — micro-adjust while ADS (LT)
        if (btn_held("XB1_LT") && mod_enabled("aim_assist")) {
            _aimFrames++;
            int strength = cfg_get_int("mr_aa_strength", 4);
            int dir = (_aimFrames / 10) % 2 == 0 ? 1 : -1;
            stick_set("RIGHT", stick_x("RIGHT") + dir * strength * 400, stick_y("RIGHT"));
        }

        // Ability combo 1: RB -> LT -> RT (burst damage rotation)
        if (btn_pressed("XB1_RB") && mod_enabled("ability_combo")) {
            StartCombo("burst");
        }

        // Ability combo 2: LB -> X -> RT (ultimate setup)
        if (btn_pressed("XB1_LB") && mod_enabled("ability_combo")) {
            StartCombo("ult_setup");
        }

        ProcessCombo();

        // Auto-melee when close range (LS neutral = engaged in close combat)
        if (mod_enabled("auto_melee")) {
            bool closeRange = Math.Abs(stick_x("LEFT")) < 8000 && Math.Abs(stick_y("LEFT")) < 8000;
            if (closeRange && btn_held("XB1_RT")) {
                int meleeInterval = cfg_get_int("mr_melee_ms", 300) / 2;
                if (_aimFrames % meleeInterval == 0) btn_set("XB1_RS", 100);
                else if (_aimFrames % meleeInterval == 2) btn_set("XB1_RS", 0);
            }
        }

        // Rapid melee cancel (animation cancel tech)
        if (btn_pressed("XB1_RS") && mod_enabled("melee_cancel")) {
            btn_set("XB1_RS", 100);
            // Brief pause then cancel with jump
        }
    }

    private void StartCombo(string name) {
        _comboActive = true; _comboFrames = 0; _comboStep = 0; _currentCombo = name;
    }

    private void ProcessCombo() {
        if (!_comboActive) return;
        _comboFrames++;
        int speed = cfg_get_int("mr_combo_speed", 6);
        int step = _comboFrames / Math.Max(speed, 1);

        if (_currentCombo == "burst") {
            if (step == 0)      btn_set("XB1_LT", 100);
            else if (step == 1) btn_set("XB1_RT", 100);
            else if (step == 3) { btn_set("XB1_LT", 0); btn_set("XB1_RT", 0); _comboActive = false; }
        } else if (_currentCombo == "ult_setup") {
            if (step == 0)      btn_set("XB1_X", 100);
            else if (step == 1) { btn_set("XB1_X", 0); btn_set("XB1_RT", 100); }
            else if (step == 3) { btn_set("XB1_RT", 0); _comboActive = false; }
        }
    }
}
