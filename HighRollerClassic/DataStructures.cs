using System.Collections.Generic;
using System.Numerics;

namespace HighRollerClassic;

public class DataStructures {
    public enum Comparators {
        Equal = 0,
        NotEqual = 1
    }

    public class MacroCheckbox {
        public bool Preview = false;
        public bool Yell = false;
    }

    public class MpSettings {
        public Vector4 colour = new();
        public Comparators comparator = Comparators.NotEqual;
        public int multiplier = 0;
        public int roll = 0;
    }

    public struct PlayerData() {
        public int bet = 0;
        public List<int> rolls = new();
        public List<int> bets = new();
        public List<int> multipliers = new();
    }
}
