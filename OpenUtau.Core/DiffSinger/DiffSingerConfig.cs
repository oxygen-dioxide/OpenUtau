﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenUtau.Core.DiffSinger {

    [Serializable]
    public class RandomPitchShifting {
        public float[] range;
    }

    [Serializable]
    public class AugmentationArgs {
        public RandomPitchShifting randomPitchShifting;
    }

    [Serializable]
    public class DsConfig {
        public string phonemes = "phonemes.txt";
        public string acoustic;
        public string vocoder;
        public List<string> speakers;
        public int hiddenSize = 256;
        public bool useKeyShiftEmbed = false;
        public bool useSpeedEmbed = false;
        public AugmentationArgs augmentationArgs;
    }
}
