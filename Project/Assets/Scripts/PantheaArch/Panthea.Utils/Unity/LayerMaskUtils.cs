using UnityEngine;

namespace Panthea.Utils
{
    public static class LayerMaskUtils
    {
        public static int GetMaskLayer(this LayerMask layerMask) {

            for(int i = 0; i < 32; i++) {

                int value = 1 << i;
    
                if((layerMask & value) == value)
                    return i;
            }
            return 0;
        }
    }
}