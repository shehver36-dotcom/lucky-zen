using SoftwareZen;
using System;

public class NBA2K26Script : GameScript {
    // ── Auto Green ───────────────────────────────
    private bool _shotActive = false;
    private bool _shotDone = false;
    private int _shotFrames = 0;

    // ── Dribble combo state ──────────────────────
    private int _comboFrames = 0;
    private int _comboStep = 0;
    private bool _comboActive = false;
    private string _activeCombo = "";

    // ── Defensive state ──────────────────────────
    private int _stealFrames = 0;

    // ── Speed boost state ────────────────────────
    private int _boostFrames = 0;
    private bool _boostActive = false;

    // ── Post move state ──────────────────────────
    private int _postFrames = 0;
    private int _postStep = 0;
    private bool _postActive = false;

    public override void OnFrame() {
        // ══════════════════════════════════════════
        // Auto Green / Perfect Release
        // ══════════════════════════════════════════
        if (mod_enabled("auto_green")) {
            bool shotPressed = btn_pressed("XB1_X");
            bool shooting = btn_held("XB1_X");

            // Tap detection — catch the press and extend the hold
            if (shotPressed && !_shotActive) {
                _shotActive = true;
                _shotDone = false;
                _shotFrames = 0;
            }

            if (_shotActive && !_shotDone) {
                _shotFrames++;
                btn_set("XB1_X", 100);  // force hold the button

                double totalHoldMs = 0;

                if (mod_enabled("adaptive_timing")) {
                    bool isFade = Math.Abs(stick_y("LEFT")) > 15000;
                    bool isMoving = Math.Abs(stick_x("LEFT")) > 8000 || Math.Abs(stick_y("LEFT")) > 8000;
                    bool isSprinting = btn_held("XB1_RT");

                    if (isFade) {
                        totalHoldMs = cfg_get("green_fade_ms", 580.0);
                    } else if (isSprinting && isMoving) {
                        totalHoldMs = cfg_get("green_pullup_ms", 520.0);
                    } else if (isMoving) {
                        totalHoldMs = cfg_get("green_moving_ms", 505.0);
                    } else {
                        totalHoldMs = cfg_get("green_stand_ms", 500.0);
                    }

                    if (btn_held("XB1_LT")) {
                        totalHoldMs += cfg_get("green_contested_offset", 35.0);
                    }
                } else {
                    totalHoldMs = cfg_get("green_stand_ms", 500.0);
                }

                int targetFrames = (int)(totalHoldMs / 2.0);

                if (_shotFrames >= targetFrames) {
                    btn_set("XB1_X", 0);
                    _shotDone = true;
                }
            }

            // Keep X suppressed after release until user lets go
            if (_shotDone) {
                btn_set("XB1_X", 0);
            }

            // Reset only when user physically releases the button
            if (!shooting && _shotDone) {
                _shotActive = false;
                _shotDone = false;
            }
        }

        // ══════════════════════════════════════════
        // Auto Fadeaway (separate toggle, stronger control)
        // ══════════════════════════════════════════
        if (mod_enabled("auto_fade") && !mod_enabled("adaptive_timing")) {
            bool shooting = btn_held("XB1_X");
            bool fading = Math.Abs(stick_y("LEFT")) > 15000 && Math.Abs(stick_x("LEFT")) < 5000;

            if (shooting && fading && !_shotActive) {
                _shotActive = true;
                _shotDone = false;
                _shotFrames = 0;
            }

            if (_shotActive && !_shotDone && fading) {
                _shotFrames++;
                btn_set("XB1_X", 100);
                double fadeMs = cfg_get("fade_timing_ms", 580.0);
                int targetFrames = (int)(fadeMs / 2.0);

                if (_shotFrames >= targetFrames) {
                    btn_set("XB1_X", 0);
                    _shotDone = true;
                }
            }

            if (_shotDone) {
                btn_set("XB1_X", 0);
            }

            if (!shooting && _shotDone) {
                _shotActive = false;
                _shotDone = false;
            }
        }

        // ══════════════════════════════════════════
        // Dribble Combos
        // ══════════════════════════════════════════
        if (mod_enabled("dribble_god") && !_postActive) {
            // Momentum crossover: RS right then RS left (tap RS right + RS left in sequence)
            bool crossTrigger = btn_pressed("XB1_RB") && Math.Abs(stick_x("LEFT")) < 5000;

            // Behind the back: double-tap X while holding RS down
            bool btbTrigger = btn_pressed("XB1_X") && Math.Abs(stick_y("LEFT")) > 20000 && Math.Abs(stick_x("LEFT")) < 5000;

            // Spin move: rotate RS from center to edge
            bool spinTrigger = btn_pressed("XB1_B") && btn_held("XB1_RT");

            // Step-back: RS down while sprinting + shoot
            bool stepbackTrigger = btn_held("XB1_RT") && btn_pressed("XB1_LS") && btn_pressed("XB1_X");

            // Hesitation (size-up): tap RT rapidly
            bool sizeUpTrigger = btn_pressed("XB1_RT") && Var<int>("rt_taps") > 0;

            if (crossTrigger && !_comboActive) {
                StartCombo("momentum_cross", cfg_get_int("combo_speed", 8));
            }
            if (btbTrigger && !_comboActive) {
                StartCombo("behind_back", cfg_get_int("combo_speed", 7));
            }
            if (spinTrigger && !_comboActive) {
                StartCombo("spin_move", cfg_get_int("combo_speed", 10));
            }
            if (stepbackTrigger && !_comboActive) {
                StartCombo("stepback", cfg_get_int("combo_speed", 12));
            }
            if (sizeUpTrigger && !_comboActive) {
                StartCombo("sizeup", cfg_get_int("combo_speed", 6));
            }

            // Track RT taps for size-up detection
            if (btn_pressed("XB1_RT")) {
                int taps = Var<int>("rt_taps") + 1;
                SetVar("rt_taps", taps);
                SetVar("rt_tap_timer", 0);
            }
            int tapTimer = Var<int>("rt_tap_timer") + 1;
            SetVar("rt_tap_timer", tapTimer);
            if (tapTimer > 20) {
                SetVar("rt_taps", 0);
                SetVar("rt_tap_timer", 0);
            }

            ProcessCombo();
        }

        // ══════════════════════════════════════════
        // Speed Boost (animation cancel burst)
        // ══════════════════════════════════════════
        if (mod_enabled("speed_boost")) {
            // Trigger: RS flick + RT held → rapid LS flick in movement direction
            bool movingForward = stick_y("LEFT") > 18000 && Math.Abs(stick_x("LEFT")) < 8000;
            bool sprinting = btn_held("XB1_RT");

            if (movingForward && sprinting && btn_pressed("XB1_LS")) {
                _boostActive = true;
                _boostFrames = 0;
            }

            if (_boostActive) {
                _boostFrames++;
                // Boost sequence: release sprint → flick LS forward → re-engage sprint
                if (_boostFrames <= 2) {
                    btn_set("XB1_RT", 0);
                    stick_set("LEFT", 0, -32768); // max forward
                } else if (_boostFrames <= 4) {
                    stick_set("LEFT", 0, 32768);  // snap back
                } else if (_boostFrames <= 6) {
                    btn_set("XB1_RT", 100);
                    stick_set("LEFT", 0, 32768);
                } else {
                    _boostActive = false;
                }
            }
        }

        // ══════════════════════════════════════════
        // Auto Steal (spam steal button defensively)
        // ══════════════════════════════════════════
        if (mod_enabled("auto_steal") && btn_held("XB1_LT")) {
            _stealFrames++;
            int interval = cfg_get_int("steal_interval_ms", 120) / 2;
            if (_stealFrames % interval < interval / 2) {
                btn_set("XB1_X", 100);
            } else {
                btn_set("XB1_X", 0);
            }
        } else {
            _stealFrames = 0;
        }

        // ══════════════════════════════════════════
        // Auto Block (perfectly timed block attempts)
        // ══════════════════════════════════════════
        if (mod_enabled("auto_block") && btn_held("XB1_LT")) {
            // Rapid Y press when in defensive stance near shooter
            int blockInterval = cfg_get_int("block_interval_ms", 80) / 2;
            if (_stealFrames % blockInterval == 0) {
                btn_set("XB1_Y", 100);
            } else if (_stealFrames % blockInterval == blockInterval / 2) {
                btn_set("XB1_Y", 0);
            }
        }

        // ══════════════════════════════════════════
        // Post Move Combos
        // ══════════════════════════════════════════
        if (mod_enabled("post_moves") && !_comboActive) {
            bool inPost = btn_held("XB1_LT") && !btn_held("XB1_RT");

            if (inPost) {
                bool dropStep = btn_pressed("XB1_X") && Math.Abs(stick_x("LEFT")) > 18000;
                bool postSpin = btn_pressed("XB1_B") && Math.Abs(stick_x("LEFT")) > 18000;
                bool upAndUnder = btn_pressed("XB1_X") && Math.Abs(stick_y("LEFT")) > 18000;

                if (dropStep && !_postActive) {
                    _postActive = true;
                    _postFrames = 0;
                    _postStep = 1; // drop step sequence
                }
                if (postSpin && !_postActive) {
                    _postActive = true;
                    _postFrames = 0;
                    _postStep = 2; // post spin sequence
                }
                if (upAndUnder && !_postActive) {
                    _postActive = true;
                    _postFrames = 0;
                    _postStep = 3; // up and under sequence
                }
            }

            ProcessPostMove();
        }

        // ══════════════════════════════════════════
        // Quick Stop Pull-Up
        // ══════════════════════════════════════════
        if (mod_enabled("quick_stop")) {
            bool sprinting = btn_held("XB1_RT");
            bool shooting = btn_pressed("XB1_X");

            if (sprinting && shooting) {
                // Release sprint → pull back LS → shoot
                btn_set("XB1_RT", 0);
                if (_holdFrames <= 3) {
                    stick_set("LEFT", 0, -20000); // pull back for stop
                }
            }
        }

        // ══════════════════════════════════════════
        // Auto Rebound (jump timing optimization)
        // ══════════════════════════════════════════
        if (mod_enabled("auto_rebound") && btn_held("XB1_LT")) {
            bool nearHoop = Math.Abs(stick_x("LEFT")) < 5000 && Math.Abs(stick_y("LEFT")) < 5000;
            if (nearHoop) {
                int reboundInterval = cfg_get_int("rebound_interval_ms", 300) / 2;
                if (_stealFrames % reboundInterval == 0) {
                    btn_set("XB1_Y", 100);
                } else if (_stealFrames % reboundInterval == 2) {
                    btn_set("XB1_Y", 0);
                }
            }
        }
    }

