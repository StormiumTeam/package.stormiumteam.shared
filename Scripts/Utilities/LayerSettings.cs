using System;
using System.Collections;
using System.Collections.Generic;
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

        private static Queue<LayerInfo> m_Layers = new Queue<LayerInfo>();
        
        public static int RegisterLayer(string layerName)
        {
            foreach (var layer in m_Layers)
            {
                if (layer.Name == layerName)
                    return layer.Index;
            }

            if (m_Layers.Count >= 22)
            {
                throw new IndexOutOfRangeException();
            }
            
            m_Layers.Enqueue(new LayerInfo()
            {
                Name = layerName,
                Index = m_Layers.Count
            });

            return m_Layers.Count - 1;
        }
    }
}