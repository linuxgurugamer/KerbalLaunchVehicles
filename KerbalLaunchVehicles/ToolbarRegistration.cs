using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolbarControl_NS;
using UnityEngine;
//using KSP_Log;



namespace KerbalLaunchVehicles
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
#if false
        public static Log Log = null;
        public static void InitLog()
        {
            if (Log == null)
#if DEBUG
                Log = new Log("KerbalLaunchVehicles", Log.LEVEL.INFO);
#else
          Log = new Log("KerbalLaunchVehicles", Log.LEVEL.ERROR);
#endif
        }

        void Awake()
        {
            InitLog();
        }
#endif
        void Start()
        {
            ToolbarControl.RegisterMod(klvGUI.WindowDefault .MODID, klvGUI.WindowDefault.MODNAME);
        }
    }
}