    // ═════════════════════════════════════════════
    // Combo Engine
    // ═════════════════════════════════════════════
    private void StartCombo(string name, int speed) {
        _comboActive = true;
        _activeCombo = name;
        _comboFrames = 0;
        _comboStep = 0;
        // speed controls how many frames between each step
        SetVar("_combo_speed", speed);
    }

    private void ProcessCombo() {
        if (!_comboActive) return;
        _comboFrames++;
        int speed = Var<int>("_combo_speed");
        int frame = _comboFrames / Math.Max(speed, 1);
        int step = frame % 16; // wrap around for sub-step tracking

        switch (_activeCombo) {
            case "momentum_cross":
                // RS flick right → pause → RS flick left
                if (step == 0)      stick_set("RIGHT", 32767, 0);
                else if (step == 2) stick_set("RIGHT", 0, 0);
                else if (step == 4) stick_set("RIGHT", -32767, 0);
                else if (step == 6) stick_set("RIGHT", 0, 0);
                else if (step >= 8) { _comboActive = false; }
                break;

            case "behind_back":
                // RS down-right → RS down-left
                if (step == 0)      stick_set("RIGHT", 23170, 23170);
                else if (step == 2) stick_set("RIGHT", 0, 0);
                else if (step == 3) stick_set("RIGHT", -23170, 23170);
                else if (step == 5) stick_set("RIGHT", 0, 0);
                else if (step >= 7) { _comboActive = false; }
                break;

            case "spin_move":
                // RS rotate: right → down → left (semicircle spin)
                if (step == 0)      stick_set("RIGHT", 32767, 0);
                else if (step == 1) stick_set("RIGHT", 23170, 23170);
                else if (step == 2) stick_set("RIGHT", 0, 32767);
                else if (step == 3) stick_set("RIGHT", -23170, 23170);
                else if (step == 4) stick_set("RIGHT", -32767, 0);
                else if (step == 6) stick_set("RIGHT", 0, 0);
                else if (step >= 8) { _comboActive = false; }
                break;

            case "stepback":
                // RS down → hold → shoot
                if (step == 0)      { stick_set("RIGHT", 0, 32767); btn_set("XB1_RT", 0); }
                else if (step == 3) { btn_set("XB1_X", 100); }
                else if (step >= 4) { _comboActive = false; }
                break;

            case "sizeup":
                // RT tap sequence for size-up hesitation
                if (step == 0)      btn_set("XB1_RT", 100);
                else if (step == 1) btn_set("XB1_RT", 0);
                else if (step == 2) btn_set("XB1_RT", 100);
                else if (step == 3) btn_set("XB1_RT", 0);
                else if (step >= 4) { _comboActive = false; }
                break;
        }
    }

