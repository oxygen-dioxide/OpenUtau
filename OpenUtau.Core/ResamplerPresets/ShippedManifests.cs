namespace OpenUtau.Core.ResamplerPresets {
    public static class ShippedManifests {
        //Shipped manifest presets for resamplers
        //Only resamplers that are commonly used and don't update should be added here.
        //It's always better to ship the manifest with your resampler than add it here.

        //To Add a manifest, add a new case here and add the manifest file to OpenUtau.Core.Classic.Data.Resources
        public static bool TryGet(string name, out byte[] manifestBytes) {
            switch (name) {
                case "moresampler":
                    manifestBytes = G2p.Data.Resources.moresampler;
                    return true;
                default:
                    manifestBytes = null;
                    return false;
            }
        }
    }
}
