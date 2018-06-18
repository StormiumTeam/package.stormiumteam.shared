using System;
using System.Collections.ObjectModel;

namespace Scripts.Utilities
{
    public struct LayerInfo
    {
        public string Name;
        public int Index;
    }
    
    public static class LayerSettings
    {
        public static ReadOnlyCollection<LayerInfo> Layers;
        
        public static int RegisterLayer(string layerName)
        {
            throw new NotImplementedException();
        }
    }
}