    // ═════════════════════════════════════════════
    // Post Move Engine
    // ═════════════════════════════════════════════
    private void ProcessPostMove() {
        if (!_postActive) return;
        _postFrames++;
        int speed = cfg_get_int("post_move_speed", 6);
        int frame = _postFrames / Math.Max(speed, 1);

        switch (_postStep) {
            case 1: // Drop step
                if (frame == 0)      stick_set("LEFT", 32767, -32767); // LS toward hoop + direction
                else if (frame == 1) { btn_set("XB1_X", 100); }
                else if (frame == 2) { btn_set("XB1_X", 0); _postActive = false; }
                break;

            case 2: // Post spin
                // Rotate LS in half-circle
                if (frame == 0)      stick_set("LEFT", 32767, 0);
                else if (frame == 1) stick_set("LEFT", 0, -32767);
                else if (frame == 2) stick_set("LEFT", -32767, 0);
                else if (frame == 3) { btn_set("XB1_X", 100); }
                else if (frame == 4) { btn_set("XB1_X", 0); _postActive = false; }
                break;

            case 3: // Up and under (pump fake then shoot)
                if (frame == 0)      btn_set("XB1_X", 100);  // pump
                else if (frame == 1) btn_set("XB1_X", 0);
                else if (frame == 2) btn_set("XB1_X", 100);  // real shot
                else if (frame == 3) btn_set("XB1_X", 0);
                else if (frame >= 4) _postActive = false;
                break;
        }
    }
}
