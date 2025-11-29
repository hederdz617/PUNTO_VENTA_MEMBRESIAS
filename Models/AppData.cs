using System;
using DPFP;

namespace NuevoAPPwindowsforms.Models
{
    public class AppData
    {
        public const int MaxFingers = 10;
        public int EnrolledFingersMask = 0;
        public int MaxEnrollFingerCount = MaxFingers;
        public bool IsEventHandlerSucceeds = true;
        public bool IsFeatureSetMatched = false;
        public int FalseAcceptRate = 0;
        public DPFP.Template[] Templates = new DPFP.Template[MaxFingers];

        public delegate void OnChangeHandler();
        public event OnChangeHandler OnChange;
        public void Update() { OnChange?.Invoke(); }
    }
}
